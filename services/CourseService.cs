using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TmsApi.Data;
using TmsApi.Entities;

namespace TmsApi.Services;

public class CourseService(TmsDbContext context, ILogger<CourseService> logger) : ICourseService
{
    // TODO 1: Use context.Courses.AsNoTracking() and return FirstOrDefaultAsync
    public async Task<Course?> GetByIdAsync(int id, CancellationToken ct)
    {
        return await context.Courses
            .AsNoTracking() // Performance optimization for read-only tracking
            .FirstOrDefaultAsync(c => c.Id == id, ct);
    }

    // TODO 2: Add course to context.Courses, SaveChangesAsync(ct), log info, and return
    public async Task<Course> CreateAsync(Course course, CancellationToken ct)
    {
        context.Courses.Add(course);

        await context.SaveChangesAsync(ct);

        logger.LogInformation("Successfully created a new course: {Title} ({Code})", course.Title, course.Code);

        return course;
    }

    // 🆕 Add this implementation:
    public async Task<IEnumerable<Course>> GetAllAsync(CancellationToken ct)
    {
        return await context.Courses
            .ToListAsync(ct);
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken ct)
    {
        var course = await context.Courses.FindAsync([id], ct);
        if (course == null)
        {
            return false; // Course not found
        }

        context.Courses.Remove(course);
        await context.SaveChangesAsync(ct);
        return true; // Successfully deleted
    }
}