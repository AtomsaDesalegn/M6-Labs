using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging; // Added for explicit logging control

var builder = WebApplication.CreateBuilder(args);

// 👇 FORCE LOGS TO STREAM DIRECTLY INTO YOUR VS CODE TERMINAL 👇
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

builder.Services
    .AddAuthentication("Training")
    .AddScheme<AuthenticationSchemeOptions, TrainingAuthHandler>("Training", null);

builder.Services.AddAuthorization();

// Registering our Exercise 2 Services
builder.Services.AddSingleton<EnrollmentWorker>();
builder.Services.AddScoped<IEnrollmentService, EnrollmentService>();

// --- EXERCISE 3: Strongly-Typed Options & Startup Validation ---
builder.Services.AddOptions<PaymentOptions>()
    .BindConfiguration("Payments")         // Binds to the "Payments" block in appsettings.json
    .ValidateDataAnnotations()             // Validates [Required] and [Range] attributes
    .ValidateOnStart();                    // Forces validation to happen immediately at startup!

// =========================================================================
// 🚀 BUILD THE APPLICATION (This ends the life of 'builder')
// =========================================================================
var app = builder.Build();

// =========================================================================
// 🛣️ MIDDLEWARE PIPELINE ORDER (From Session 1)
// =========================================================================
app.UseMiddleware<RequestLoggingMiddleware>();
app.UseExceptionHandler("/error"); 
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

// 🧪 TEMPORARY TEST ROUTE FOR EXERCISE 4 LOGGING
app.MapGet("/api/logs-test", async (IEnrollmentService service) =>
{
    // 1. Trigger successful execution log (Information)
    var rec = await service.EnrollAsync("S-001", "CS-101");
    
    // 2. Trigger warning duplicate log (Warning)
    await service.EnrollAsync("S-001", "CS-101");
    
    // 3. Trigger missing record warning log (Warning)
    await service.GetByIdAsync("ghost-id");
    
    // 4. Trigger successful delete log (Information)
    await service.DeleteAsync(rec.Id);

    return Results.Ok("Structured logs triggered in console!");
});

app.Run();