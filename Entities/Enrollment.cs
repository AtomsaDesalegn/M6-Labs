using System;
namespace TmsApi.Entities;

public class Enrollment
{
    public int Id { get; set; }
    public int StudentId { get; set; }
    public int CourseId { get; set; }
    public decimal? Grade { get; set; } // Nullable, as student may be currently enrolled
    public DateTime EnrolledAt { get; set; } = DateTime.UtcNow;
    // Navigation properties back to entities
    // Navigation properties back to entities
    public Student? Student { get; set; }
    public Course? Course { get; set; }
    public bool IsArchived { get; set; } = false;
}