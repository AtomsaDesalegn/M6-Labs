using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TmsApi.Entities;
using TmsApi.Models;

namespace TmsApi.Data.Configurations;

public class EnrollmentConfiguration : IEntityTypeConfiguration<Enrollment>
    {
        public void Configure(EntityTypeBuilder<Enrollment> builder)
        {
            // builder.ToTable("Enrollments");

            // 1. Composite Primary Key (Prevents a student from enrolling in the same course twice)
            builder.HasKey(e => new { e.StudentId, e.CourseId });

            // 2. Relationship: One Student has Many Enrollments
            builder.HasOne(e => e.Student)
                   .WithMany(s => s.Enrollments)
                   .HasForeignKey(e => e.StudentId)
                   .OnDelete(DeleteBehavior.Cascade); // If a student is deleted, drop their enrollments

            // 3. Relationship: One Course has Many Enrollments
            builder.HasOne(e => e.Course)
                   .WithMany(c => c.Enrollments)
                   .HasForeignKey(e => e.CourseId)
                   .OnDelete(DeleteBehavior.Cascade); // If a course is deleted, drop its enrollments
        }
    }