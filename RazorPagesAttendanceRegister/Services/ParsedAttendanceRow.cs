namespace RazorPagesAttendanceRegister.Services
{
    /// <summary>
    /// Represents one attendance row parsed from an import file.
    /// </summary>
    public class ParsedAttendanceRow
    {
        /// <summary>
        /// Gets or sets the student's number used to match an attendance user.
        /// </summary>
        public string StudentNumber { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the date of the lecture.
        /// </summary>
        public DateOnly LectureDate { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the student was present.
        /// </summary>
        public bool WasPresent { get; set; }
    }
}
