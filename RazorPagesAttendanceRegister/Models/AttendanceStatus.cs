namespace RazorPagesAttendanceRegister.Models
{
    /// <summary>
    /// Represents the attendance status recorded for a student during a lecture.
    /// </summary>
    public enum AttendanceStatus
    {
        /// <summary>
        /// The student was present for the lecture.
        /// </summary>
        Present,

        /// <summary>
        /// The student was absent from the lecture.
        /// </summary>
        Absent,

        /// <summary>
        /// The student arrived late to the lecture.
        /// </summary>
        Late,

        /// <summary>
        /// The student's absence was excused.
        /// </summary>
        Excused
    }
}
