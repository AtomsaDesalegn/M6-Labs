using Microsoft.AspNetCore.Mvc;
using TmsApi.Models;
[ApiController]
[Route("api/enrollments")]

public class EnrollmentsController(IEnrollmentService enrollmentService) : ControllerBase
{
    //Get /api/enrollments returns all enrollments records
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var enrollments = await enrollmentService.GetAllAsync();
        return Ok(enrollments);
    }

    //Get /api/enrollments/{id} returns one or 404
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(string id)
    {
        var record = await enrollmentService.GetByIdAsync(id);
        return record is not null ? Ok(record) : NotFound();
    }

    // POST /api/enrollments creates a new enrollment and returns 201 with Location header
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateEnrollmentRequest request)
    {
        var record = await enrollmentService.EnrollAsync(request.StudentId, request.CourseCode);
        return CreatedAtAction(nameof(GetById), new { id = record.Id }, record);
    }

    // Delete/api/entrollments/{id} returns 204 or 404
    [HttpDelete("{id}")]

    public async Task<IActionResult> Delete(string id)
    {
        var deleted = await enrollmentService.DeleteAsync(id);
        return deleted ? NoContent() : NotFound();
    }

    [HttpPost("bulk-archive")]
    public async Task<IActionResult> BulkArchive([FromQuery] DateTime cutoffDate)
    {
        int updatedRows = await enrollmentService.ArchiveOldEnrollmentsAsync(cutoffDate);
        return Ok(new { Message = $"{updatedRows} enrollments successfully archived." });
    }
}