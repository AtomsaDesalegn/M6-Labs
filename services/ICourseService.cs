namespace TmsApi.Services;

using TmsApi.Entities;

public interface ICourseService
{
    List<Course> GetAllCourses();
    Course? GetCourseById(int id); // 
    Course CreateCourse(Course newCourse);
    bool DeleteCourse(int id); // 
}