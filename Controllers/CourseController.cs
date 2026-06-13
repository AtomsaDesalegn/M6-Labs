using Microsoft.AspNetCore.Mvc;
using TmsApi.Services;
using TmsApi.Models;

namespace TmsApi.Controllers;

[ApiController]
[Route("api/courses")] // 🎯 Fixed: Explicit route path added
public class CourseController : ControllerBase
{
    private readonly ICourseService _courseService;

    public CourseController(ICourseService courseService)
    {
        _courseService = courseService;
    }

    // 1. GET ALL COURSES
    [HttpGet]
    public ActionResult<List<Course>> GetAll()
    {
        var courses = _courseService.GetAllCourses();
        return Ok(courses);
    }

    // 2. CREATE A NEW COURSE
    [HttpPost]
    public ActionResult<Course> Create([FromBody] Course newCourse)
    {
        if (newCourse == null)
        {
            return BadRequest("Course data cannot be empty.");
        }

        var createdCourse = _courseService.CreateCourse(newCourse);

        return CreatedAtAction(nameof(GetAll), new { id = createdCourse.Id }, createdCourse);
    }
}