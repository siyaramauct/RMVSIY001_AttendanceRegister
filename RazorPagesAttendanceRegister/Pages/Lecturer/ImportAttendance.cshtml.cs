using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using RazorPagesAttendanceRegister.Data;
using RazorPagesAttendanceRegister.Models;
using RazorPagesAttendanceRegister.Services;

namespace RazorPagesAttendanceRegister.Pages.Lecturer
{
    [Authorize(Roles = "Lecturer")]
    public class ImportAttendanceModel : PageModel
    {
        private const long MaxFileSize = 5 * 1024 * 1024;
        private readonly AttendanceDbContext _context;
        private readonly IAttendanceImportParser _parser;
        private readonly IAttendanceImportService _importService;

        public ImportAttendanceModel(
            AttendanceDbContext context,
            IAttendanceImportParser parser,
            IAttendanceImportService importService)
        {
            _context = context;
            _parser = parser;
            _importService = importService;
        }

        [BindProperty(SupportsGet = true)]
        public int CourseId { get; set; }

        public List<Course> Courses { get; private set; } = new();

        public List<string> StructuralErrors { get; private set; } = new();

        public ImportSummary? Summary { get; private set; }

        public string? ErrorMessage { get; private set; }

        public async Task OnGetAsync()
        {
            var lecturerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            await LoadCoursesAsync(lecturerId);
        }

        public async Task<IActionResult> OnPostAsync(int courseId, IFormFile? file)
        {
            CourseId = courseId;
            var lecturerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            await LoadCoursesAsync(lecturerId);

            if (string.IsNullOrWhiteSpace(lecturerId))
            {
                ErrorMessage = "You must be signed in as a lecturer to import attendance.";
                return Page();
            }

            if (file == null || file.Length == 0)
            {
                ErrorMessage = "Please choose an .xlsx attendance file.";
                return Page();
            }

            if (!string.Equals(Path.GetExtension(file.FileName), ".xlsx", StringComparison.OrdinalIgnoreCase))
            {
                ErrorMessage = "Only .xlsx attendance files are supported.";
                return Page();
            }

            if (file.Length > MaxFileSize)
            {
                ErrorMessage = "The attendance file must be 5 MB or smaller.";
                return Page();
            }

            ImportParseResult parseResult;
            await using (var fileStream = file.OpenReadStream())
            {
                parseResult = await _parser.ParseAsync(fileStream);
            }

            if (parseResult.StructuralErrors.Count > 0)
            {
                StructuralErrors = parseResult.StructuralErrors;
                return Page();
            }

            Summary = await _importService.ImportAsync(lecturerId, courseId, parseResult.Rows);
            return Page();
        }

        private async Task LoadCoursesAsync(string? lecturerId)
        {
            if (string.IsNullOrWhiteSpace(lecturerId))
            {
                Courses = new List<Course>();
                return;
            }

            Courses = await _context.Courses
                .Where(course => course.LecturerId == lecturerId)
                .OrderBy(course => course.Code)
                .ToListAsync();
        }
    }
}
