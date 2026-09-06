using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using RazorPagesAttendanceRegister.Data;
using RazorPagesAttendanceRegister.Models;
using RazorPagesAttendanceRegister.Services;

namespace RazorPagesAttendanceRegister.Pages.Lecturer
{
    [Authorize(Roles = "Lecturer")]
    public class OverviewModel : PageModel
    {
        private readonly AttendanceDbContext _context;
        private readonly IAttendanceService _attendanceService;

        public OverviewModel(AttendanceDbContext context, IAttendanceService attendanceService)
        {
            _context = context;
            _attendanceService = attendanceService;
        }

        public List<Course> Courses { get; private set; } = new();
        public int? CourseId { get; private set; }
        public List<LectureAttendanceSummaryDto> AttendanceOverview { get; private set; } = new();
        public string ChartDataJson { get; private set; } = "[]";

        public async Task OnGetAsync(int? courseId)
        {
            var lecturerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(lecturerId))
            {
                return;
            }

            Courses = await _context.Courses
                .Where(course => course.LecturerId == lecturerId)
                .OrderBy(course => course.Code)
                .ToListAsync();

            CourseId = courseId.HasValue && Courses.Any(course => course.Id == courseId.Value)
                ? courseId.Value
                : Courses.FirstOrDefault()?.Id;

            if (!CourseId.HasValue)
            {
                return;
            }

            AttendanceOverview = await _attendanceService
                .GetAttendanceOverviewAsync(lecturerId, CourseId.Value);

            var chartData = AttendanceOverview.Select(summary => new
            {
                LectureDate = summary.LectureDate.ToString("dd MMM"),
                summary.PresentCount,
                summary.AbsentCount,
                summary.LateCount,
                summary.ExcusedCount
            });

            ChartDataJson = JsonSerializer.Serialize(
                chartData,
                new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
        }
    }
}
