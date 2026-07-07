namespace TmsApi.services;
using TmsApi.Models;

public interface IStudentService
{
    Task<IEnumerable<Models.Student>> GetAllAsync();
    Task<Models.Student?> GetByIdAsync(string id);

    // 💡 ADD THIS LINE TO THE CONTRACT:
    Task<Student> CreateAsync(string name, int age, decimal gpa);

    // 💡 ADD THIS NEW LINE TO THE CONTRACT:
    Task<bool> DeleteAsync(string id);

    Task<bool> UpdateAsync(string id, string name, decimal gpa, uint version);
    Task<IEnumerable<TmsApi.Entities.Student>> GetDeletedStudentsAsync();
}