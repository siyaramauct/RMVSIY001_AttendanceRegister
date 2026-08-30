namespace RazorPagesAttendanceRegister.Services
{
    /// <summary>
    /// Represents the outcome of an attendance operation.
    /// </summary>
    public class AttendanceResult
    {
        /// <summary>
        /// Gets or sets a value indicating whether the attendance action succeeded.
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// Gets or sets a message describing the result of the attendance action.
        /// </summary>
        public string Message { get; set; } = string.Empty;
    }
}
