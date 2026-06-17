namespace TmsApi.Services;

using TmsApi.Entities; 

public class CourseService : ICourseService
{
    // Using a static list if this is a transient service, so data persists between requests
    private static readonly List<Course> _courses = new();  

    public List<Course> GetAllCourses() => _courses;

    public Course? GetCourseById(int id) // Changed string -> int
    {
        return _courses.FirstOrDefault(c => c.Id == id);
    }

    public Course CreateCourse(Course newCourse)
    {
        _courses.Add(newCourse);
        return newCourse;
    }

    public bool DeleteCourse(int id) // Changed string -> int
    {
        var course = GetCourseById(id);
        if (course == null) return false;
        
        _courses.Remove(course);
        return true;
    }
}