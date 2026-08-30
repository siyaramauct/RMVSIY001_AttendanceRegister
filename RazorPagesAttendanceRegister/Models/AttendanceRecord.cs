namespace RazorPagesAttendanceRegister.Models
{
    /// <summary>
    /// Represents a student's attendance record for a specific lecture.
    /// </summary>
    public class AttendanceRecord
    {
        /// <summary>
        /// Gets or sets the unique identifier for the attendance record.
        /// This is an identity/autoincrement integer primary key.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the identifier of the lecture for which attendance was recorded.
        /// This is a foreign key to <see cref="Lecture.Id"/>.
        /// </summary>
        public int LectureId { get; set; }

        /// <summary>
        /// Gets or sets the lecture for which attendance was recorded.
        /// </summary>
        public Lecture? Lecture { get; set; }

        /// <summary>
        /// Gets or sets the identifier of the student associated with this attendance record.
        /// This is a foreign key to <see cref="AttendanceUser.Id"/>.
        /// </summary>
        public string StudentId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the attendance status assigned to the student for the lecture.
        /// </summary>
        public AttendanceStatus Status { get; set; }

        /// <summary>
        /// Gets or sets the UTC timestamp when the attendance record was created.
        /// </summary>
        public DateTime RecordedAt { get; set; }
    }
}
