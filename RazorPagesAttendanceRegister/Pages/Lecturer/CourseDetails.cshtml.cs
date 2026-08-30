using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using RazorPagesAttendanceRegister.Data;
using RazorPagesAttendanceRegister.Models;

namespace RazorPagesAttendanceRegister.Pages.Lecturer
{
    [Authorize(Roles = "Lecturer")]
    public class CourseDetailsModel : PageModel
    {
        private readonly AttendanceDbContext _context;

        public CourseDetailsModel(AttendanceDbContext context)
        {
            _context = context;
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
            public DateTime RecordedAt { get; set; }
        }

        public List<LectureAttendanceGroup> AttendanceByLecture { get; set; } = new();
        public string ErrorMessage { get; set; } = string.Empty;
        public string SuccessMessage { get; set; } = string.Empty;

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
