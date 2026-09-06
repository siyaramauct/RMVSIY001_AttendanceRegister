using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using RazorPagesAttendanceRegister.Data;
using RazorPagesAttendanceRegister.Models;
using RazorPagesAttendanceRegister.Services;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<AttendanceDbContext>(options =>
    options.UseSqlite(connectionString));

builder.Services.AddIdentity<AttendanceUser, IdentityRole>(options =>
    {
        options.SignIn.RequireConfirmedAccount = false;
    })
    .AddEntityFrameworkStores<AttendanceDbContext>()
    .AddDefaultTokenProviders();

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("Student", policy => policy.RequireRole("Student"));
    options.AddPolicy("Lecturer", policy => policy.RequireRole("Lecturer"));
});

builder.Services.AddRazorPages(options =>
{
    // Route path must match the folder name under /Pages exactly.
    options.Conventions.AuthorizeFolder("/Student", "Student");
    options.Conventions.AuthorizeFolder("/Lecturer", "Lecturer");
});
builder.Services.AddScoped<IEmailSender, NoOpEmailSender>();
builder.Services.AddScoped<IAttendanceService, AttendanceService>();
builder.Services.AddScoped<IAttendanceImportParser, AttendanceImportParser>();
builder.Services.AddScoped<IAttendanceImportService, AttendanceImportService>();

var app = builder.Build();

await SeedRolesAsync(app);
await SeedTestDataAsync(app);
await SeedAdditionalTestDataAsync(app);

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();
app.MapRazorPages()
   .WithStaticAssets();

app.Run();

static async Task SeedRolesAsync(WebApplication app)
{
    using var scope = app.Services.CreateScope();
    var context = scope.ServiceProvider.GetRequiredService<AttendanceDbContext>();
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

    // Apply pending migrations before any role queries run.
    await context.Database.MigrateAsync();

    string[] roles = ["Student", "Lecturer"];

    foreach (var roleName in roles)
    {
        if (!await roleManager.RoleExistsAsync(roleName))
        {
            await roleManager.CreateAsync(new IdentityRole(roleName));
        }
    }
}

