using System.ComponentModel.DataAnnotations;

namespace RazorPagesAttendanceRegister.Models
{
    /// <summary>
    /// Represents a student's query about an attendance record.
    /// </summary>
    public class AttendanceQuery
    {
        /// <summary>
        /// Gets or sets the unique identifier for the query.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the attendance record being queried.
        /// </summary>
        public int AttendanceRecordId { get; set; }

        /// <summary>
        /// Gets or sets the attendance record being queried.
        /// </summary>
        public AttendanceRecord? AttendanceRecord { get; set; }

        /// <summary>
        /// Gets or sets the student who submitted the query.
        /// </summary>
        [Required]
        public string StudentId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the student who submitted the query.
        /// </summary>
        public AttendanceUser? Student { get; set; }

        /// <summary>
        /// Gets or sets the reason for the query.
        /// </summary>
        [Required]
        [MaxLength(500)]
        public string Reason { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the review status of the query.
        /// </summary>
        public QueryStatus Status { get; set; } = QueryStatus.Pending;

        /// <summary>
        /// Gets or sets the lecturer's response, if provided.
        /// </summary>
        public string? LecturerResponse { get; set; }

        /// <summary>
        /// Gets or sets the UTC timestamp when the query was created.
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Gets or sets the UTC timestamp when the query was resolved, if resolved.
        /// </summary>
        public DateTime? ResolvedAt { get; set; }
    }
}
