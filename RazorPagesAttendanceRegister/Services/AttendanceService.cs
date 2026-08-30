using Microsoft.EntityFrameworkCore;
using RazorPagesAttendanceRegister.Data;
using RazorPagesAttendanceRegister.Models;

namespace RazorPagesAttendanceRegister.Services
{
    /// <summary>
    /// Handles attendance recording and lecture retrieval for the attendance system.
    /// </summary>
    public class AttendanceService : IAttendanceService
    {
        private readonly AttendanceDbContext _context;

        /// <summary>
        /// Initializes a new instance of the <see cref="AttendanceService"/> class.
        /// </summary>
        /// <param name="context">The data context used to access attendance data.</param>
        public AttendanceService(AttendanceDbContext context)
        {
            _context = context;
        }

        /// <inheritdoc />
        public async Task<AttendanceResult> RecordAttendanceAsync(string studentId, int lectureId)
        {
            if (string.IsNullOrWhiteSpace(studentId))
            {
                return new AttendanceResult
                {
                    Success = false,
                    Message = "Student not found."
                };
            }

            var lecture = await _context.Lectures
                .FirstOrDefaultAsync(l => l.Id == lectureId);

            if (lecture == null)
            {
                return new AttendanceResult
                {
                    Success = false,
                    Message = "Lecture not found."
                };
            }

            var alreadyExists = await _context.AttendanceRecords
                .AnyAsync(ar => ar.StudentId == studentId && ar.LectureId == lectureId);

            if (alreadyExists)
            {
                return new AttendanceResult
                {
                    Success = false,
                    Message = "You've already recorded attendance for this lecture."
                };
            }

            // Known simplification: lesson times are treated as UTC values for this project, so we compare directly against DateTime.UtcNow.
            var now = DateTime.UtcNow;
            var startTime = lecture.ScheduledDate.ToDateTime(lecture.StartTime, DateTimeKind.Utc);
            var endTime = lecture.ScheduledDate.ToDateTime(lecture.EndTime, DateTimeKind.Utc);

            if (now < startTime)
            {
                return new AttendanceResult
                {
                    Success = false,
                    Message = "This lecture hasn't started yet."
                };
            }

            if (now > endTime)
            {
                return new AttendanceResult
                {
                    Success = false,
                    Message = "This lecture has already ended — you can no longer mark attendance for it."
                };
            }

            var record = new AttendanceRecord
            {
                LectureId = lectureId,
                StudentId = studentId,
                Status = AttendanceStatus.Present,
                RecordedAt = DateTime.UtcNow
            };

            await _context.AttendanceRecords.AddAsync(record);
            await _context.SaveChangesAsync();

            return new AttendanceResult
            {
                Success = true,
                Message = "Attendance recorded successfully."
            };
        }

        /// <inheritdoc />
        public async Task<List<Lecture>> GetAvailableLecturesAsync()
        {
            return await _context.Lectures
                .Include(l => l.Course)
                .OrderByDescending(l => l.ScheduledDate)
                .ToListAsync();
        }

        /// <inheritdoc />
        public async Task<List<Lecture>> GetLecturesForStudentAsync(string studentId)
        {
            if (string.IsNullOrWhiteSpace(studentId))
            {
                return new List<Lecture>();
            }

            var lectures = await _context.Lectures
                .Include(l => l.Course)
                .OrderByDescending(l => l.ScheduledDate)
                .ToListAsync();

            var recordsToInsert = new List<AttendanceRecord>();
            var now = DateTime.UtcNow;

            foreach (var lecture in lectures)
            {
                var lectureEnd = lecture.ScheduledDate.ToDateTime(lecture.EndTime, DateTimeKind.Utc);
                if (now <= lectureEnd)
                {
                    continue;
                }

                var hasRecord = await _context.AttendanceRecords
                    .AnyAsync(ar => ar.StudentId == studentId && ar.LectureId == lecture.Id);

                if (hasRecord)
                {
                    continue;
                }

                recordsToInsert.Add(new AttendanceRecord
                {
                    LectureId = lecture.Id,
                    StudentId = studentId,
                    Status = AttendanceStatus.Absent,
                    RecordedAt = DateTime.UtcNow
                });
            }

            if (recordsToInsert.Count > 0)
            {
                await _context.AttendanceRecords.AddRangeAsync(recordsToInsert);
                await _context.SaveChangesAsync();
            }

            return lectures;
        }

        /// <inheritdoc />
        public async Task<List<AttendanceRecordDisplayDto>> GetAttendanceHistoryForStudentAsync(string studentId)
        {
            if (string.IsNullOrWhiteSpace(studentId))
            {
                return new List<AttendanceRecordDisplayDto>();
            }

            var records = await _context.AttendanceRecords
                .Where(ar => ar.StudentId == studentId)
                .Join(
                    _context.Lectures.Include(l => l.Course),
                    ar => ar.LectureId,
                    l => l.Id,
                    (ar, l) => new AttendanceRecordDisplayDto
                    {
                        AttendanceRecordId = ar.Id,
                        LectureId = l.Id,
                        CourseCode = l.Course != null ? l.Course.Code : string.Empty,
                        CourseName = l.Course != null ? l.Course.Name : string.Empty,
                        LectureDate = l.ScheduledDate,
                        Status = ar.Status
                    })
                .OrderByDescending(x => x.LectureDate)
                .ToListAsync();

            return records;
        }

