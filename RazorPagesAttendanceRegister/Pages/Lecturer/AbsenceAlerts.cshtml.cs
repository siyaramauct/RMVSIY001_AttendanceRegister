using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using RazorPagesAttendanceRegister.Data;
using RazorPagesAttendanceRegister.Models;
using RazorPagesAttendanceRegister.Services;

namespace RazorPagesAttendanceRegister.Pages.Lecturer
{
    [Authorize(Roles = "Lecturer")]
    public class AbsenceAlertsModel : PageModel
    {
        private readonly AttendanceDbContext _context;
        private readonly IAttendanceService _attendanceService;

        public AbsenceAlertsModel(
            AttendanceDbContext context,
            IAttendanceService attendanceService)
        {
            _context = context;
            _attendanceService = attendanceService;
        }

        public List<Course> Courses { get; private set; } = new();
        public int? CourseId { get; private set; }
        public List<AbsenceStreakDto> AbsenceAlerts { get; private set; } = new();

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

            AbsenceAlerts = await _attendanceService
                .GetStudentsWithMissingStreakAsync(CourseId.Value);
        }
    }
}
