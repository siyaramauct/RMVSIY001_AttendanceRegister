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
        /// Gets or sets the courses available in the attendance system.
        /// </summary>
        public DbSet<Course> Courses { get; set; }

        /// <summary>
        /// Gets or sets the lecture sessions scheduled for courses.
        /// </summary>
        public DbSet<Lecture> Lectures { get; set; }

        /// <summary>
        /// Gets or sets the attendance records captured for students during lectures.
        /// </summary>
        public DbSet<AttendanceRecord> AttendanceRecords { get; set; }

        /// <summary>
        /// Gets or sets the queries submitted about attendance records.
        /// </summary>
        public DbSet<AttendanceQuery> AttendanceQueries { get; set; }

        /// <summary>
        /// Initializes a new instance of the AttendanceDbContext with the supplied database options.
        /// </summary>
        /// <param name="options">The EF Core options for this context.</param>
        public AttendanceDbContext(DbContextOptions<AttendanceDbContext> options)
            : base(options)
        {
        }

        /// <summary>
        /// Configures the EF Core model, including attendance-specific uniqueness and delete rules.
        /// </summary>
        /// <param name="modelBuilder">The model builder used to define the entity configuration.</param>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Ensures a student can only have one attendance row for the same lecture,
            // preventing duplicate attendance submissions for the same class session.
            modelBuilder.Entity<AttendanceRecord>()
                .HasIndex(ar => new { ar.LectureId, ar.StudentId })
                .IsUnique();

            // Sets the real Course -> Lecture relationship explicitly so EF does not create a shadow
            // foreign key column and keeps the CourseId property mapped to the correct course record.
            modelBuilder.Entity<Lecture>()
                .HasOne(l => l.Course)
                .WithMany(c => c.Lectures)
                .HasForeignKey(l => l.CourseId)
                .OnDelete(DeleteBehavior.Restrict);

            // Restricts deletion of a lecture when attendance records still reference it so attendance
            // history remains intact and is not automatically wiped by a lecture removal.
            modelBuilder.Entity<AttendanceRecord>()
                .HasOne(ar => ar.Lecture)
                .WithMany()
                .HasForeignKey(ar => ar.LectureId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<AttendanceQuery>()
                .Property(aq => aq.Reason)
                .HasMaxLength(500)
                .IsRequired();

            modelBuilder.Entity<AttendanceQuery>()
                .Property(aq => aq.Status)
                .HasDefaultValue(QueryStatus.Pending);

            modelBuilder.Entity<AttendanceQuery>()
                .HasOne(aq => aq.AttendanceRecord)
                .WithMany()
                .HasForeignKey(aq => aq.AttendanceRecordId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<AttendanceQuery>()
                .HasOne(aq => aq.Student)
                .WithMany()
                .HasForeignKey(aq => aq.StudentId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
