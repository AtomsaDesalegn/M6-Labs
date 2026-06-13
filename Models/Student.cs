namespace TmsApi.Models;

public class Student
{
    public required string Id { get; init; }
    public required string Name { get; set; }
    public int Age { get; set; }
    public decimal GPA { get; set; }
}