        /// <inheritdoc />
        public async Task<AttendanceResult> RaiseQueryAsync(string studentId, int attendanceRecordId, string reason)
        {
            var record = await _context.AttendanceRecords
                .FirstOrDefaultAsync(ar => ar.Id == attendanceRecordId);

            if (record == null)
            {
                return new AttendanceResult
                {
                    Success = false,
                    Message = "Attendance record not found."
                };
            }

            if (record.StudentId != studentId)
            {
                return new AttendanceResult
                {
                    Success = false,
                    Message = "You can only query your own attendance records."
                };
            }

            var pendingQueryExists = await _context.AttendanceQueries
                .AnyAsync(aq => aq.AttendanceRecordId == attendanceRecordId && aq.Status == QueryStatus.Pending);

            if (pendingQueryExists)
            {
                return new AttendanceResult
                {
                    Success = false,
                    Message = "You already have a pending query on this record."
                };
            }

            if (string.IsNullOrEmpty(reason) || reason.Length > 500)
            {
                return new AttendanceResult
                {
                    Success = false,
                    Message = "The query reason is required and must be 500 characters or fewer."
                };
            }

            var query = new AttendanceQuery
            {
                AttendanceRecordId = attendanceRecordId,
                StudentId = studentId,
                Reason = reason,
                Status = QueryStatus.Pending,
                CreatedAt = DateTime.UtcNow
            };

            await _context.AttendanceQueries.AddAsync(query);
            await _context.SaveChangesAsync();

            return new AttendanceResult
            {
                Success = true,
                Message = "Your query has been submitted."
            };
        }

        /// <inheritdoc />
        public async Task<List<AttendanceQuery>> GetQueriesForStudentAsync(string studentId)
        {
            if (string.IsNullOrWhiteSpace(studentId))
            {
                return new List<AttendanceQuery>();
            }

            return await _context.AttendanceQueries
                .Where(aq => aq.StudentId == studentId)
                .Include(aq => aq.AttendanceRecord!)
                    .ThenInclude(ar => ar.Lecture!)
                        .ThenInclude(l => l.Course)
                .OrderByDescending(aq => aq.CreatedAt)
                .ToListAsync();
        }

        /// <inheritdoc />
        public async Task<List<AttendanceQuery>> GetPendingQueriesForLecturerAsync(string lecturerId)
        {
            if (string.IsNullOrWhiteSpace(lecturerId))
            {
                return new List<AttendanceQuery>();
            }

            return await _context.AttendanceQueries
                .Where(aq => aq.Status == QueryStatus.Pending &&
                             aq.AttendanceRecord!.Lecture!.Course!.LecturerId == lecturerId)
                .Include(aq => aq.Student)
                .Include(aq => aq.AttendanceRecord!)
                    .ThenInclude(ar => ar.Lecture!)
                        .ThenInclude(l => l.Course)
                .OrderBy(aq => aq.CreatedAt)
                .ToListAsync();
        }

        /// <inheritdoc />
        public async Task<AttendanceResult> ResolveQueryAsync(
            string lecturerId,
            int queryId,
            bool approve,
            string response)
        {
            var query = await _context.AttendanceQueries
                .Include(aq => aq.AttendanceRecord!)
                    .ThenInclude(ar => ar.Lecture!)
                        .ThenInclude(l => l.Course)
                .FirstOrDefaultAsync(aq => aq.Id == queryId);

            if (query == null)
            {
                return new AttendanceResult
                {
                    Success = false,
                    Message = "Query not found."
                };
            }

            var courseLecturerId = query.AttendanceRecord?.Lecture?.Course?.LecturerId;
            if (courseLecturerId != lecturerId)
            {
                return new AttendanceResult
                {
                    Success = false,
                    Message = "You can only resolve queries for your own courses."
                };
            }

            if (query.Status != QueryStatus.Pending)
            {
                return new AttendanceResult
                {
                    Success = false,
                    Message = "This query has already been resolved."
                };
            }

            query.Status = approve ? QueryStatus.Approved : QueryStatus.Rejected;
            query.LecturerResponse = response;
            query.ResolvedAt = DateTime.UtcNow;

            if (approve && query.AttendanceRecord != null)
            {
                query.AttendanceRecord.Status = AttendanceStatus.Excused;
            }

            await _context.SaveChangesAsync();

            return new AttendanceResult
            {
                Success = true,
                Message = approve
                    ? "Query approved successfully."
                    : "Query rejected successfully."
            };
        }
    }
}
