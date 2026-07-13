using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Tms.Api.Dtos;
using TmsApi.Data;
using TmsApi.Entities;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace TmsApi.Services;

public class CourseService(TmsDbContext context, ILogger<CourseService> logger) : ICourseService
{
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
        logger.LogInformation("Created course {CourseId} ({Code})", course.Id, course.Code);
        return (await GetByIdAsync(course.Id, ct))!;
    }

    // Updated to handle true database-level pagination variables
    public async Task<(IEnumerable<CourseResponseDto> Items, int TotalCount)> GetPagedCoursesAsync(int page, int pageSize, CancellationToken ct)
    {
        // Query 1: Exactly one SELECT COUNT(*) database statement
        var totalCount = await context.Courses.CountAsync(ct);

        // Query 2: Exactly one paged SELECT ... LIMIT ... OFFSET database statement
        var items = await context.Courses
            .AsNoTracking()
            .OrderBy(c => c.Id) // Explicit sorting ensures consistent SQL offsetting
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(c => new CourseResponseDto(
                c.Id, c.Code, c.Title, c.MaxCapacity, c.Enrollments.Count))
            .ToListAsync(ct);

        return (items, totalCount);
    }

    // Fallback implementation to satisfy standard interface requirements if needed elsewhere
    public async Task<IEnumerable<Course>> GetAllAsync(CancellationToken ct)
    {
        return await context.Courses.AsNoTracking().ToListAsync(ct);
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken ct)
    {
        var course = await context.Courses.FindAsync([id], ct);
        if (course == null)
        {
            return false; 
        }

        context.Courses.Remove(course);
        await context.SaveChangesAsync(ct);
        return true; 
    }

    public Task<bool> CodeExistsAsync(string code, CancellationToken ct) =>
        context.Courses.AsNoTracking().AnyAsync(c => c.Code == code, ct);
}