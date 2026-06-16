using Microsoft.AspNetCore.Mvc;
using TmsApi.Services;
using TmsApi.Models;

namespace TmsApi.Controllers;

[ApiController]
[Route("api/courses")]
public class CourseController : ControllerBase
{
    private readonly ICourseService _courseService;

    public CourseController(ICourseService courseService)
    {
        _courseService = courseService;
    }

    // 1. GET ALL COURSES -> GET api/courses
    [HttpGet]
    public ActionResult<List<Course>> GetAll()
    {
        return Ok(_courseService.GetAllCourses());
    }

    // 2. GET COURSE BY ID -> GET api/courses/{id}
    [HttpGet("{id}")] // 🆕 Added
    public ActionResult<Course> GetById(string id)
    {
        var course = _courseService.GetCourseById(id);
        if (course == null) return NotFound($"Course with ID {id} not found.");
        return Ok(course);
    }

    // 3. CREATE A NEW COURSE -> POST api/courses
    [HttpPost]
    public ActionResult<Course> Create([FromBody] Course newCourse)
    {
        if (newCourse == null) return BadRequest("Course data cannot be empty.");

        var createdCourse = _courseService.CreateCourse(newCourse);
        
        // Fixed: Points to GetById now for REST standard compliance
        return CreatedAtAction(nameof(GetById), new { id = createdCourse.Id }, createdCourse);
    }

    // 4. DELETE A COURSE -> DELETE api/courses/{id}
    [HttpDelete("{id}")] // 🆕 Added
    public ActionResult Delete(string id)
    {
        var deleted = _courseService.DeleteCourse(id);
        if (!deleted) return NotFound($"Course with ID {id} not found.");
        
        return NoContent(); // 204 Standard response for successful deletes
    }
}