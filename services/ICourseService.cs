namespace TmsApi.Services;
using TmsApi.Models;

public interface ICourseService
{
    List<Course> GetAllCourses();
    Course? GetCourseById(string id); // 🆕 Added
    Course CreateCourse(Course newCourse);
    bool DeleteCourse(string id); // 🆕 Added
}