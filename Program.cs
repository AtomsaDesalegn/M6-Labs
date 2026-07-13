using TmsApi.Services;
using TmsApi.Models;
using TmsApi.Entities;
using TmsApi.Controllers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Scalar.AspNetCore;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.EntityFrameworkCore;
using TmsApi.Data;
using Tms.Api.Services;
using Tms.Api.Persistence;

var builder = WebApplication.CreateBuilder(args);

// FORCE LOGS TO STREAM DIRECTLY INTO YOUR VS CODE TERMINAL
builder.Logging.ClearProviders();
builder.Logging.AddConsole();

// --- Turn on Strict Container Architecture Validation (From Exercise 2) ---
builder.Host.UseDefaultServiceProvider(options =>
{
    options.ValidateScopes = true;
    options.ValidateOnBuild = true;
});

// =========================================================================
// 🧰 SERVICES REGISTRATION
// =========================================================================
builder.Services.AddControllers();

// Register TmsDbContext scoped for incoming HTTP requests using PostgreSQL
builder.Services.AddDbContext<TmsDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("TmsDatabase"))
    .LogTo(Console.WriteLine, LogLevel.Information)
    .EnableSensitiveDataLogging());

// EXERCISE 6: Add the ProblemDetails service to the DI container
builder.Services.AddProblemDetails();

// 🛠️ EXERCISE 7 TODO 2a: Register OpenAPI document generation services
builder.Services.AddOpenApi();

builder.Services
    .AddAuthentication("Training")
    .AddScheme<AuthenticationSchemeOptions, TrainingAuthHandler>("Training", null);

builder.Services.AddAuthorization();

// Registering our Exercise 2 Services
builder.Services.AddSingleton<EnrollmentWorker>();
builder.Services.AddScoped<IEnrollmentService, EnrollmentService>();

// Scoped Student Service
builder.Services.AddScoped<IStudentService, StudentService>();

// Course service registration
builder.Services.AddScoped<ICourseService, CourseService>();

// --- EXERCISE 3: Strongly-Typed Options & Startup Validation ---
builder.Services.AddOptions<PaymentOptions>()
    .BindConfiguration("Payments")
    .ValidateDataAnnotations()
    .ValidateOnStart();

// =========================================================================
// 🚀 BUILD THE APPLICATION
// =========================================================================
var app = builder.Build();

// =========================================================================
// 🛣️ MIDDLEWARE PIPELINE ORDER
// =========================================================================

app.UseMiddleware<RequestLoggingMiddleware>();
app.UseExceptionHandler();
app.UseStatusCodePages();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

// Protected Endpoint
app.MapGet("/api/assessments/results", () => Results.Ok(new
{
    courseCode = "CS-101",
    studentId = "S-001",
    letterGrade = "A"
})).RequireAuthorization();

app.MapControllers();

// EXERCISE 6: Map a test route '/api/error'
app.MapGet("/api/error", () =>
{
    throw new InvalidOperationException("TMS System Failure: Could not connect to the Enrollment Database.");
});

// Exercise 7: Fixed with shaping (single round-trip)
app.MapGet("/api/exercise7", async (TmsApi.Data.TmsDbContext db, CancellationToken cancellationToken) =>
{
    Console.WriteLine("=== Starting exercise 7: Part B(fixed with shaping) ===");

    var report = await db.Students
        .AsNoTracking()
        .Select(s => new
        {
            s.Name,
            EnrollmentCount = s.Enrollments.Count
        })
        .ToListAsync(cancellationToken);

    foreach (var r in report)
    {
        Console.WriteLine($"{r.Name}: {r.EnrollmentCount} enrollments");
    }
    return Results.Ok("Part B complete. Check your terminal logs for a single SQL statement!");
});

// =========================================================================
// 🗄️ AUTOMATED DATABASE MIGRATION & SEED DATA ENGINE
// =========================================================================
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<TmsDbContext>();

    context.Database.Migrate();

    if (!context.Students.Any())
    {
        var students = new List<TmsApi.Entities.Student>
        {
            new() { RegistrationNumber = "TMS-2026-0001", Name = "Alice Smith", GPA = 3.8m, IsActive = true },
            new() { RegistrationNumber = "TMS-2026-0002", Name = "Bob Jones", GPA = 2.9m, IsActive = true },
            new() { RegistrationNumber = "TMS-2026-0003", Name = "Charlie Brown", GPA = 3.4m, IsActive = false },
            new() { RegistrationNumber = "TMS-2026-0004", Name = "Diana Prince", GPA = 3.9m, IsActive = true },
            new() { RegistrationNumber = "TMS-2026-0005", Name = "Evan Wright", GPA = 2.5m, IsActive = true }
        };

        var courses = new List<Course>
        {
            new() { Code = "CS-101", Title = "Introduction to Computer Science", MaxCapacity = 30 },
            new() { Code = "CS-201", Title = "Data Structures and Algorithms", MaxCapacity = 25 },
            new() { Code = "MAT-101", Title = "Calculus I", MaxCapacity = 40 }
        };

        try
        {
            context.Students.AddRange(students);
            context.Courses.AddRange(courses);
            context.SaveChanges();

            var enrollments = new List<Enrollment>
            {
                new() { StudentId = students[0].Id, CourseId = courses[0].Id, Grade = 4.0m },
                new() { StudentId = students[0].Id, CourseId = courses[1].Id, Grade = 3.6m },
                new() { StudentId = students[1].Id, CourseId = courses[0].Id, Grade = 2.8m },
                new() { StudentId = students[3].Id, CourseId = courses[1].Id, Grade = 3.9m }
            };
            context.Enrollments.AddRange(enrollments);

            var assessments = new List<Assessment>
            {
                new() { Title = "Midterm Quiz", MaxScore = 100, Weight = 0.30m, CourseId = courses[0].Id },
                new() { Title = "Final Exam", MaxScore = 100, Weight = 0.70m, CourseId = courses[0].Id },
                new() { Title = "Coding Challenge 1", MaxScore = 50, Weight = 0.20m, CourseId = courses[1].Id }
            };
            context.Assessments.AddRange(assessments);

            var certificates = new List<Certificate>
            {
                new()
                {
                    SerialNumber = "TMS-2026-CERT-0001",
                    IssuedAt = DateTime.UtcNow,
                    StudentId = students[0].Id,
                    CourseId = courses[0].Id
                }
            };
            context.Certificates.AddRange(certificates);

            context.SaveChanges();
        }
        catch (Exception ex)
        {
            Console.WriteLine("==================================================");
            Console.WriteLine("❌ DATABASE SEEDING ERROR DETAILS:");
            Console.WriteLine(ex.InnerException?.Message ?? ex.Message);
            Console.WriteLine("==================================================");
            throw;
        }
    }
}
if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var context = scope.ServiceProvider.GetRequiredService<TmsDbContext>();
    await DataSeeder.SeedAsync(context);
}
app.Run();