using RazorPagesAttendanceRegister.Models;

namespace RazorPagesAttendanceRegister.Services
{
    /// <summary>
    /// Summarizes attendance statuses for one lecture date.
    /// </summary>
    public class LectureAttendanceSummaryDto
    {
        /// <summary>
        /// Gets or sets the lecture date represented by the counts below.
        /// </summary>
        public DateOnly LectureDate { get; set; }

        /// <summary>
        /// Gets or sets the number of Present records for the lecture date.
        /// </summary>
        public int PresentCount { get; set; }

        /// <summary>
        /// Gets or sets the number of Absent records for the lecture date.
        /// </summary>
        public int AbsentCount { get; set; }

        /// <summary>
        /// Gets or sets the number of Late records for the lecture date.
        /// </summary>
        public int LateCount { get; set; }

        /// <summary>
        /// Gets or sets the number of Excused records for the lecture date.
        /// </summary>
        public int ExcusedCount { get; set; }
    }
}
