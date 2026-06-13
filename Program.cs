
using TmsApi.Services;
using TmsApi.services;
using TmsApi.Models;
using TmsApi.Controllers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging; 
using Scalar.AspNetCore; 
using Microsoft.AspNetCore.OpenApi; // 👈 ADD THIS LINE

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
builder.Services.AddSingleton<IEnrollmentService, EnrollmentService>();

// 💡 Add your new student service registration right here:
builder.Services.AddSingleton<IStudentService, StudentService>();

// Add your new Course service here
builder.Services.AddSingleton<ICourseService, CourseService>();

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

// 🛠️ EXERCISE 7 TODO 1 & TODO 3: Environment Conditional Tuning
if (app.Environment.IsDevelopment())
{
    // In Development, map the interactive Scalar API documentation explorer
    app.MapOpenApi();
    app.MapScalarApiReference();
}
else
{
    // 🔒 In Production, act strictly: force the exception handler to shield internal stack traces
    app.UseExceptionHandler(); 
}

// Turn empty status codes (like bare 404s/401s) into ProblemDetails JSON across all environments
app.UseStatusCodePages();

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

// 🧪 TEMPORARY TEST ROUTE FOR EXERCISE 4 LOGGING
app.MapGet("/api/logs-test", async (IEnrollmentService service) =>
{
    var rec = await service.EnrollAsync("S-001", "CS-101");
    await service.EnrollAsync("S-001", "CS-101");
    await service.GetByIdAsync("ghost-id");
    await service.DeleteAsync(rec.Id);

    return Results.Ok("Structured logs triggered in console!");
});

app.Run();