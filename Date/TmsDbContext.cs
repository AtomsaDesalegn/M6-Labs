using Microsoft.EntityFrameworkCore;
using TmsApi.Entities;
namespace TmsApi.Data;

public class TmsDbContext(DbContextOptions<TmsDbContext> options) : DbContext(options)
{
    public DbSet<Student> Students => Set<Student>();
    public DbSet<Course> Courses => Set<Course>();
    public DbSet<Enrollment> Enrollments => Set<Enrollment>();
    public DbSet<Assessment> Assessments => Set<Assessment>();
    public DbSet<Certificate> Certificates => Set<Certificate>();


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        modelBuilder.Entity<TmsApi.Entities.Student>()
        .Property<DateTime>("LastUpdated");

        modelBuilder.Entity<Course>(builder =>
        {
            // Set Primary Key
            builder.HasKey(c => c.Id);
            
            // Set Max Lengths and Requirements
            builder.Property(c => c.Code)
                .IsRequired()
                .HasMaxLength(10);
                
            builder.Property(c => c.Title)
                .IsRequired()
                .HasMaxLength(200);
                
            // CRITICAL: This index causes PostgreSQL to reject duplicate course codes
            builder.HasIndex(c => c.Code)
                .IsUnique();

            // Set up relationship to Enrollments
            builder.HasMany(c => c.Enrollments)
                .WithOne(e => e.Course)
                .HasForeignKey(e => e.CourseId);
        });
    }
}