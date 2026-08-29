using Microsoft.AspNetCore.Identity;

namespace RazorPagesAttendanceRegister.Models
{
    /// <summary>
    /// Represents an application user for the attendance register system.
    /// Inherits from ASP.NET Core Identity's default user type using the default string-based Id.
    /// </summary>
    public class AttendanceUser : IdentityUser
    {
        /// <summary>
        /// The user's first name as displayed in the application and used for identity-related communication.
        /// </summary>
        [PersonalData]
        public required string Name { get; set; }

        /// <summary>
        /// The user's surname or family name, used for identification and display in attendance records.
        /// </summary>
        [PersonalData]
        public required string Surname { get; set; }

        /// <summary>
        /// The user's role within the system, determining whether they are a student or lecturer.
        /// </summary>
        [PersonalData]
        public UserRole Role { get; set; }

        /// <summary>
        /// The student's unique number used to map the user to the corresponding Excel spreadsheet record.
        /// This is only relevant when Role is set to Student and may be null for other roles.
        /// </summary>
        [PersonalData]
        public string? StudentNumber { get; set; }
    }
}
