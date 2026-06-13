namespace TmsApi.Services;
public class CourseService : ICourseService
{
    public readonly List<Course> _courses = new();  
    public List<Course> GetAllCourses()
    {
        return _courses;
    }

    public Course CreateCourse(Course newCourse)
    {
        _courses.Add(newCourse);
        return newCourse;
    }
}