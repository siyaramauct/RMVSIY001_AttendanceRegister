namespace RazorPagesAttendanceRegister.Services
{
    /// <summary>
    /// Handles importing attendance data for courses and lectures.
    /// </summary>
    public interface IAttendanceImportService
    {
        /// <summary>
        /// Imports parsed attendance rows for a course owned by the specified lecturer.
        /// </summary>
        /// <param name="lecturerId">The identifier of the lecturer importing attendance.</param>
        /// <param name="courseId">The identifier of the course to import attendance for.</param>
        /// <param name="rows">The parsed attendance rows to import.</param>
        /// <returns>A summary describing the result of the import operation.</returns>
        Task<ImportSummary> ImportAsync(string lecturerId, int courseId, List<ParsedAttendanceRow> rows);
    }
}
