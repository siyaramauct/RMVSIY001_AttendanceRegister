using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using RazorPagesAttendanceRegister.Models;
using RazorPagesAttendanceRegister.Services;

namespace RazorPagesAttendanceRegister.Pages.Lecturer
{
    [Authorize(Roles = "Lecturer")]
    public class ManageQueriesModel : PageModel
    {
        private readonly IAttendanceService _attendanceService;

        public ManageQueriesModel(IAttendanceService attendanceService)
        {
            _attendanceService = attendanceService;
        }

        public List<AttendanceQuery> PendingQueries { get; private set; } = new();

        public string? StatusMessage { get; private set; }

        public bool IsSuccess { get; private set; }

        public async Task OnGetAsync()
        {
            StatusMessage = TempData["StatusMessage"] as string;
            IsSuccess = bool.TryParse(TempData["IsSuccess"] as string, out var isSuccess) && isSuccess;

            var lecturerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(lecturerId))
            {
                return;
            }

            PendingQueries = await _attendanceService.GetPendingQueriesForLecturerAsync(lecturerId);
        }

        public async Task<IActionResult> OnPostResolveAsync(int queryId, bool approve, string response)
        {
            var lecturerId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            AttendanceResult result;
            if (string.IsNullOrWhiteSpace(lecturerId))
            {
                result = new AttendanceResult
                {
                    Success = false,
                    Message = "You must be signed in to resolve an attendance query."
                };
            }
            else
            {
                result = await _attendanceService.ResolveQueryAsync(lecturerId, queryId, approve, response);
            }

            TempData["StatusMessage"] = result.Message;
            TempData["IsSuccess"] = result.Success.ToString();
            return RedirectToPage();
        }
    }
}
