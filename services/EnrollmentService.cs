using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using TmsApi.Data; // 

// --- The contract ---
public interface IEnrollmentService
{
    Task<EnrollmentRecord> EnrollAsync(string studentId, string courseCode);
    Task<EnrollmentRecord?> GetByIdAsync(string id);
    Task<IReadOnlyList<EnrollmentRecord>> GetAllAsync();
    Task<bool> DeleteAsync(string id);

    Task<int> ArchiveOldEnrollmentsAsync(DateTime cutoffDate);
}

// --- The database-connected implementation ---
public class EnrollmentService : IEnrollmentService
{
    private readonly TmsDbContext _context; // 👈 Wired up your real DbContext
    private readonly ILogger<EnrollmentService> _logger;

    public EnrollmentService(TmsDbContext context, ILogger<EnrollmentService> logger)
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

        // Check for duplicate enrollment inside PostgreSQL
        var existing = await _context.Enrollments
            .FirstOrDefaultAsync(e => e.StudentId == numericStudentId && e.CourseId == numericCourseId);

        if (existing is not null)
        {
            _logger.LogWarning(
                "Duplicate enrollment attempt {StudentId} already in {CourseCode} (record {EnrollmentId})",
                studentId, courseCode, existing.Id);

            return new EnrollmentRecord(existing.Id.ToString(), studentId, courseCode, existing.EnrolledAt);
        }

        // Create a new entity item to push down to Postgres
        var newEntity = new TmsApi.Entities.Enrollment
        {
            StudentId = numericStudentId,
            CourseId = numericCourseId,
            Grade = 0.0m, // Set default starting grade
            EnrolledAt = DateTime.UtcNow
        };

        _context.Enrollments.Add(newEntity);
        await _context.SaveChangesAsync(); // Commit row to disk

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
        // Pull directly from the live PostgreSQL table!
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
        // ⚡ Bulk update executes directly on the database in 1 single SQL command
        int rowsAffected = await _context.Enrollments
            .Where(e => e.EnrolledAt < cutoffDate && !e.IsArchived)
            .ExecuteUpdateAsync(s => s.SetProperty(e => e.IsArchived, true));

        return rowsAffected;
    }
}

// --- The data shape (Kept exact signature to prevent breaking contracts) ---
public record EnrollmentRecord(
    string Id,
    string StudentId,
    string CourseCode,
    DateTime EnrolledAt);