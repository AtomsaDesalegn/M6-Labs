using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

public class EnrollmentWorker
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<EnrollmentWorker> _logger;

    // FIX: Inject the safe ServiceScopeFactory instead of the scoped service directly
    public EnrollmentWorker(IServiceScopeFactory scopeFactory, ILogger<EnrollmentWorker> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    public void ProcessBatch()
    {
        _logger.LogInformation("⚙️ Background scholarship worker batch execution started...");

        // 1. Create a short-lived scope boundary
        using (var scope = _scopeFactory.CreateScope())
        {
            // 2. Safely resolve the scoped service from this temporary boundary provider
            var enrollmentService = scope.ServiceProvider.GetRequiredService<IEnrollmentService>();

            // 3. Simulate processing work safely inside the scope
            _logger.LogInformation("🔒 Scope created. Resolving dependencies safely...");
            
            // (Your background processing logic goes here)
        } 
        // 4. The 'using' block ends here. The scope is instantly disposed, 
        // and any scoped services created inside it are completely cleared from memory!
        
        _logger.LogInformation("🔓 Scope disposed. Memory completely cleared.");
    }
}