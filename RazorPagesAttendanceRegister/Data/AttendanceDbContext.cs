using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using RazorPagesAttendanceRegister.Models;

namespace RazorPagesAttendanceRegister.Data
{
    /// <summary>
    /// Provides the Entity Framework Core database context for ASP.NET Core Identity and the attendance application.
    /// It stores the built-in Identity tables and any future application-specific entities.
    /// </summary>
    public class AttendanceDbContext : IdentityDbContext<AttendanceUser>
    {
        /// <summary>
        /// Initializes a new instance of the AttendanceDbContext with the supplied database options.
        /// </summary>
        /// <param name="options">The EF Core options for this context.</param>
        public AttendanceDbContext(DbContextOptions<AttendanceDbContext> options)
            : base(options)
        {
        }
    }
}
