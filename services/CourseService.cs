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

    public async Task<PagedResponse<CourseResponseDto>> GetCoursesAsync(
    PagedRequest request, CancellationToken ct)
    {
        // TODO 1: Start with a no-tracking IQueryable
        IQueryable<Course> query = context.Courses.AsNoTracking();

        // TODO 2: Case-insensitive search via PostgreSQL ILike
        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            string searchPattern = $"%{request.Search}%";
            query = query.Where(c => EF.Functions.ILike(c.Title, searchPattern)
                                  || EF.Functions.ILike(c.Code, searchPattern));
        }

        // TODO 3: Count BEFORE paging
        int totalCount = await query.CountAsync(ct);

        // TODO 4: Apply whitelisted OrderBy, then Descending if requested
        // Fall back to "Title" if the requested sort column is missing or unknown
        string sortColumn = request.OrderBy?.Trim() ?? "Title";

        query = sortColumn switch
        {
            "Code" => request.Descending ? query.OrderByDescending(c => c.Code) : query.OrderBy(c => c.Code),
            "MaxCapacity" => request.Descending ? query.OrderByDescending(c => c.MaxCapacity) : query.OrderBy(c => c.MaxCapacity),
            _ => request.Descending ? query.OrderByDescending(c => c.Title) : query.OrderBy(c => c.Title)
        };

        // TODO 5 & 6: Skip/Take, project, materialize, and return the PagedResponse
        var items = await query
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(c => new CourseResponseDto(
                c.Id,
                c.Code,
                c.Title,
                c.MaxCapacity,
                c.Enrollments.Count))
            .ToListAsync(ct);

        return new PagedResponse<CourseResponseDto>
        {
            Items = items,
            TotalCount = totalCount,
            Page = request.Page,
            PageSize = request.PageSize
        };
    }
}