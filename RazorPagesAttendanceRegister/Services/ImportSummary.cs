namespace RazorPagesAttendanceRegister.Services
{
    /// <summary>
    /// Summarizes the outcome of importing attendance data.
    /// </summary>
    public class ImportSummary
    {
        /// <summary>
        /// Gets or sets the number of attendance records created.
        /// </summary>
        public int RecordsCreated { get; set; }

        /// <summary>
        /// Gets or sets the number of attendance records updated.
        /// </summary>
        public int RecordsUpdated { get; set; }

        /// <summary>
        /// Gets or sets the number of lectures created automatically from the import.
        /// </summary>
        public int LecturesAutoCreated { get; set; }

        /// <summary>
        /// Gets or sets student numbers that could not be matched to users.
        /// </summary>
        public List<string> UnmatchedStudentNumbers { get; set; } = new();

        /// <summary>
        /// Gets or sets a value indicating whether the import succeeded.
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// Gets or sets a message describing the import result.
        /// </summary>
        public string Message { get; set; } = string.Empty;
    }
}
