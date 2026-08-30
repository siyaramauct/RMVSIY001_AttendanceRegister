using RazorPagesAttendanceRegister.Models;

namespace RazorPagesAttendanceRegister.Services
{
    /// <summary>
    /// Represents a student's attendance record in a display-friendly format for the UI.
    /// </summary>
    public class AttendanceRecordDisplayDto
    {
        /// <summary>
        /// Gets or sets the attendance record identifier.
        /// </summary>
        public int AttendanceRecordId { get; set; }

        /// <summary>
        /// Gets or sets the lecture identifier.
        /// </summary>
        public int LectureId { get; set; }

        /// <summary>
        /// Gets or sets the course code associated with the lecture.
        /// </summary>
        public string CourseCode { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the course name associated with the lecture.
        /// </summary>
        public string CourseName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the date of the lecture.
        /// </summary>
        public DateOnly LectureDate { get; set; }

        /// <summary>
        /// Gets or sets the status that was recorded for the lecture.
        /// </summary>
        public AttendanceStatus Status { get; set; }
    }
}
