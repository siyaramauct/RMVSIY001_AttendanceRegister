namespace RazorPagesAttendanceRegister.Services
{
    /// <summary>
    /// Represents the result of parsing an attendance import file.
    /// </summary>
    public class ImportParseResult
    {
        /// <summary>
        /// Gets or sets the attendance rows parsed from the file.
        /// </summary>
        public List<ParsedAttendanceRow> Rows { get; set; } = new();

        /// <summary>
        /// Gets or sets file-level errors that prevented reliable parsing.
        /// These errors do not represent individual student data issues.
        /// </summary>
        public List<string> StructuralErrors { get; set; } = new();
    }
}
