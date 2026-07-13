using Microsoft.AspNetCore.Mvc;
using TmsApi.Services;
using TmsApi.Entities;
using Tms.Api.Dtos;

namespace TmsApi.Controllers;

[ApiController]
[Route("api/courses")]
public class CourseController(ICourseService courseService, LinkGenerator linkGenerator) : ControllerBase
{
    [HttpGet("{id:int}", Name = nameof(GetCourseById))]
    public async Task<IActionResult> GetCourseById(int id, CancellationToken ct)
    {
        var course = await courseService.GetByIdAsync(id, ct);
        if (course is null)
        {
            return NotFound();
        }

        // 1. Generate URLs dynamically using the primary constructor's linkGenerator
        var selfHref = linkGenerator.GetPathByName(HttpContext, nameof(GetCourseById), new { id });
        var enrollmentsHref = linkGenerator.GetPathByAction(HttpContext,
            action: "GetEnrollments",
            controller: "Enrollments",
            values: new { courseId = id });

        // 2. Build the core HATEOAS Links list using positional record constructor syntax
        var links = new List<LinkDto>
        {
            new(selfHref!, "self", "GET"),
            new(selfHref!, "update", "PUT"),
            new(selfHref!, "delete", "DELETE"),
            new(enrollmentsHref!, "enrollments", "GET")
        };

        // Conditional HATEOAS Link: Only provide the enroll action if there is available space
        if (course.EnrollmentCount < course.MaxCapacity)
        {
            links.Add(new LinkDto(enrollmentsHref!, "enroll", "POST"));
        }

        // 3. Map to detail DTO and return
        var detailDto = new CourseDetailDto
        {
            Id = course.Id,
            Code = course.Code,
            Title = course.Title,
            MaxCapacity = course.MaxCapacity,
            EnrollmentCount = course.EnrollmentCount,
            Links = links
        };

        return Ok(detailDto);
    }

    [HttpPost]
    public async Task<IActionResult> CreateCourse(CreateCourseRequest request, CancellationToken ct)
    {
        if (await courseService.CodeExistsAsync(request.Code, ct))
        {
            return Conflict(new ProblemDetails
            {
                Title = "Course code already exists",
                Detail = $"A course with code '{request.Code}' is already registered",
                Status = StatusCodes.Status409Conflict
            });
        }
        
        var result = await courseService.CreateAsync(request, ct);
        return CreatedAtAction(nameof(GetCourseById), new { id = result.Id }, result);
    }

    [HttpGet]
    public async Task<IActionResult> GetAllCourses(CancellationToken ct)
    {
        var courses = await courseService.GetAllAsync(ct); 
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

        return NoContent();
    }
}