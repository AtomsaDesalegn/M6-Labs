using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using TmsApi.Data;
using Tms.Api.Dtos;
using Tms.Api.Services;
using TmsApi.Entities; 

public class EnrollmentService(TmsDbContext context, ILogger<EnrollmentService> logger) : IEnrollmentService
{
    public Task<EnrollmentResponseDto?> GetByIdAsync(int courseId, int id, CancellationToken ct) =>
        context.Enrollments
            .AsNoTracking()
            .Where(e => e.Id == id && e.CourseId == courseId)
            .Select(e => new EnrollmentResponseDto(e.Id, e.CourseId, e.StudentId, e.EnrolledAt))
            .FirstOrDefaultAsync(ct);

    public async Task<EnrollmentResponseDto> CreateAsync(int courseId, EnrollStudentRequest request, CancellationToken ct)
    {
        // 1. Manually check the database for the current highest ID and increment it by 1
        var nextId = context.Enrollments.Any() ? context.Enrollments.Max(e => e.Id) + 1 : 1;

        // 2. Build the new enrollment with the manual ID explicitly assigned
        var enrollment = new Enrollment
        {
            Id = nextId,
            CourseId = courseId,
            StudentId = request.StudentId,
            EnrolledAt = DateTime.UtcNow
        };
        
        context.Enrollments.Add(enrollment);
        await context.SaveChangesAsync(ct);

        logger.LogInformation("Successfully created enrollment for student {StudentId} in course {CourseId}.", request.StudentId, courseId);

        var result = await GetByIdAsync(courseId, enrollment.Id, ct);

        return result ?? throw new InvalidOperationException("Failed to retrieve the newly created enrollment.");
    }

    public async Task<IEnumerable<EnrollmentResponseDto>> GetByCourseIdAsync(int courseId, CancellationToken ct) =>
        await context.Enrollments
            .AsNoTracking()
            .Where(e => e.CourseId == courseId)
            .Select(e => new EnrollmentResponseDto(e.Id, e.CourseId, e.StudentId, e.EnrolledAt))
            .ToListAsync(ct);
}

/* 
// --- OLD OUTDATED IMPLEMENTATION (COMMENTED OUT TO PREVENT ERRORS) ---

public class OldEnrollmentService
{
    private readonly TmsDbContext _context;
    private readonly ILogger<EnrollmentService> _logger;

    public OldEnrollmentService(TmsDbContext context, ILogger<EnrollmentService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<EnrollmentRecord> EnrollAsync(string studentId, string courseCode)
    {
        if (!int.TryParse(studentId, out int numericStudentId) || !int.TryParse(courseCode, out int numericCourseId))
        {
            throw new ArgumentException("Student ID and Course Code must be numeric integers for the database.");
        }

        var existing = await _context.Enrollments
            .FirstOrDefaultAsync(e => e.StudentId == numericStudentId && e.CourseId == numericCourseId);

        if (existing is not null)
        {
            _logger.LogWarning(
                "Duplicate enrollment attempt {StudentId} already in {CourseCode} (record {EnrollmentId})",
                studentId, courseCode, existing.Id);

            return new EnrollmentRecord(existing.Id.ToString(), studentId, courseCode, existing.EnrolledAt);
        }

        var newEntity = new TmsApi.Entities.Enrollment
        {
            StudentId = numericStudentId,
            CourseId = numericCourseId,
            Grade = 0.0m,
            EnrolledAt = DateTime.UtcNow
        };

        _context.Enrollments.Add(newEntity);
        await _context.SaveChangesAsync();

        _logger.LogInformation(
            "Enrolled {StudentId} in {CourseCode} record {EnrollmentId}",
            studentId, courseCode, newEntity.Id);

        return new EnrollmentRecord(newEntity.Id.ToString(), studentId, courseCode, newEntity.EnrolledAt);
    }

    public async Task<EnrollmentRecord?> GetByIdAsync(string id)
    {
        if (!int.TryParse(id, out int numericId)) return null;

        var record = await _context.Enrollments.FirstOrDefaultAsync(e => e.Id == numericId);
        if (record is null)
        {
            _logger.LogWarning("Enrollment {EnrollmentId} not found", id);
            return null;
        }

        return new EnrollmentRecord(record.Id.ToString(), record.StudentId.ToString(), record.CourseId.ToString(), record.EnrolledAt);
    }

    public async Task<IReadOnlyList<EnrollmentRecord>> GetAllAsync()
    {
        var dbEnrollments = await _context.Enrollments.ToListAsync();

        return dbEnrollments.Select(e => new EnrollmentRecord(
            e.Id.ToString(),
            e.StudentId.ToString(),
            e.CourseId.ToString(),
            e.EnrolledAt
        )).ToList();
    }

    public async Task<bool> DeleteAsync(string id)
    {
        if (!int.TryParse(id, out int numericId)) return false;

        var enrollment = await _context.Enrollments.FindAsync(numericId);
        if (enrollment == null)
        {
            _logger.LogWarning("Delete failed enrollment {EnrollmentId} not found", id);
            return false;
        }

        _context.Enrollments.Remove(enrollment);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Deleted enrollment {EnrollmentId}", id);
        return true;
    }

    public async Task<int> ArchiveOldEnrollmentsAsync(DateTime cutoffDate)
    {
        int rowsAffected = await _context.Enrollments
            .Where(e => e.EnrolledAt < cutoffDate && !e.IsArchived)
            .ExecuteUpdateAsync(s => s.SetProperty(e => e.IsArchived, true));

        return rowsAffected;
    }
}
*/

// --- The data shape ---
public record EnrollmentRecord(
    string Id,
    string StudentId,
    string CourseCode,
    DateTime EnrolledAt);