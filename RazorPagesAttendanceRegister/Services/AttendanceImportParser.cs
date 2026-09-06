using System.Globalization;
using ClosedXML.Excel;

namespace RazorPagesAttendanceRegister.Services
{
    /// <summary>
    /// Parses attendance workbooks using the expected student/date matrix layout.
    /// </summary>
    public class AttendanceImportParser : IAttendanceImportParser
    {
        /// <inheritdoc />
        public Task<ImportParseResult> ParseAsync(Stream fileStream)
        {
            var result = new ImportParseResult();

            if (fileStream == null || !fileStream.CanRead)
            {
                result.StructuralErrors.Add("The attendance file could not be read.");
                return Task.FromResult(result);
            }

            try
            {
                using var workbook = new XLWorkbook(fileStream);
                var worksheet = workbook.Worksheets.FirstOrDefault();
                if (worksheet == null)
                {
                    result.StructuralErrors.Add("The attendance file does not contain a worksheet.");
                    return Task.FromResult(result);
                }

                var usedRange = worksheet.RangeUsed();
                if (usedRange == null)
                {
                    result.StructuralErrors.Add("The attendance file is empty.");
                    return Task.FromResult(result);
                }

                var firstColumn = usedRange.RangeAddress.FirstAddress.ColumnNumber;
                var lastColumn = usedRange.RangeAddress.LastAddress.ColumnNumber;
                var firstRow = usedRange.RangeAddress.FirstAddress.RowNumber;
                var lastRow = usedRange.RangeAddress.LastAddress.RowNumber;

                if (firstRow > 1 || lastColumn < 3)
                {
                    result.StructuralErrors.Add("The attendance file header row could not be parsed. Expected student details in columns A-B and lecture dates starting from column C.");
                    return Task.FromResult(result);
                }

                var lectureDates = new Dictionary<int, DateOnly>();
                for (var column = 3; column <= lastColumn; column++)
                {
                    var headerCell = worksheet.Cell(1, column);
                    DateOnly lectureDate;
                    var dateParsed = false;

                    if (headerCell.TryGetValue<DateTime>(out var dt))
                    {
                        lectureDate = DateOnly.FromDateTime(dt);
                        dateParsed = true;
                    }
                    else
                    {
                        var headerText = headerCell.GetString().Trim();
                        if (DateOnly.TryParseExact(
                                headerText,
                                new[] { "yyyy/MM/dd", "yyyy-MM-dd" },
                                CultureInfo.InvariantCulture,
                                DateTimeStyles.None,
                                out lectureDate))
                        {
                            dateParsed = true;
                        }
                    }

                    if (!dateParsed)
                    {
                        var headerText = headerCell.GetString().Trim();
                        result.StructuralErrors.Add(
                            $"The date header in row 1, column {headerCell.Address.ColumnLetter} could not be parsed: '{headerText}'. Expected format: yyyy/MM/dd.");
                        return Task.FromResult(result);
                    }

                    lectureDates[column] = lectureDate;
                }

                for (var row = 2; row <= lastRow; row++)
                {
                    var rowCells = worksheet.Row(row).Cells(firstColumn, lastColumn);
                    if (rowCells.All(cell => string.IsNullOrWhiteSpace(cell.GetString())))
                    {
                        continue;
                    }

                    var studentNumber = worksheet.Cell(row, 2).GetString().Trim();
                    if (string.IsNullOrWhiteSpace(studentNumber))
                    {
                        continue;
                    }

                    foreach (var dateColumn in lectureDates)
                    {
                        var cell = worksheet.Cell(row, dateColumn.Key);
                        var valid = false;
                        var value = 0;

                        if (cell.TryGetValue<int>(out var intVal) && intVal is 0 or 1)
                        {
                            value = intVal;
                            valid = true;
                        }
                        else
                        {
                            var cellText = cell.GetString().Trim();
                            if (int.TryParse(cellText, NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsedInt) &&
                                parsedInt is 0 or 1)
                            {
                                value = parsedInt;
                                valid = true;
                            }
                            else if (double.TryParse(cellText, NumberStyles.Float, CultureInfo.InvariantCulture, out var parsedDouble) &&
                                     parsedDouble is 0.0 or 1.0)
                            {
                                value = (int)parsedDouble;
                                valid = true;
                            }
                        }

                        if (!valid)
                        {
                            var cellText = cell.GetString().Trim();
                            result.StructuralErrors.Add(
                                $"Invalid attendance value at row {row}, column {cell.Address.ColumnLetter}: '{cellText}'. Expected 1 for Present or 0 for Absent.");
                            continue;
                        }

                        result.Rows.Add(new ParsedAttendanceRow
                        {
                            StudentNumber = studentNumber,
                            LectureDate = dateColumn.Value,
                            WasPresent = value == 1
                        });
                    }
                }
            }
            catch (Exception exception)
            {
                result.StructuralErrors.Add($"The attendance file could not be read: {exception.Message}");
            }

            return Task.FromResult(result);
        }
    }
}
