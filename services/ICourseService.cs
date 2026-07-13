namespace TmsApi.Services;

using Tms.Api.Dtos;
using TmsApi.Entities;

public interface ICourseService
{
Task<CourseResponseDto?> GetByIdAsync(int id, CancellationToken ct);
Task<CourseResponseDto> CreateAsync(CreateCourseRequest course, CancellationToken ct);
Task<IEnumerable<Course>> GetAllAsync(CancellationToken ct);
Task<bool> DeleteAsync(int id, CancellationToken ct);
Task<bool> CodeExistsAsync(string code, CancellationToken ct);
Task<PagedResponse<CourseResponseDto>> GetCoursesAsync(PagedRequest request, CancellationToken ct);
}