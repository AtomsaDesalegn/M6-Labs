using Microsoft.AspNetCore.Mvc;
using TmsApi.Services;
using TmsApi.Entities; //  Updated to point to your correct entities namespace

namespace TmsApi.Controllers;

[ApiController]
[Route("api/courses")]
public class CourseController(ICourseService courseService) : ControllerBase
{
    [HttpGet("{id:int}", Name = nameof(GetCourseById))]
    public async Task<IActionResult> GetCourseById(int id, CancellationToken ct)
    {
        var course = await courseService.GetByIdAsync(id, ct);
        if (course is null)
        {
            return NotFound();
        }

        return Ok(course);

        throw new NotImplementedException();
    }

    [HttpPost]

    public async Task<IActionResult> CreateCourse(Course course, CancellationToken ct)
    {
        var result = await courseService.CreateAsync(course, ct);

        return CreatedAtAction(nameof(GetCourseById), new { id = result.Id }, result);

        throw new NotImplementedException();
    }

    [HttpGet]
    public async Task<IActionResult> GetAllCourses(CancellationToken ct)
    {
        var courses = await courseService.GetAllAsync(ct); // Assumes your service has this method
        return Ok(courses);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteCourse(int id, CancellationToken ct)
    {
        var deleted = await courseService.DeleteAsync(id, ct);
        if (!deleted)
        {
            return NotFound(new { message = $"Course with ID {id} not found." });
        }

        return NoContent(); // HTTP 204 is the standard success response for a DELETE
    }

}