namespace TmsApi.Services;
using TmsApi.Models;

public class CourseService : ICourseService
{
    // Note: Using a static list if this is a transient service, so data persists between requests
    private static readonly List<Course> _courses = new();  

    public List<Course> GetAllCourses() => _courses;

    public Course? GetCourseById(string id) // 🆕 Added
    {
        return _courses.FirstOrDefault(c => c.Id == id);
    }

    public Course CreateCourse(Course newCourse)
    {
        _courses.Add(newCourse);
        return newCourse;
    }

    public bool DeleteCourse(string id) // 🆕 Added
    {
        var course = GetCourseById(id);
        if (course == null) return false;
        
        _courses.Remove(course);
        return true;
    }
}