using System.IO;

namespace RazorPagesAttendanceRegister.Services
{
    /// <summary>
    /// Parses attendance data from an uploaded import file.
    /// </summary>
    public interface IAttendanceImportParser
    {
        /// <summary>
        /// Parses attendance rows from the supplied file stream.
        /// </summary>
        /// <param name="fileStream">The stream containing the import file.</param>
        /// <returns>The parsed rows and any file-level structural errors.</returns>
        Task<ImportParseResult> ParseAsync(Stream fileStream);
    }
}
