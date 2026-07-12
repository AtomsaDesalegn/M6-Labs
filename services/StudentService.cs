using Microsoft.EntityFrameworkCore;
using TmsApi.Models;
using TmsApi.Data;
using TmsApi.Services;

namespace TmsApi.Services;

public class StudentService(TmsDbContext context) : IStudentService
{
    // 1. Get All Students from PostgreSQL and map to API Models
    public async Task<IEnumerable<TmsApi.Models.Student>> GetAllAsync()
    {
        var dbStudents = await context.Students.ToListAsync();

        // Map list of Entities.Student to IEnumerable<Models.Student>
        return dbStudents.Select(s => new TmsApi.Models.Student
        {
            Id = s.Id.ToString(), // Converts int Id to string format
            Name = s.Name,
            Age = 20,             // Default fallback age required by your model spec
            GPA = s.GPA,
            Version = s.Version
        });
    }

    // 2. Get Student By ID and map to API Model
    public async Task<TmsApi.Models.Student?> GetByIdAsync(string id)
    {
        if (!int.TryParse(id, out int numericId)) return null;

        var dbStudent = await context.Students.FirstOrDefaultAsync(s => s.Id == numericId);
        if (dbStudent == null) return null;

        // Map single Entities.Student to Models.Student
        return new TmsApi.Models.Student
        {
            Id = dbStudent.Id.ToString(),
            Name = dbStudent.Name,
            Age = 20,
            GPA = dbStudent.GPA,
            Version = dbStudent.Version
        };
    }

    // 3. Create a New Student in PostgreSQL and return as API Model
    public async Task<TmsApi.Models.Student> CreateAsync(string name, int age, decimal gpa)
    {
        // Notice we are explicitly instantiating the database entity type here to save it
        var newDbStudent = new TmsApi.Entities.Student
        {
            Name = name,
            GPA = gpa,
            RegistrationNumber = $"TMS-2026-{Guid.NewGuid().ToString()[..4].ToUpper()}"
        };

        context.Students.Add(newDbStudent);
        await context.SaveChangesAsync();

        // Map and return it back as the required API model type
        return new TmsApi.Models.Student
        {
            Id = newDbStudent.Id.ToString(),
            Name = newDbStudent.Name,
            Age = age,
            GPA = newDbStudent.GPA
        };
    }

    // 4. Delete a Student from PostgreSQL
    public async Task<bool> DeleteAsync(string id)
    {
        if (!int.TryParse(id, out int numericId)) return false;

        var student = await context.Students.FindAsync(numericId);
        if (student == null) return false;

        context.Students.Remove(student);
        await context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> UpdateAsync(string id, string name, decimal gpa, uint version)
    {
        if (!int.TryParse(id, out int numericId)) return false;

        // Fetch the existing record from the database
        var dbStudent = await context.Students.FirstOrDefaultAsync(s => s.Id == numericId);
        if (dbStudent == null) return false;

        // Apply the incoming values to the tracked entity
        dbStudent.Name = name;
        dbStudent.GPA = gpa;

        // Set the original version token we received from the client.
        // If this version doesn't match the database's xmin, EF Core throws DbUpdateConcurrencyException
        context.Entry(dbStudent).Property(s => s.Version).OriginalValue = version;

        // Manually update our hidden shadow property right before saving
        context.Entry(dbStudent).Property("LastUpdated").CurrentValue = DateTime.UtcNow;

        await context.SaveChangesAsync();
        return true;
    }

    public async Task<IEnumerable<TmsApi.Entities.Student>> GetDeletedStudentsAsync()
    {
        // 👑 Bypasses the global query filter to return ONLY soft-deleted records
        return await context.Students
            .IgnoreQueryFilters()
            .Where(s => s.IsDeleted)
            .ToListAsync();
    }

    public async Task<IEnumerable<TmsApi.Models.Student>> GetPagedStudentsAsync(int pageNumber, int pageSize = 20)
    {
        if (pageNumber < 1) pageNumber = 1;

        return await context.Students
            .OrderBy(s => s.Name)
            .ThenBy(s => s.Id)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(s => new TmsApi.Models.Student
            {
                Id = s.Id.ToString(),
                Name = s.Name,
                Age = 20, // Default fallback age matching your other endpoints
                GPA = s.GPA
            })
            .ToListAsync(); // Placed perfectly at the end
    }
}