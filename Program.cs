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
using Microsoft.EntityFrameworkCore;
using TmsApi.Data;
using Tms.Api.Services;
using Tms.Api.Filters;
using Tms.Api.Persistence;

var builder = WebApplication.CreateBuilder(args);

// FORCE LOGS TO STREAM DIRECTLY INTO YOUR VS CODE TERMINAL
builder.Logging.ClearProviders();
builder.Logging.AddConsole();

// --- Turn on Strict Container Architecture Validation ---
builder.Host.UseDefaultServiceProvider(options =>
{
    options.ValidateScopes = true;
    options.ValidateOnBuild = true;
});

// =========================================================================
// 🧰 SERVICES REGISTRATION
// =========================================================================
builder.Services.AddControllers(options =>
{
    options.Filters.Add<AuditLogFilter>();
});

builder.Services.AddDbContext<TmsDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("TmsDatabase"))
    .LogTo(Console.WriteLine, LogLevel.Information)
    .EnableSensitiveDataLogging());

builder.Services.AddProblemDetails();
builder.Services.AddOpenApi();

builder.Services
    .AddAuthentication("Training")
    .AddScheme<AuthenticationSchemeOptions, TrainingAuthHandler>("Training", null);

builder.Services.AddAuthorization();

// Register Services
builder.Services.AddSingleton<EnrollmentWorker>();
builder.Services.AddScoped<IEnrollmentService, EnrollmentService>();
builder.Services.AddScoped<IStudentService, StudentService>();
builder.Services.AddScoped<ICourseService, CourseService>();

builder.Services.AddOptions<PaymentOptions>()
    .BindConfiguration("Payments")
    .ValidateDataAnnotations()
    .ValidateOnStart();

// =========================================================================
// 🚀 BUILD AND MIDDLEWARE
// =========================================================================
var app = builder.Build();

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

// Endpoints
app.MapGet("/api/assessments/results", () => Results.Ok(new
{
    courseCode = "CS-101",
    studentId = "S-001",
    letterGrade = "A"
})).RequireAuthorization();

app.MapControllers();

app.MapGet("/api/error", () =>
{
    throw new InvalidOperationException("TMS System Failure: Could not connect to the Enrollment Database.");
});

// Exercise 7: Fixed with shaping
app.MapGet("/api/exercise7", async (TmsDbContext db, CancellationToken cancellationToken) =>
{
    var report = await db.Students
        .AsNoTracking()
        .Select(s => new { s.Name, EnrollmentCount = s.Enrollments.Count })
        .ToListAsync(cancellationToken);

    return Results.Ok(report);
});

// =========================================================================
// 🗄️ AUTOMATED DATABASE MIGRATION & SEED DATA ENGINE
// =========================================================================
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<TmsDbContext>();
    context.Database.Migrate();
    await DataSeeder.SeedAsync(context);
}

app.Run();