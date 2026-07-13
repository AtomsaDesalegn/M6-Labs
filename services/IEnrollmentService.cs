using System.Threading;
using System.Threading.Tasks;
using Tms.Api.Dtos;

namespace Tms.Api.Services;

public interface IEnrollmentService
{
    Task<EnrollmentResponseDto?> GetByIdAsync(int courseId, int id, CancellationToken ct);
    Task<EnrollmentResponseDto> CreateAsync(int courseId, EnrollStudentRequest request, CancellationToken ct);
    Task<IEnumerable<EnrollmentResponseDto>> GetByCourseIdAsync(int courseId, CancellationToken ct);
}