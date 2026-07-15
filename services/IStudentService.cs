namespace TmsApi.Services;

using TmsApi.DTOs;

public interface IStudentService
{
    Task<StudentResponseDto?> GetByIdAsync(int id, CancellationToken ct);
    Task<(IEnumerable<StudentResponseDto> Items, int TotalCount)> GetPagedStudentsAsync(int page, int pageSize, CancellationToken ct);
    Task<IEnumerable<StudentResponseDto>> GetAllAsync(CancellationToken ct);
    Task<StudentResponseDto> CreateAsync(CreateStudentRequestDto requestDto, CancellationToken ct);
    Task<bool> RegistrationNumberExistsAsync(string regNum, CancellationToken ct);
    Task<bool> DeleteAsync(int id, CancellationToken ct);
}