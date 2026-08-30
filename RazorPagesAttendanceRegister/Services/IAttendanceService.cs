using RazorPagesAttendanceRegister.Models;

namespace RazorPagesAttendanceRegister.Services
{
    /// <summary>
    /// Provides attendance-related behaviors for students and lectures.
    /// </summary>
    public interface IAttendanceService
    {
        /// <summary>
        /// Records attendance for a student against a lecture if the lecture exists and the student has not already recorded attendance.
        /// </summary>
        /// <param name="studentId">The student identifier.</param>
        /// <param name="lectureId">The lecture identifier.</param>
        /// <returns>The result of the attendance recording attempt.</returns>
        Task<AttendanceResult> RecordAttendanceAsync(string studentId, int lectureId);

        /// <summary>
        /// Retrieves all available lectures along with their associated course details.
        /// </summary>
        /// <returns>A list of lectures ordered by the most recent scheduled date first.</returns>
        Task<List<Lecture>> GetAvailableLecturesAsync();

        /// <summary>
        /// Retrieves all lectures a student is expected to attend, backfilling absent records for past lectures when needed.
        /// </summary>
        /// <param name="studentId">The student identifier.</param>
        /// <returns>A list of lecture records for the student, including course details.</returns>
        Task<List<Lecture>> GetLecturesForStudentAsync(string studentId);

        /// <summary>
        /// Retrieves the student's attendance records for past lectures, including the recorded status for each lecture.
        /// </summary>
        /// <param name="studentId">The student identifier.</param>
        /// <returns>A display-friendly list of attendance records for the student.</returns>
        Task<List<AttendanceRecordDisplayDto>> GetAttendanceHistoryForStudentAsync(string studentId);

        /// <summary>
        /// Submits a query about a student's attendance record.
        /// </summary>
        /// <param name="studentId">The student identifier.</param>
        /// <param name="attendanceRecordId">The attendance record identifier.</param>
        /// <param name="reason">The reason for querying the record.</param>
        /// <returns>The result of the query submission attempt.</returns>
        Task<AttendanceResult> RaiseQueryAsync(string studentId, int attendanceRecordId, string reason);

        /// <summary>
        /// Retrieves all attendance queries submitted by a student, including related lecture and course details.
        /// </summary>
        /// <param name="studentId">The student identifier.</param>
        /// <returns>The student's attendance queries.</returns>
        Task<List<AttendanceQuery>> GetQueriesForStudentAsync(string studentId);

        /// <summary>
        /// Retrieves pending attendance queries for courses owned by a lecturer.
        /// </summary>
        /// <param name="lecturerId">The lecturer identifier.</param>
        /// <returns>Pending queries with student, course, lecture, and attendance details.</returns>
        Task<List<AttendanceQuery>> GetPendingQueriesForLecturerAsync(string lecturerId);

        /// <summary>
        /// Approves or rejects a pending attendance query owned by a lecturer's course.
        /// </summary>
        /// <param name="lecturerId">The lecturer identifier.</param>
        /// <param name="queryId">The query identifier.</param>
        /// <param name="approve">Whether the query should be approved.</param>
        /// <param name="response">The lecturer's response.</param>
        /// <returns>The result of resolving the query.</returns>
        Task<AttendanceResult> ResolveQueryAsync(string lecturerId, int queryId, bool approve, string response);
    }
}
