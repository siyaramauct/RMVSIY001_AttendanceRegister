using Microsoft.EntityFrameworkCore;
using RazorPagesAttendanceRegister.Data;
using RazorPagesAttendanceRegister.Models;

namespace RazorPagesAttendanceRegister.Services
{
    /// <summary>
    /// Implements attendance import operations for courses, lectures, and student records.
    /// </summary>
    public class AttendanceImportService : IAttendanceImportService
    {
        private readonly AttendanceDbContext _context;

        /// <summary>
        /// Initializes a new instance of the <see cref="AttendanceImportService"/> class.
        /// </summary>
        /// <param name="context">The database context used to query and persist attendance entities.</param>
        public AttendanceImportService(AttendanceDbContext context)
        {
            _context = context;
        }

        /// <inheritdoc />
        public async Task<ImportSummary> ImportAsync(string lecturerId, int courseId, List<ParsedAttendanceRow> rows)
        {
            var summary = new ImportSummary();

            // 1. Fetch the Course by courseId and verify ownership.
            if (string.IsNullOrWhiteSpace(lecturerId))
            {
                summary.Success = false;
                summary.Message = "You can only import attendance for your own courses.";
                return summary;
            }

            var course = await _context.Courses
                .FirstOrDefaultAsync(c => c.Id == courseId);

            if (course == null || course.LecturerId != lecturerId)
            {
                summary.Success = false;
                summary.Message = "You can only import attendance for your own courses.";
                return summary;
            }

            if (rows == null || rows.Count == 0)
            {
                summary.Success = true;
                summary.Message = "No attendance rows were provided for import.";
                return summary;
            }

            // 4. Wrap steps 2–3 in a single database transaction. Call SaveChangesAsync once at the end.
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // 2. Group the incoming rows by LectureDate.
                var distinctDates = rows.Select(r => r.LectureDate).Distinct().ToList();
                var lecturesByDate = new Dictionary<DateOnly, Lecture>();

                var existingLectures = await _context.Lectures
                    .Where(l => l.CourseId == courseId && distinctDates.Contains(l.ScheduledDate))
                    .ToListAsync();

                foreach (var existingLecture in existingLectures)
                {
                    lecturesByDate[existingLecture.ScheduledDate] = existingLecture;
                }

                foreach (var date in distinctDates)
                {
                    if (!lecturesByDate.ContainsKey(date))
                    {
                        // Placeholder times (09:00 - 10:00) since the spreadsheet only carries a date, not exact times.
                        var newLecture = new Lecture
                        {
                            CourseId = courseId,
                            ScheduledDate = date,
                            StartTime = new TimeOnly(9, 0),
                            EndTime = new TimeOnly(10, 0)
                        };

                        await _context.Lectures.AddAsync(newLecture);
                        lecturesByDate[date] = newLecture;
                        summary.LecturesAutoCreated++;
                    }
                }

                // 3. For each ParsedAttendanceRow:
                var studentNumbers = rows
                    .Select(r => r.StudentNumber)
                    .Where(sn => !string.IsNullOrWhiteSpace(sn))
                    .Distinct()
                    .ToList();

                var students = await _context.Users
                    .Where(u => u.StudentNumber != null && studentNumbers.Contains(u.StudentNumber))
                    .ToDictionaryAsync(u => u.StudentNumber!, u => u);

                var existingLectureIds = existingLectures.Select(l => l.Id).ToList();
                var existingRecords = await _context.AttendanceRecords
                    .Where(ar => existingLectureIds.Contains(ar.LectureId))
                    .ToListAsync();

                var recordDict = new Dictionary<(DateOnly, string), AttendanceRecord>();
                foreach (var record in existingRecords)
                {
                    var recLecture = existingLectures.FirstOrDefault(l => l.Id == record.LectureId);
                    if (recLecture != null)
                    {
                        recordDict[(recLecture.ScheduledDate, record.StudentId)] = record;
                    }
                }

                foreach (var row in rows)
                {
                    // Look up the AttendanceUser by StudentNumber.
                    if (string.IsNullOrWhiteSpace(row.StudentNumber) || !students.TryGetValue(row.StudentNumber, out var student))
                    {
                        if (!string.IsNullOrWhiteSpace(row.StudentNumber) && !summary.UnmatchedStudentNumbers.Contains(row.StudentNumber))
                        {
                            summary.UnmatchedStudentNumbers.Add(row.StudentNumber);
                        }
                        continue;
                    }

                    var lecture = lecturesByDate[row.LectureDate];
                    var status = row.WasPresent ? AttendanceStatus.Present : AttendanceStatus.Absent;

                    if (recordDict.TryGetValue((row.LectureDate, student.Id), out var existingRecord))
                    {
                        // If one exists, update its Status and set Method = AttendanceMethod.ExcelImport.
                        existingRecord.Status = status;
                        existingRecord.Method = AttendanceMethod.ExcelImport;
                        existingRecord.RecordedAt = DateTime.UtcNow;
                        summary.RecordsUpdated++;
                    }
                    else
                    {
                        // If none exists, create a new AttendanceRecord with Status, RecordedAt = UtcNow, Method = ExcelImport.
                        var newRecord = new AttendanceRecord
                        {
                            Lecture = lecture,
                            StudentId = student.Id,
                            Status = status,
                            Method = AttendanceMethod.ExcelImport,
                            RecordedAt = DateTime.UtcNow
                        };

                        await _context.AttendanceRecords.AddAsync(newRecord);
                        recordDict[(row.LectureDate, student.Id)] = newRecord;
                        summary.RecordsCreated++;
                    }
                }

                // Call SaveChangesAsync once at the end of the transaction.
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                // 5. Set Success=true and a Message summarizing counts.
                var totalRecords = summary.RecordsCreated + summary.RecordsUpdated;
                var unmatchedCount = summary.UnmatchedStudentNumbers.Count;
                var unmatchedMessage = unmatchedCount > 0
                    ? $", {unmatchedCount} student number{(unmatchedCount == 1 ? "" : "s")} not matched."
                    : ".";

                summary.Success = true;
                summary.Message = $"Imported {totalRecords} records ({summary.RecordsUpdated} updated), {summary.LecturesAutoCreated} lecture{(summary.LecturesAutoCreated == 1 ? "" : "s")} auto-created{unmatchedMessage}";

                return summary;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                summary.Success = false;
                summary.Message = $"An error occurred while importing attendance: {ex.Message}";
                return summary;
            }
        }
    }
}
