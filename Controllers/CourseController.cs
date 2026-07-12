using Microsoft.AspNetCore.Mvc;
using TmsApi.Services;
using TmsApi.Entities; 
using Tms.Api.Dtos;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Mvc.Infrastructure;

namespace TmsApi.Controllers;

[ApiController]
[Route("api/courses")]
public class CourseController(ICourseService courseService) : ControllerBase
{
    [HttpGet("{id:int}", Name = nameof(GetCourseById))]
    public async Task<IActionResult> GetCourseById(int id, CancellationToken ct)
    {
        var course = await courseService.GetByIdAsync(id, ct);
        return course is not null ? Ok(course) : NotFound();
    }

    [HttpPost]

    public async Task<IActionResult> CreateCourse(CreateCourseRequest request, CancellationToken ct)
    {
        if(await courseService.CodeExistsAsync(request.Code, ct))
        {
            return Conflict(new ProblemDetails{
                Title = "Course code alreay exists",
                Detail = $"A course with code '{request.Code}' is already registered",
                Status = StatusCodes.Status409Conflict
            });
        }
        var result = await courseService.CreateAsync(request, ct);
        return CreatedAtAction(nameof(GetCourseById), new {id = result.Id}, result);

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