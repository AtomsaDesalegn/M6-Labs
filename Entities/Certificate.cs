using System;

namespace TmsApi.Entities;

public class Certificate
{
    // Primary key (surrogate)
    public int Id { get; set; }

    // Natural key — human-readable (uniqueness configured in Session 2)
    public required string SerialNumber { get; set; } 
    
    public DateTime IssuedAt { get; set; } = DateTime.UtcNow;

    // Foreign keys + navigation to the student and course
    public int StudentId { get; set; }
    public int CourseId { get; set; }
    
    public Student Student { get; set; } = null!;
    public Course Course { get; set; } = null!;
}