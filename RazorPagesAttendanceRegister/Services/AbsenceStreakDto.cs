using System;

namespace RazorPagesAttendanceRegister.Services
{
    /// <summary>
    /// Represents a student's current consecutive absence streak for a course.
    /// </summary>
    public class AbsenceStreakDto
    {
        /// <summary>
        /// Gets or sets the Identity identifier of the student.
        /// </summary>
        public string StudentId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the student's display name, composed from their name and surname.
        /// </summary>
        public string StudentName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the number of newest consecutive records marked Absent.
        /// </summary>
        public int CurrentStreak { get; set; }

        /// <summary>
        /// Gets or sets the date of the student's most recent Present or Excused record.
        /// </summary>
        public DateOnly? LastAttendedDate { get; set; }
    }
}
