using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TmsApi.Models;

namespace TmsApi.Data.Configurations;

public class StudentConfiguration : IEntityTypeConfiguration<Student>
{
    public void Configure(EntityTypeBuilder<Student> builder)
    {
        // builder.ToTable("Students");

        //Primary key
        builder.HasKey(s => s.Id);

        //Constraints

        builder.Property(s => s.Name)
        .IsRequired()
        .HasMaxLength(100);

        builder.Property(s => s.GPA)
        .HasPrecision(3, 2);
    }
}