static async Task SeedTestDataAsync(WebApplication app)
{
    using var scope = app.Services.CreateScope();
    var context = scope.ServiceProvider.GetRequiredService<AttendanceDbContext>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AttendanceUser>>();
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

    // Apply any pending migrations before checking whether course data is present.
    await context.Database.MigrateAsync();

    // Only seed if the Course table is empty so the sample data is not duplicated on every restart.
    if (await context.Courses.AnyAsync())
    {
        return;
    }

    var today = DateOnly.FromDateTime(DateTime.Today);
    var now = DateTime.UtcNow;

    // Create lecturer users
    var walterWhite = new AttendanceUser
    {
        UserName = "walter.white@uct.ac.za",
        Email = "walter.white@uct.ac.za",
        Name = "Walter",
        Surname = "White",
        Role = UserRole.Lecturer,
        EmailConfirmed = true
    };

    var joanaHill = new AttendanceUser
    {
        UserName = "joana.hill@uct.ac.za",
        Email = "joana.hill@uct.ac.za",
        Name = "Joana",
        Surname = "Hill",
        Role = UserRole.Lecturer,
        EmailConfirmed = true
    };

    // Create student users
    var samSmith = new AttendanceUser
    {
        UserName = "sam.smith@uct.ac.za",
        Email = "sam.smith@uct.ac.za",
        Name = "Sam",
        Surname = "Smith",
        Role = UserRole.Student,
        StudentNumber = "SMTSAM001",
        EmailConfirmed = true
    };

    var amadDiallo = new AttendanceUser
    {
        UserName = "amad.diallo@uct.ac.za",
        Email = "amad.diallo@uct.ac.za",
        Name = "Amad",
        Surname = "Diallo",
        Role = UserRole.Student,
        StudentNumber = "DIALAMO001",
        EmailConfirmed = true
    };

    // Create users if they don't exist
    var existingWalter = await userManager.FindByEmailAsync(walterWhite.Email);
    if (existingWalter == null)
    {
        await userManager.CreateAsync(walterWhite, "TestPassword123!");
        await userManager.AddToRoleAsync(walterWhite, "Lecturer");
    }
    else
    {
        walterWhite = existingWalter;
    }

    var existingJoana = await userManager.FindByEmailAsync(joanaHill.Email);
    if (existingJoana == null)
    {
        await userManager.CreateAsync(joanaHill, "TestPassword123!");
        await userManager.AddToRoleAsync(joanaHill, "Lecturer");
    }
    else
    {
        joanaHill = existingJoana;
    }

    var existingSam = await userManager.FindByEmailAsync(samSmith.Email);
    if (existingSam == null)
    {
        await userManager.CreateAsync(samSmith, "TestPassword123!");
        await userManager.AddToRoleAsync(samSmith, "Student");
    }
    else
    {
        samSmith = existingSam;
    }

    var existingAmad = await userManager.FindByEmailAsync(amadDiallo.Email);
    if (existingAmad == null)
    {
        await userManager.CreateAsync(amadDiallo, "TestPassword123!");
        await userManager.AddToRoleAsync(amadDiallo, "Student");
    }
    else
    {
        amadDiallo = existingAmad;
    }


    // Create courses for both lecturers
    var course1 = new Course
    {
        Code = "INF3003W",
        Name = "Systems Development I",
        LecturerId = walterWhite.Id
    };

    var course2 = new Course
    {
        Code = "MAT1012F",
        Name = "Calculus for Engineers",
        LecturerId = joanaHill.Id
    };

    await context.Courses.AddRangeAsync(course1, course2);
    await context.SaveChangesAsync();

    // Create lectures for course 1 (5 past, 1 today/now, 5 future)
    var lectures1 = new List<Lecture>();

    // 5 past lectures (spanning last 5 days)
    for (int i = 5; i >= 1; i--)
    {
        lectures1.Add(new Lecture
        {
            CourseId = course1.Id,
            ScheduledDate = today.AddDays(-i),
            StartTime = new TimeOnly(9, 0),
            EndTime = new TimeOnly(10, 30)
        });
    }

    // Today/now lecture (spanning current moment for testing time-window logic)
    lectures1.Add(new Lecture
    {
        CourseId = course1.Id,
        ScheduledDate = DateOnly.FromDateTime(now),
        StartTime = new TimeOnly(now.AddMinutes(-10).Hour, now.AddMinutes(-10).Minute),
        EndTime = new TimeOnly(now.AddMinutes(20).Hour, now.AddMinutes(20).Minute)
    });

    // 5 future lectures (spanning next 5 days)
    for (int i = 1; i <= 5; i++)
    {
        lectures1.Add(new Lecture
        {
            CourseId = course1.Id,
            ScheduledDate = today.AddDays(i),
            StartTime = new TimeOnly(14, 0),
            EndTime = new TimeOnly(15, 30)
        });
    }

    // Create lectures for course 2 (5 past, 1 today/now, 5 future)
    var lectures2 = new List<Lecture>();

    // 5 past lectures
    for (int i = 5; i >= 1; i--)
    {
        lectures2.Add(new Lecture
        {
            CourseId = course2.Id,
            ScheduledDate = today.AddDays(-i),
            StartTime = new TimeOnly(11, 0),
            EndTime = new TimeOnly(12, 30)
        });
    }

    // Today/now lecture
    lectures2.Add(new Lecture
    {
        CourseId = course2.Id,
        ScheduledDate = DateOnly.FromDateTime(now),
        StartTime = new TimeOnly(now.AddMinutes(-15).Hour, now.AddMinutes(-15).Minute),
        EndTime = new TimeOnly(now.AddMinutes(15).Hour, now.AddMinutes(15).Minute)
    });

    // 5 future lectures
    for (int i = 1; i <= 5; i++)
    {
        lectures2.Add(new Lecture
        {
            CourseId = course2.Id,
            ScheduledDate = today.AddDays(i),
            StartTime = new TimeOnly(10, 0),
            EndTime = new TimeOnly(11, 30)
        });
    }

    await context.Lectures.AddRangeAsync(lectures1);
    await context.Lectures.AddRangeAsync(lectures2);
    await context.SaveChangesAsync();

    // Populate attendance records for both students across all lectures
    var allLectures = await context.Lectures.ToListAsync();
    var attendanceRecords = new List<AttendanceRecord>();

    foreach (var lecture in allLectures)
    {
        var lectureDateTime = lecture.ScheduledDate.ToDateTime(lecture.EndTime, DateTimeKind.Utc);

        // Only create attendance records for past lectures
        if (lectureDateTime > DateTime.UtcNow)
        {
            continue;
        }

        // Sam Smith attendance (mix of Present, Absent, Late, Excused)
        var samStatus = (lecture.ScheduledDate.Day % 4) switch
        {
            0 => AttendanceStatus.Present,
            1 => AttendanceStatus.Absent,
            2 => AttendanceStatus.Late,
            _ => AttendanceStatus.Excused
        };

        attendanceRecords.Add(new AttendanceRecord
        {
            LectureId = lecture.Id,
            StudentId = samSmith.Id,
            Status = samStatus,
            RecordedAt = DateTime.UtcNow
        });

        // Amad Diallo attendance (mix of Present, Absent, Late)
        var amadStatus = (lecture.ScheduledDate.Day % 3) switch
        {
            0 => AttendanceStatus.Present,
            1 => AttendanceStatus.Absent,
            _ => AttendanceStatus.Late
        };

        attendanceRecords.Add(new AttendanceRecord
        {
            LectureId = lecture.Id,
            StudentId = amadDiallo.Id,
            Status = amadStatus,
            RecordedAt = DateTime.UtcNow
        });
    }

    if (attendanceRecords.Count > 0)
    {
        await context.AttendanceRecords.AddRangeAsync(attendanceRecords);
        await context.SaveChangesAsync();
    }

    Console.WriteLine($"Seed data created: {allLectures.Count} lectures, {attendanceRecords.Count} attendance records.");
    Console.WriteLine($"Test accounts created:");
    Console.WriteLine($"  Lecturer 1: {walterWhite.Email} / Password: TestPassword123!");
    Console.WriteLine($"  Lecturer 2: {joanaHill.Email} / Password: TestPassword123!");
    Console.WriteLine($"  Student 1: {samSmith.Email} / Password: TestPassword123!");
    Console.WriteLine($"  Student 2: {amadDiallo.Email} / Password: TestPassword123!");
}

