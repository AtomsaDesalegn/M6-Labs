namespace TmsApi.Entities;

public class Student
{
    public int Id { get; set; }
    // sur rogate primary key — internal, used by foreign keys
    public required string RegistrationNumber { get; set; } // natural key — human-readable(uniqueness configured in Session 2)
    public required string Name { get; set; }
    public decimal GPA { get; set; }
    public bool IsActive { get; set; } = true;
    public int Age{get; set;}
    public uint Version { get; set; }
    // Navigation property for many-to-many relationship
    public ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();
}