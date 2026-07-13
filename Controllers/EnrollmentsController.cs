using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Tms.Api.Dtos;
using Tms.Api.Services;
using TmsApi.Services;

namespace Tms.Api.Controllers;

[ApiController]
[Route("api/courses/{courseId:int}/enrollments")]
public class EnrollmentsController(
    ICourseService courseService,
    IEnrollmentService enrollmentService) : ControllerBase
{
    [HttpGet("{id:int}", Name = nameof(GetEnrollment))]
    public async Task<IActionResult> GetEnrollment(int courseId, int id, CancellationToken ct)
    {
        var enrollment = await enrollmentService.GetByIdAsync(courseId, id, ct);
        return enrollment is not null ? Ok(enrollment) : NotFound();
    }

    [HttpPost]
    public async Task<IActionResult> EnrollStudent(int courseId, EnrollStudentRequest request, CancellationToken ct)
    {
        // 1. Look up the parent course. If null, return NotFound().
        var course = await courseService.GetByIdAsync(courseId, ct);
        if (course is null)
        {
            return NotFound();
        }

        // 2. Check capacity. If full, return Conflict with ProblemDetails.
        if (course.EnrollmentCount >= course.MaxCapacity)
        {
            return Conflict(new ProblemDetails
            {
                Title = "Course is full",
                Detail = $"Course '{course.Title}' has reached its maximum capacity of {course.MaxCapacity}.",
                Status = StatusCodes.Status409Conflict
            });
        }

        // 3. Otherwise, call enrollmentService.CreateAsync and return CreatedAtAction.
        var enrollment = await enrollmentService.CreateAsync(courseId, request, ct);

        return CreatedAtAction(
            nameof(GetEnrollment),
            new { courseId, id = enrollment.Id },
            enrollment);
    }

    [HttpGet]
    public async Task<IActionResult> GetEnrollments(int courseId, CancellationToken ct)
    {
        var enrollments = await enrollmentService.GetByCourseIdAsync(courseId, ct);
        return Ok(enrollments);
    }
}