static async Task SeedAdditionalTestDataAsync(WebApplication app)
{
    using var scope = app.Services.CreateScope();
    var context = scope.ServiceProvider.GetRequiredService<AttendanceDbContext>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AttendanceUser>>();

    var additionalLecturers = new (string Email, string Name, string Surname)[]
    {
        ("test.lecturer1@uct.ac.za", "Aisha", "Ndlovu"),
        ("test.lecturer2@uct.ac.za", "Bongani", "Maseko"),
        ("test.lecturer3@uct.ac.za", "Candice", "Jacobs"),
        ("test.lecturer4@uct.ac.za", "David", "Peters"),
        ("test.lecturer5@uct.ac.za", "Elena", "Naidoo")
    };

    var additionalStudents = new (string Email, string Name, string Surname, string StudentNumber)[]
    {
        ("test.student1@uct.ac.za", "Liam", "Mokoena", "TESTSTU001"),
        ("test.student2@uct.ac.za", "Mia", "Williams", "TESTSTU002"),
        ("test.student3@uct.ac.za", "Noah", "Dlamini", "TESTSTU003"),
        ("test.student4@uct.ac.za", "Zoe", "Pillay", "TESTSTU004"),
        ("test.student5@uct.ac.za", "Ethan", "Botha", "TESTSTU005")
    };

    var lecturers = new List<AttendanceUser>();
    foreach (var seedLecturer in additionalLecturers)
    {
        var lecturer = await GetOrCreateSeedUserAsync(
            userManager,
            seedLecturer.Email,
            seedLecturer.Name,
            seedLecturer.Surname,
            UserRole.Lecturer);
        lecturers.Add(lecturer);
    }

    var students = new List<AttendanceUser>();
    foreach (var seedStudent in additionalStudents)
    {
        var student = await GetOrCreateSeedUserAsync(
            userManager,
            seedStudent.Email,
            seedStudent.Name,
            seedStudent.Surname,
            UserRole.Student,
            seedStudent.StudentNumber);
        students.Add(student);
    }

    var courseDefinitions = new (string Code, string Name)[]
    {
        ("TST1001W", "Foundations of Computing"),
        ("TST1002W", "Database Design"),
        ("TST1003W", "Web Application Development"),
        ("TST1004W", "Software Testing"),
        ("TST1005W", "User Experience Design")
    };

    var existingCourseCodes = await context.Courses
        .Where(c => c.Code.StartsWith("TST100"))
        .Select(c => c.Code)
        .ToListAsync();

    var newCourses = new List<Course>();
    for (var index = 0; index < courseDefinitions.Length; index++)
    {
        var definition = courseDefinitions[index];
        if (existingCourseCodes.Contains(definition.Code))
        {
            continue;
        }

        newCourses.Add(new Course
        {
            Code = definition.Code,
            Name = definition.Name,
            LecturerId = lecturers[index].Id
        });
    }

    if (newCourses.Count == 0)
    {
        return;
    }

    await context.Courses.AddRangeAsync(newCourses);
    await context.SaveChangesAsync();

    var today = DateOnly.FromDateTime(DateTime.UtcNow);
    var lectures = new List<Lecture>();
    foreach (var course in newCourses)
    {
        for (var daysAgo = 5; daysAgo >= 1; daysAgo--)
        {
            lectures.Add(new Lecture
            {
                CourseId = course.Id,
                ScheduledDate = today.AddDays(-daysAgo),
                StartTime = new TimeOnly(9, 0),
                EndTime = new TimeOnly(10, 30)
            });
        }
    }

    await context.Lectures.AddRangeAsync(lectures);
    await context.SaveChangesAsync();

    var attendanceRecords = new List<AttendanceRecord>();
    foreach (var lecture in lectures)
    {
        for (var studentIndex = 0; studentIndex < students.Count; studentIndex++)
        {
            var status = ((lecture.ScheduledDate.Day + studentIndex) % 4) switch
            {
                0 => AttendanceStatus.Present,
                1 => AttendanceStatus.Absent,
                2 => AttendanceStatus.Late,
                _ => AttendanceStatus.Excused
            };

            attendanceRecords.Add(new AttendanceRecord
            {
                LectureId = lecture.Id,
                StudentId = students[studentIndex].Id,
                Status = status,
                RecordedAt = DateTime.UtcNow
            });
        }
    }

    await context.AttendanceRecords.AddRangeAsync(attendanceRecords);
    await context.SaveChangesAsync();

    Console.WriteLine($"Additional test data created: {newCourses.Count} courses, {lectures.Count} lectures, {attendanceRecords.Count} attendance records.");
    Console.WriteLine("Additional test accounts use password: TestPassword123!");
}

