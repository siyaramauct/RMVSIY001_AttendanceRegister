using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using RazorPagesAttendanceRegister.Data;
using RazorPagesAttendanceRegister.Models;
using RazorPagesAttendanceRegister.Services;

namespace RazorPagesAttendanceRegister.Pages.Lecturer
{
    [Authorize(Roles = "Lecturer")]
    public class CourseDetailsModel : PageModel
    {
        private readonly AttendanceDbContext _context;
        private readonly IAttendanceImportParser _importParser;
        private readonly IAttendanceImportService _importService;

        public CourseDetailsModel(
            AttendanceDbContext context,
            IAttendanceImportParser importParser,
            IAttendanceImportService importService)
        {
            _context = context;
            _importParser = importParser;
            _importService = importService;
        }

        public Course? Course { get; set; }

        /// <summary>
        /// Represents attendance records grouped by lecture date.
        /// </summary>
        public class LectureAttendanceGroup
        {
            public DateOnly LectureDate { get; set; }
            public TimeOnly StartTime { get; set; }
            public TimeOnly EndTime { get; set; }
            public int LectureId { get; set; }
            public List<StudentAttendanceRecord> StudentRecords { get; set; } = new();
        }

        /// <summary>
        /// Represents a single student's attendance record for a lecture.
        /// </summary>
        public class StudentAttendanceRecord
        {
            public int AttendanceRecordId { get; set; }
            public string StudentId { get; set; } = string.Empty;
            public string StudentName { get; set; } = string.Empty;
            public string StudentNumber { get; set; } = string.Empty;
            public AttendanceStatus Status { get; set; }
            public AttendanceMethod Method { get; set; }
            public DateTime RecordedAt { get; set; }
        }

        public List<LectureAttendanceGroup> AttendanceByLecture { get; set; } = new();
        public string ErrorMessage { get; set; } = string.Empty;
        public string SuccessMessage { get; set; } = string.Empty;
        public List<string> StructuralErrors { get; set; } = new();
        public List<string> UnmatchedStudents { get; set; } = new();
        public ImportSummary? ImportResult { get; set; }

        public async Task<IActionResult> OnGetAsync(int courseId)
        {
            var lecturerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(lecturerId))
            {
                ErrorMessage = "You must be signed in to view this page.";
                return Page();
            }

            // Get the course and verify the lecturer owns it
            Course = await _context.Courses
                .Where(c => c.Id == courseId && c.LecturerId == lecturerId)
                .FirstOrDefaultAsync();

            if (Course == null)
            {
                ErrorMessage = "Course not found or you don't have permission to view it.";
                return Page();
            }

            await LoadAttendanceData(courseId);
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int courseId, int attendanceRecordId, int newStatus)
        {
            var lecturerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(lecturerId))
            {
                ErrorMessage = "You must be signed in to perform this action.";
                return Page();
            }

            // Verify lecturer owns the course
            var course = await _context.Courses
                .Where(c => c.Id == courseId && c.LecturerId == lecturerId)
                .FirstOrDefaultAsync();

            if (course == null)
            {
                ErrorMessage = "Course not found or you don't have permission to modify it.";
                return Page();
            }

            // Get the attendance record
            var attendanceRecord = await _context.AttendanceRecords
                .FirstOrDefaultAsync(ar => ar.Id == attendanceRecordId);

            if (attendanceRecord == null)
            {
                ErrorMessage = "Attendance record not found.";
                await LoadAttendanceData(courseId);
                Course = course;
                return Page();
            }

            // Verify the lecture belongs to this course
            var lecture = await _context.Lectures
                .FirstOrDefaultAsync(l => l.Id == attendanceRecord.LectureId && l.CourseId == courseId);

            if (lecture == null)
            {
                ErrorMessage = "Attendance record does not belong to this course.";
                await LoadAttendanceData(courseId);
                Course = course;
                return Page();
            }

            // Update the status
            if (Enum.TryParse<AttendanceStatus>(newStatus.ToString(), out var statusValue))
            {
                attendanceRecord.Status = statusValue;
                attendanceRecord.Method = AttendanceMethod.Manual;
                attendanceRecord.RecordedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
                SuccessMessage = "Attendance record updated successfully.";
            }
            else
            {
                ErrorMessage = "Invalid attendance status.";
            }

