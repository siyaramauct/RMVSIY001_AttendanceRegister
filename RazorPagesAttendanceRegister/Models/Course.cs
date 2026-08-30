namespace RazorPagesAttendanceRegister.Models
{
    /// <summary>
    /// Represents a course that is taught within the attendance register system.
    /// </summary>
    public class Course
    {
        /// <summary>
        /// Gets or sets the unique identifier for the course.
        /// This is an identity/autoincrement integer primary key.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the unique course code used to identify the course.
        /// This value is required.
        /// </summary>
        public required string Code { get; set; }

        /// <summary>
        /// Gets or sets the human-readable name of the course.
        /// This value is required.
        /// </summary>
        public required string Name { get; set; }

        /// <summary>
        /// Gets or sets the identifier of the lecturer who owns the course.
        /// This is a nullable foreign key to <see cref="AttendanceUser.Id"/>.
        /// </summary>
        public string? LecturerId { get; set; }

        /// <summary>
        /// Gets or sets the lectures associated with this course.
        /// </summary>
        public List<Lecture> Lectures { get; set; } = new();
    }
}
