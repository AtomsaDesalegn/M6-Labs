using Microsoft.AspNetCore.Mvc;
[ApiController]
[Route("api/enrollments")]

public class EnrollmentsController(IEnrollmentService enrollmentService): ControllerBase
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
}