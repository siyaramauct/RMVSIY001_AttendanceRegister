using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using RazorPagesAttendanceRegister.Models;
using RazorPagesAttendanceRegister.Services;

namespace RazorPagesAttendanceRegister.Pages.Student
{
    [Authorize(Roles = "Student")]
    public class RecordAttendanceModel : PageModel
    {
        private readonly IAttendanceService _attendanceService;

        public RecordAttendanceModel(IAttendanceService attendanceService)
        {
            _attendanceService = attendanceService;
        }

        public List<Lecture> ActiveLectures { get; private set; } = new();

        public List<AttendanceRecordDisplayDto> PastAttendance { get; private set; } = new();

        public List<AttendanceQuery> StudentQueries { get; private set; } = new();

        public string? StatusMessage { get; private set; }

        public bool IsSuccess { get; private set; }

        public async Task OnGetAsync()
        {
            StatusMessage = TempData["StatusMessage"] as string;
            IsSuccess = bool.TryParse(TempData["IsSuccess"] as string, out var isSuccess) && isSuccess;

            var studentId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(studentId))
            {
                ActiveLectures = new List<Lecture>();
                PastAttendance = new List<AttendanceRecordDisplayDto>();
                StudentQueries = new List<AttendanceQuery>();
                return;
            }

            var lectures = await _attendanceService.GetLecturesForStudentAsync(studentId);
            var allHistory = await _attendanceService.GetAttendanceHistoryForStudentAsync(studentId);
            StudentQueries = await _attendanceService.GetQueriesForStudentAsync(studentId);

            var now = DateTime.UtcNow;

            ActiveLectures = lectures
                .Where(l =>
                {
                    var start = l.ScheduledDate.ToDateTime(l.StartTime, DateTimeKind.Utc);
                    var end = l.ScheduledDate.ToDateTime(l.EndTime, DateTimeKind.Utc);
                    var hasPresent = allHistory.Any(h => h.LectureId == l.Id && h.Status == AttendanceStatus.Present);
                    return now >= start && now <= end && !hasPresent;
                })
                .OrderBy(l => l.ScheduledDate)
                .ToList();

            PastAttendance = allHistory
                .OrderByDescending(h => h.LectureDate)
                .ToList();
        }

        public async Task<IActionResult> OnPostAsync(int lectureId)
        {
            var studentId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrWhiteSpace(studentId))
            {
                StatusMessage = "You must be signed in to record attendance.";
                IsSuccess = false;
                await OnGetAsync();
                return Page();
            }

            var result = await _attendanceService.RecordAttendanceAsync(studentId, lectureId);
            StatusMessage = result.Message;
            IsSuccess = result.Success;

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostRaiseQueryAsync(int attendanceRecordId, string reason)
        {
            var studentId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            AttendanceResult result;
            if (string.IsNullOrWhiteSpace(studentId))
            {
                result = new AttendanceResult
                {
                    Success = false,
                    Message = "You must be signed in to raise an attendance query."
                };
            }
            else
            {
                result = await _attendanceService.RaiseQueryAsync(studentId, attendanceRecordId, reason);
            }

            TempData["StatusMessage"] = result.Message;
            TempData["IsSuccess"] = result.Success.ToString();
            return RedirectToPage();
        }
    }
}