static async Task<AttendanceUser> GetOrCreateSeedUserAsync(
    UserManager<AttendanceUser> userManager,
    string email,
    string name,
    string surname,
    UserRole role,
    string? studentNumber = null)
{
    var existingUser = await userManager.FindByEmailAsync(email);
    if (existingUser != null)
    {
        if (await userManager.HasPasswordAsync(existingUser))
        {
            var removePasswordResult = await userManager.RemovePasswordAsync(existingUser);
            if (!removePasswordResult.Succeeded)
            {
                var errors = string.Join(", ", removePasswordResult.Errors.Select(error => error.Description));
                throw new InvalidOperationException($"Could not reset test account {email}: {errors}");
            }
        }

        var addPasswordResult = await userManager.AddPasswordAsync(existingUser, "TestPassword123!");
        if (!addPasswordResult.Succeeded)
        {
            var errors = string.Join(", ", addPasswordResult.Errors.Select(error => error.Description));
            throw new InvalidOperationException($"Could not set password for test account {email}: {errors}");
        }

        if (!await userManager.IsInRoleAsync(existingUser, role.ToString()))
        {
            var existingRoleResult = await userManager.AddToRoleAsync(existingUser, role.ToString());
            if (!existingRoleResult.Succeeded)
            {
                var errors = string.Join(", ", existingRoleResult.Errors.Select(error => error.Description));
                throw new InvalidOperationException($"Could not assign role to test account {email}: {errors}");
            }
        }

        return existingUser;
    }

    var user = new AttendanceUser
    {
        UserName = email,
        Email = email,
        Name = name,
        Surname = surname,
        Role = role,
        StudentNumber = studentNumber,
        EmailConfirmed = true
    };

    var result = await userManager.CreateAsync(user, "TestPassword123!");
    if (!result.Succeeded)
    {
        var errors = string.Join(", ", result.Errors.Select(error => error.Description));
        throw new InvalidOperationException($"Could not create test account {email}: {errors}");
    }

    var roleResult = await userManager.AddToRoleAsync(user, role.ToString());
    if (!roleResult.Succeeded)
    {
        var errors = string.Join(", ", roleResult.Errors.Select(error => error.Description));
        throw new InvalidOperationException($"Could not assign role to test account {email}: {errors}");
    }

    return user;
}

public class NoOpEmailSender : IEmailSender
{
    public Task SendEmailAsync(string email, string subject, string htmlMessage)
    {
        return Task.CompletedTask;
    }
}
