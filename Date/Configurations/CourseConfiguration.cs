using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TmsApi.Entities;
using TmsApi.Models;

namespace TmsApi.Data.Configurations;

public class CourseConfiguration : IEntityTypeConfiguration<Course>
    {
        public void Configure(EntityTypeBuilder<Course> builder)
        {
            // builder.ToTable("Courses");

            // Primary Key
            builder.HasKey(c => c.Id);

            // Constraints
            builder.Property(c => c.Title)
                   .IsRequired()
                   .HasMaxLength(150);
        }
    }