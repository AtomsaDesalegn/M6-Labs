namespace TmsApi.Services;

using TmsApi.Entities;

public interface ICourseService
{
Task<Course?> GetByIdAsync(int id, CancellationToken ct);
Task<Course> CreateAsync(Course course, CancellationToken ct);
Task<IEnumerable<Course>> GetAllAsync(CancellationToken ct);
Task<bool> DeleteAsync(int id, CancellationToken ct);
}