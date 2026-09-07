namespace RazorPagesAttendanceRegister.Models
{
    /// <summary>
    /// Identifies how an attendance record was created or last updated.
    /// </summary>
    public enum AttendanceMethod
    {
        /// <summary>
        /// Indicates that a lecturer or the attendance workflow entered the record manually.
        /// </summary>
        Manual,

        /// <summary>
        /// Indicates that the record was created or updated from an Excel import.
        /// </summary>
        ExcelImport
    }
}