            await LoadAttendanceData(courseId);
            Course = course;
            return Page();
        }

        public async Task<IActionResult> OnPostUploadAsync(int courseId, IFormFile? uploadFile)
        {
            var lecturerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(lecturerId))
            {
                ErrorMessage = "You must be signed in to import attendance.";
                return Page();
            }

            var course = await _context.Courses
                .Where(c => c.Id == courseId && c.LecturerId == lecturerId)
                .FirstOrDefaultAsync();

            if (course == null)
            {
                ErrorMessage = "Course not found or you don't have permission to modify it.";
                return Page();
            }

            Course = course;

            if (uploadFile == null || uploadFile.Length == 0)
            {
                ErrorMessage = "Please select an Excel (.xlsx) file to upload.";
                await LoadAttendanceData(courseId);
                return Page();
            }

            if (!uploadFile.FileName.EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase))
            {
                ErrorMessage = "Only Excel spreadsheet files (.xlsx) are supported.";
                await LoadAttendanceData(courseId);
                return Page();
            }

            ImportParseResult parseResult;
            try
            {
                using var stream = uploadFile.OpenReadStream();
                parseResult = await _importParser.ParseAsync(stream);
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Failed to read the uploaded spreadsheet: {ex.Message}";
                await LoadAttendanceData(courseId);
                return Page();
            }

            if (parseResult.StructuralErrors.Any())
            {
                StructuralErrors = parseResult.StructuralErrors;
                ErrorMessage = "The uploaded file contains formatting or structural errors.";
                await LoadAttendanceData(courseId);
                return Page();
            }

            if (!parseResult.Rows.Any())
            {
                ErrorMessage = "No attendance data rows were found in the uploaded file.";
                await LoadAttendanceData(courseId);
                return Page();
            }

            var summary = await _importService.ImportAsync(lecturerId, courseId, parseResult.Rows);

            if (summary.Success)
            {
                SuccessMessage = summary.Message;
                ImportResult = summary;
                UnmatchedStudents = summary.UnmatchedStudentNumbers;
            }
            else
            {
                ErrorMessage = summary.Message;
            }

            await LoadAttendanceData(courseId);
            return Page();
        }

        private async Task LoadAttendanceData(int courseId)
        {
            var now = DateTime.UtcNow;

            // Get all lectures for this course
            var allLectures = await _context.Lectures
                .Where(l => l.CourseId == courseId)
                .Include(l => l.Course)
                .ToListAsync();

            // Filter to only past and current lectures (client-side)
            var lectures = allLectures
                .Where(l => l.ScheduledDate.ToDateTime(l.EndTime, DateTimeKind.Utc) <= now)
                .OrderByDescending(l => l.ScheduledDate)
                .ThenByDescending(l => l.StartTime)
                .ToList();

            if (!lectures.Any())
            {
                return;
            }

            // Get all attendance records for these lectures
            var lectureIds = lectures.Select(l => l.Id).ToList();
            var attendanceRecords = await _context.AttendanceRecords
                .Where(ar => lectureIds.Contains(ar.LectureId))
                .ToListAsync();

            // Get all students who have attended any lecture in this course
            var studentIds = attendanceRecords.Select(ar => ar.StudentId).Distinct().ToList();
            var students = await _context.Users
                .Where(u => studentIds.Contains(u.Id))
                .ToListAsync();

            // Create a dictionary for quick student lookup
            var studentDict = students.ToDictionary(s => s.Id, s => s);

            // Group attendance by lecture date
            AttendanceByLecture = lectures
                .Select(lecture => new LectureAttendanceGroup
                {
                    LectureDate = lecture.ScheduledDate,
                    StartTime = lecture.StartTime,
                    EndTime = lecture.EndTime,
                    LectureId = lecture.Id,
                    StudentRecords = attendanceRecords
                        .Where(ar => ar.LectureId == lecture.Id)
                        .Select(ar =>
                        {
                            studentDict.TryGetValue(ar.StudentId, out var student);
                            return new StudentAttendanceRecord
                            {
                                AttendanceRecordId = ar.Id,
                                StudentId = ar.StudentId,
                                StudentName = student != null ? $"{student.Name} {student.Surname}" : "Unknown",
                                StudentNumber = student?.StudentNumber ?? string.Empty,
                                Status = ar.Status,
                                Method = ar.Method,
                                RecordedAt = ar.RecordedAt
                            };
                        })
                        .OrderBy(sr => sr.StudentName)
                        .ToList()
                })
                .ToList();
        }
    }
}
