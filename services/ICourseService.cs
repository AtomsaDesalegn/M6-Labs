namespace TmsApi.Services;
public interface ICourseService
{
    List<Course> GetAllCourses();
    Course CreateCourse(Course newCourse);
}