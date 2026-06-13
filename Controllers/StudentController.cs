using Microsoft.AspNetCore.Mvc;
using TmsApi.services;
[ApiController]
[Route("api/students")]

public class StudentController(IStudentService studentService) : ControllerBase
{
    //Get/api/students returns all enrollment records
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var students = await studentService.GetAllAsync();
        return Ok(students);
    }
    //Get/api/students/{id} returns one or 404
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(string id)
    {
        var record = await studentService.GetByIdAsync(id);
        return record is not null ? Ok(record) : NotFound();
    }


    //POST/api/students create and returns 201 with Location header
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateStudentRequest request)
    {
        var record = await studentService.CreateAsync(request.Name, request.Age, request.GPA);
        return CreatedAtAction(nameof(GetById), new { id = record.Id }, record);
    }
    //DELETE /api/enrollment/{id} returns 204 or 404
    [HttpDelete("{id}")]

    public async Task<IActionResult> Delete(string id)
    {
        var deleted = await studentService.DeleteAsync(id);
        return deleted ? NoContent() : NotFound();
    }
}
// Place this at the very bottom of StudentController.cs
public record CreateStudentRequest(string Name, int Age, decimal GPA);
