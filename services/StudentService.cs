using TmsApi.Models;

namespace TmsApi.services;

public class StudentService : IStudentService
{
    // A temporary in-memory list to act as your database for now
    private readonly List<Student> _students = new()
    {
        new Student { Id = "STU-001", Name = "Abeba", Age = 20, GPA = 3.8m },
        new Student { Id = "STU-002", Name = "Chala", Age = 22, GPA = 3.5m }
    };

    public async Task<IEnumerable<Student>> GetAllAsync()
    {
        await Task.Delay(10); // Simulate a fast async database fetch
        return _students;
    }

    public async Task<Student?> GetByIdAsync(string id)
    {
        await Task.Delay(10);
        return _students.FirstOrDefault(s => s.Id == id);
    }

    // 3. Create a New Student 💡 (Added to fix your exact error)
    public async Task<Student> CreateAsync(string name, int age, decimal gpa)
    {
        await Task.Delay(10);
        
        // Generate next ID based on count
        var nextId = $"STU-{_students.Count + 1:D3}";
        
        var newStudent = new Student 
        { 
            Id = nextId, 
            Name = name, 
            Age = age, 
            GPA = gpa 
        };
        
        _students.Add(newStudent);
        return newStudent;
    }

    // 4. Delete a Student 💡 (Added to keep your contract fully satisfied)
    public async Task<bool> DeleteAsync(string id)
    {
        await Task.Delay(10);
        
        var student = _students.FirstOrDefault(s => s.Id == id);
        if (student == null) 
        {
            return false;
        }

        _students.Remove(student);
        return true;
    }
}