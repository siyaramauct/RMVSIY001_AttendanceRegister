using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using RazorPagesAttendanceRegister.Data;
using RazorPagesAttendanceRegister.Models;
using Microsoft.EntityFrameworkCore;

namespace RazorPagesAttendanceRegister.Pages.Lecturer
{
    [Authorize(Roles = "Lecturer")]
    public class ViewAttendanceModel : PageModel
    {
        private readonly AttendanceDbContext _context;

        public ViewAttendanceModel(AttendanceDbContext context)
        {
            _context = context;
        }

        public List<CourseAttendanceSummary> CourseList { get; set; } = new();

        public class CourseAttendanceSummary
        {
            public int CourseId { get; set; }
            public string Code { get; set; } = string.Empty;
            public string Name { get; set; } = string.Empty;
            public int TotalLectures { get; set; }
            public int EnrolledStudents { get; set; }
        }

        public async Task OnGetAsync()
        {
            var lecturerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(lecturerId))
            {
                return;
            }

            // Get all courses taught by this lecturer with their lectures
            var courses = await _context.Courses
                .Where(c => c.LecturerId == lecturerId)
                .Include(c => c.Lectures)
                .ToListAsync();

            // Get all attendance records with their lectures
            var allRecords = await _context.AttendanceRecords
                .ToListAsync();

            var lectures = await _context.Lectures.ToListAsync();

            CourseList = courses
                .Select(c => new CourseAttendanceSummary
                {
                    CourseId = c.Id,
                    Code = c.Code,
                    Name = c.Name,
                    TotalLectures = c.Lectures.Count,
                    EnrolledStudents = allRecords
                        .Where(ar => lectures.Any(l => l.Id == ar.LectureId && l.CourseId == c.Id))
                        .Select(ar => ar.StudentId)
                        .Distinct()
                        .Count()
                })
                .ToList();
        }
    }
}
