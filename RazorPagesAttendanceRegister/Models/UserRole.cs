namespace RazorPagesAttendanceRegister.Models
{
    /// <summary>
    /// Defines the role assigned to a user in the attendance system.
    /// </summary>
    public enum UserRole
    {
        /// <summary>
        /// A student user who may be tracked in attendance records and matched to a spreadsheet record.
        /// </summary>
        Student,

        /// <summary>
        /// A lecturer user who manages or monitors attendance for classes or students.
        /// </summary>
        Lecturer
    }
}
