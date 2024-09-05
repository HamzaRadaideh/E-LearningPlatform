using DinarkTaskOne.Models.UserSpecficModels;
using DinarkTaskOne.Models.Authentication_Authorization;
using DinarkTaskOne.Models.ManageCourse;
using Microsoft.EntityFrameworkCore;

namespace DinarkTaskOne.Data
{
    public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
    {

        // DbSets for Authentication and Authorization
        public DbSet<UsersModel> Users { get; set; }
        public DbSet<RolesModel> Roles { get; set; }
        public DbSet<SessionCache> SessionCaches { get; set; }

        // DbSets for Course Management
        public DbSet<CourseModel> Courses { get; set; }
        public DbSet<EnrollModel> Enrollments { get; set; }
        public DbSet<MaterialsModel> Materials { get; set; }
        public DbSet<AnnouncementModel> Announcements { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<RolesModel>()
                .Property(r => r.RoleId)
                .ValueGeneratedNever();

            modelBuilder.Entity<RolesModel>().HasData(
                new RolesModel { RoleId = 1, RoleName = "Instructor" },
                new RolesModel { RoleId = 2, RoleName = "Student" },
                new RolesModel { RoleId = 3, RoleName = "Admin" }
            );

            modelBuilder.Entity<UsersModel>()
                .ToTable("Users")
                .HasDiscriminator<string>("UserType")
                .HasValue<UsersModel>("User")
                .HasValue<StudentModel>("Student")
                .HasValue<InstructorModel>("Instructor")
                .HasValue<AdminModel>("Admin");

            modelBuilder.Entity<CourseModel>()
                .HasOne(c => c.Instructor)
                .WithMany(i => i.Courses)
                .HasForeignKey(c => c.InstructorId)
                .OnDelete(DeleteBehavior.Cascade);

            //modelBuilder.Entity<CourseModel>()
            //    .HasMany(c => c.Quizzes)
            //    .WithOne(q => q.Course)
            //    .HasForeignKey(q => q.CourseId)
            //    .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<CourseModel>()
                .HasMany(c => c.Materials)
                .WithOne(m => m.Course)
                .HasForeignKey(m => m.CourseId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<CourseModel>()
                .HasMany(c => c.Enrollments)
                .WithOne(e => e.Course)
                .HasForeignKey(e => e.CourseId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<EnrollModel>()
                .HasOne(e => e.Student)
                .WithMany(s => s.Enrollments)
                .HasForeignKey(e => e.StudentId)
                .OnDelete(DeleteBehavior.Restrict);

            //modelBuilder.Entity<QuizModel>()
            //    .HasMany(q => q.Questions)
            //    .WithOne(q => q.Quiz)
            //    .HasForeignKey(q => q.QuizId)
            //    .OnDelete(DeleteBehavior.Cascade);

            //modelBuilder.Entity<QuestionModel>()
            //    .HasMany(q => q.Options)
            //    .WithOne(o => o.Question)
            //    .HasForeignKey(o => o.QuestionId)
            //    .OnDelete(DeleteBehavior.Cascade);

            //modelBuilder.Entity<CorrectOptionModel>()
            //    .HasOne(c => c.Question)
            //    .WithOne(q => q.CorrectOption)
            //    .HasForeignKey<CorrectOptionModel>(c => c.QuestionId)
            //    .OnDelete(DeleteBehavior.Cascade);

            //modelBuilder.Entity<CorrectOptionModel>()
            //    .HasIndex(c => c.QuestionId)
            //    .IsUnique();

            //modelBuilder.Entity<CourseGradeModel>()
            //    .HasOne(cg => cg.Course)
            //    .WithMany(c => c.CourseGrades)
            //    .HasForeignKey(cg => cg.CourseId)
            //    .OnDelete(DeleteBehavior.Cascade);

            //modelBuilder.Entity<CourseGradeModel>()
            //    .HasOne(cg => cg.Student)
            //    .WithMany(s => s.CourseGrades)
            //    .HasForeignKey(cg => cg.StudentId)
            //    .OnDelete(DeleteBehavior.Restrict);

            //modelBuilder.Entity<QuizAttemptModel>()
            //    .HasOne(qa => qa.Quiz)
            //    .WithMany(q => q.QuizAttempts)
            //    .HasForeignKey(qa => qa.QuizId)
            //    .OnDelete(DeleteBehavior.Cascade);

            //modelBuilder.Entity<QuizAttemptModel>()
            //    .HasOne(qa => qa.Student)
            //    .WithMany(s => s.QuizAttempts)
            //    .HasForeignKey(qa => qa.StudentId)
            //    .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
