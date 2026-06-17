using Microsoft.AspNetCore.Mvc;
using TmsApi.Services;
using TmsApi.Entities; //  Updated to point to your correct entities namespace

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
    [HttpGet("{id}")]
    public ActionResult<Course> GetById(int id) // Changed string to int
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
        
        return CreatedAtAction(nameof(GetById), new { id = createdCourse.Id }, createdCourse);
    }

    // 4. DELETE A COURSE -> DELETE api/courses/{id}
    [HttpDelete("{id}")]
    public ActionResult Delete(int id) //Changed string to int
    {
        var deleted = _courseService.DeleteCourse(id);
        if (!deleted) return NotFound($"Course with ID {id} not found.");
        
        return NoContent(); 
    }
}