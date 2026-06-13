public class Course
{
    public required string Id{get; set;}
    public required string Title{get; set;}
    public int Capacity{get; set;}
    public DateOnly startDate {get; set;}
}