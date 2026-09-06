using RazorPagesAttendanceRegister.Models;

namespace RazorPagesAttendanceRegister.Services
{
    /// <summary>
    /// Summarizes attendance statuses for one lecture date.
    /// </summary>
    public class LectureAttendanceSummaryDto
    {
        public DateOnly LectureDate { get; set; }
        public int PresentCount { get; set; }
        public int AbsentCount { get; set; }
        public int LateCount { get; set; }
        public int ExcusedCount { get; set; }
    }
}
