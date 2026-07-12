using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Tms.Api.Dtos;
using TmsApi.Data;
using TmsApi.Entities;


namespace TmsApi.Services;

public class CourseService(TmsDbContext context, ILogger<CourseService> logger) : ICourseService
{
    // TODO 1: Use context.Courses.AsNoTracking() and return FirstOrDefaultAsync
    public async Task<CourseResponseDto?> GetByIdAsync(int id, CancellationToken ct)
    {
        return await context.Courses
            .AsNoTracking()
            .Include(c => c.Enrollments) 
            .Where(c => c.Id == id)
            .Select(c => new CourseResponseDto(
                c.Id, c.Code, c.Title, c.MaxCapacity, c.Enrollments.Count))
            .FirstOrDefaultAsync(ct);
    }

    // TODO 2: Add course to context.Courses, SaveChangesAsync(ct), log info, and return
    public async Task<CourseResponseDto> CreateAsync(CreateCourseRequest request, CancellationToken ct)
    {
        var course = new Course
        {
            Code = request.Code,
            Title = request.Title,
            MaxCapacity = request.MaxCapacity
        };
        context.Courses.Add(course);
        await context.SaveChangesAsync(ct);
        logger.LogInformation("Created course {CourseId} ({Code})", course.
        Id, course.Code);
        return (await GetByIdAsync(course.Id, ct))!;
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

    public Task<bool> CodeExistsAsync(string code, CancellationToken ct) =>
    context.Courses.AsNoTracking().AnyAsync(c => c.Code == code, ct);
}