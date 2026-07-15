using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TmsApi.DTOs;
using TmsApi.services;

namespace TmsApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class StudentController : ControllerBase
{
    private readonly IStudentService _studentService;
    
    public StudentController(IStudentService studentService)
    {
        _studentService = studentService;
    }

    /// <summary>
    /// Get a student by ID
    /// </summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [EndpointSummary("Get a student by ID")]
    [EndpointDescription("Returns a single student detail using their unique identifier.")]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
    {
        var student = await _studentService.GetByIdAsync(id, ct);
        if(student == null)
        {
            return NotFound();
        }
        return Ok(student);
    }

    /// <summary>
    /// List students with pagination
    /// </summary>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [EndpointSummary("List students with pagination")]
    [EndpointDescription("Returns a paginated list of TMS students. PageSize is capped at 50.")]
    public async Task<IActionResult> GetAllStudents([FromQuery] int page = 1, [FromQuery] int pageSize = 10, CancellationToken ct = default)
    {
        if (page < 1) page = 1;
        if (pageSize < 1) pageSize = 10;
        if (pageSize > 50) pageSize = 50;
        
        var (items, totalCount) = await _studentService.GetPagedStudentsAsync(page, pageSize, ct);
        var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

        return Ok(new
        {
            page,
            pageSize,
            totalCount,
            totalPages,
            items
        });
    }

    /// <summary>
    /// Create a new student
    /// </summary>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [EndpointSummary("Create a new student")]
    [EndpointDescription("Creates a new student record. The registration number must be unique.")]
    public async Task<IActionResult> Create(CreateStudentRequestDto request, CancellationToken ct)
    {
        if(await _studentService.RegistrationNumberExistsAsync(request.RegistrationNumber, ct))
        {
            return Conflict(new ProblemDetails
            {
                Title = "Registration number already exists",
                Detail = $"A student with registration number '{request.RegistrationNumber}' is already registered.",
                Status = StatusCodes.Status409Conflict
            });
        }
        var newStudent = await _studentService.CreateAsync(request, ct);

        return CreatedAtAction(nameof(GetById), new {id = newStudent.Id}, newStudent);
    }

    /// <summary>
    /// Delete a student
    /// </summary>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [EndpointSummary("Delete a student")]
    [EndpointDescription("Permanently removes a student record from the system.")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var deleted = await _studentService.DeleteAsync(id, ct);
        if (!deleted)
        {
            return NotFound();
        }
        return NoContent();
    }
}