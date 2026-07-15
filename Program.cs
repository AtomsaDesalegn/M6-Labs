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
using TmsApi.services;

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
// 🧰 SERVICES REGISTRATION (Where builder is active!)
// =========================================================================
builder.Services.AddControllers();

// Register TmsDbContext scoped for incoming HTTP requests using PostgreSQL
builder.Services.AddDbContext<TmsDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("TmsDatabase"))
    .LogTo(Console.WriteLine, LogLevel.Information) // Log SQL to output window
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
// 🚀 BUILD THE APPLICATION (This ends the life of 'builder')
// =========================================================================
var app = builder.Build();

// =========================================================================
// 🛣️ MIDDLEWARE PIPELINE ORDER (From Session 1)
// =========================================================================

// Placed on the absolute outside edge to catch the correct final HTTP status codes
app.UseMiddleware<RequestLoggingMiddleware>();

// GLOBAL EXCEPTION HANDLING: Intercepts raw crashes and transforms them into 
// clean JSON ProblemDetails format across all environments (Crucial for Exercise 3)
app.UseExceptionHandler();

// Turn empty status codes (like bare 404s/401s) into ProblemDetails JSON
app.UseStatusCodePages();

if (app.Environment.IsDevelopment())
{
    // In Development, map the interactive Scalar API documentation explorer
    app.MapOpenApi();
    app.MapScalarApiReference();

    // UNIFIED DATABASE STARTUP: Migrates the database and applies professional seed data
    using var scope = app.Services.CreateScope();
    var context = scope.ServiceProvider.GetRequiredService<TmsDbContext>();
    await DataSeeder.SeedAsync(context);
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

// EXERCISE 6: Map a test route '/api/error' that intentionally throws
app.MapGet("/api/error", () =>
{
    throw new InvalidOperationException("TMS System Failure: Could not connect to the Enrollment Database.");
});

//Exercise 7: Fixed with shaping (single round-trip)
app.MapGet("/api/exercise7", async (TmsDbContext db, CancellationToken cancellationToken) =>
{
    Console.WriteLine("=== Starting exercise 7: Part B(fixed with shaping) ===");

    //Fix: Single query with projection
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

app.Run();