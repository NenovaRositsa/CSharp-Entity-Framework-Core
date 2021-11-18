using Microsoft.EntityFrameworkCore;
using P01_StudentSystem.Data.Models;

namespace P01_StudentSystem.Data
{
    public class StudentSystemContext : DbContext
    {
        public StudentSystemContext()
        {

        }

        public StudentSystemContext(DbContextOptions options)
            : base(options)
        {

        }

        public DbSet<Course> Courses { get; set; }
        public DbSet<Homework> HomeworkSubmissions { get; set; }
        public DbSet<Resource> Resources { get; set; }
        public DbSet<Student> Students { get; set; }
        public DbSet<StudentCourse> StudentCourses { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlServer(@"Server=NENOVI-PC\SQLEXPRESS;Database=StudentSystem;Integrated Security=True;");
            }

            base.OnConfiguring(optionsBuilder);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Student>(entity =>

           {
               entity.HasKey(x => x.StudentId);

               entity.Property(x => x.Name)
               .IsRequired()
               .HasMaxLength(100)
               .IsUnicode();

               entity.Property(x => x.PhoneNumber)
               .IsRequired(false)
               .IsUnicode(false)
               .HasDefaultValueSql("CHAR(10)");

               entity.Property(x => x.RegisteredOn)
               .IsRequired();

               entity.Property(x => x.Birthday)
               .IsRequired(false);
           });

            modelBuilder.Entity<Course>(entity =>
            {
                entity.HasKey(x => x.CourseId);

                entity.Property(x => x.Name)
                .IsRequired()
                .IsUnicode()
                .HasMaxLength(80);

                entity.Property(x => x.Description)
                .IsRequired(false)
                .IsUnicode();

                entity.Property(c => c.StartDate)
                .IsRequired();

                entity.Property(c => c.EndDate)
                    .IsRequired();

                entity.Property(c => c.Price)
                    .IsRequired();

            });

            modelBuilder.Entity<Resource>(entity =>
            {
                entity.HasKey(x => x.ResourceId);

                entity.Property(x => x.Name)
                .IsRequired()
                .IsUnicode()
                .HasMaxLength(50);

                entity.Property(x => x.Url)
                .IsRequired()
                .IsUnicode(false);

                entity.Property(x => x.ResourceType)
                .IsRequired();

                entity
                .HasOne(c => c.Course)
                .WithMany(r => r.Resources)
                .HasForeignKey(c => c.CourseId);

            });
            modelBuilder.Entity<Homework>(entity =>
            {
                entity.HasKey(x => x.HomeworkId);

                entity.Property(x => x.Content)
                .IsRequired()
                .IsUnicode(false);

                entity.Property(h => h.ContentType)
                .IsRequired();

               entity.Property(h => h.SubmissionTime)
                    .IsRequired();

                entity
                .HasOne(s => s.Student)
                .WithMany(h => h.HomeworkSubmissions)
                .HasForeignKey(s => s.StudentId);

                entity
                .HasOne(c => c.Course)
                .WithMany(h => h.HomeworkSubmissions)
                .HasForeignKey(c => c.CourseId);
            });

            modelBuilder.Entity<StudentCourse>(entity =>
            {
                entity.HasKey(x => new { x.CourseId, x.StudentId });

                entity.HasOne(sc => sc.Student)
                .WithMany(s => s.CourseEnrollments)
                .HasForeignKey(sc => sc.StudentId);

                entity.HasOne(sc => sc.Course)
                    .WithMany(c => c.StudentsEnrolled)
                    .HasForeignKey(sc => sc.CourseId);

            });
               
        }
    }
}
