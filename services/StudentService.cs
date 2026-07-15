using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TmsApi.Data;
using TmsApi.DTOs;
using TmsApi.Entities;

namespace TmsApi.services;

public class StudentService(TmsDbContext context, ILogger<StudentService> logger) : IStudentService
{
    public async Task<StudentResponseDto?> GetByIdAsync(int id, CancellationToken ct)
    {
        return await context.Students
            .AsNoTracking()
            .Where(s => s.Id == id)
            .Select(s => new StudentResponseDto(s.Id, s.RegistrationNumber, s.Name, s.GPA, s.Age, s.IsActive))
            .FirstOrDefaultAsync(ct);
    }
    
    public async Task<StudentResponseDto> CreateAsync(CreateStudentRequestDto requestDto, CancellationToken ct)
    {
        var student = new Student
        {
            RegistrationNumber = requestDto.RegistrationNumber,
            Name = requestDto.Name,
            GPA = requestDto.GPA,
            Age = requestDto.Age,
            IsActive = true
        };

        context.Students.Add(student);
        await context.SaveChangesAsync(ct);

        logger.LogInformation("Created student {StudentId} ({RegistrationNumber})", student.Id, student.RegistrationNumber);

        return(await GetByIdAsync(student.Id, ct))!;
    }

    public async Task<(IEnumerable<StudentResponseDto> Items, int TotalCount)> GetPagedStudentsAsync(int page, int pageSize, CancellationToken ct)
    {
        var totalCount = await context.Students.CountAsync(ct);
        var items = await context.Students
            .AsNoTracking()
            .OrderBy(s => s.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(s => new StudentResponseDto(s.Id, s.RegistrationNumber, s.Name, s.GPA, s.Age, s.IsActive))
            .ToListAsync(ct);
        
        return (items, totalCount);
    }

    public async Task<IEnumerable<StudentResponseDto>> GetAllAsync(CancellationToken ct)
    {
        return await context.Students
            .AsNoTracking()
            .Select(s => new StudentResponseDto(
                s.Id,
                s.RegistrationNumber,
                s.Name,
                s.GPA,
                s.Age,
                s.IsActive))
            .ToListAsync(ct);
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken ct)
    {
        var student = await context.Students.FindAsync([id], ct);
        if(student == null)
        {
            return false;
        }
        context.Students.Remove(student);
        await context.SaveChangesAsync(ct);
        return true;
    }

    public async Task<bool> RegistrationNumberExistsAsync(string regNum, CancellationToken ct) => 
        await context.Students
            .AsNoTracking()
            .AnyAsync(s => s.RegistrationNumber == regNum, ct);
}