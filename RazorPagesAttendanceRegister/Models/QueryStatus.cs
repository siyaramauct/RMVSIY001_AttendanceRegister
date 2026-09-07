namespace RazorPagesAttendanceRegister.Models
{
    /// <summary>
    /// Represents the review state of an attendance query.
    /// </summary>
    public enum QueryStatus
    {
        /// <summary>
        /// The query is awaiting a lecturer's decision.
        /// </summary>
        Pending,

        /// <summary>
        /// The lecturer accepted the query and the attendance record was excused.
        /// </summary>
        Approved,

        /// <summary>
        /// The lecturer declined the query without changing the original status.
        /// </summary>
        Rejected
    }
}
