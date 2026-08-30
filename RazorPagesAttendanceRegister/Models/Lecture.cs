namespace RazorPagesAttendanceRegister.Models
{
    /// <summary>
    /// Represents a scheduled lecture session for a course.
    /// </summary>
    public class Lecture
    {
        /// <summary>
        /// Gets or sets the unique identifier for the lecture.
        /// This is an identity/autoincrement integer primary key.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the identifier of the course to which this lecture belongs.
        /// This is a foreign key to <see cref="Course.Id"/>.
        /// </summary>
        public int CourseId { get; set; }

        /// <summary>
        /// Gets or sets the course to which this lecture belongs.
        /// </summary>
        public Course? Course { get; set; }

        /// <summary>
        /// Gets or sets the date on which the lecture is scheduled.
        /// </summary>
        public DateOnly ScheduledDate { get; set; }

        /// <summary>
        /// Gets or sets the start time for the lecture.
        /// </summary>
        public TimeOnly StartTime { get; set; }

        /// <summary>
        /// Gets or sets the end time for the lecture.
        /// </summary>
        public TimeOnly EndTime { get; set; }
    }
}
