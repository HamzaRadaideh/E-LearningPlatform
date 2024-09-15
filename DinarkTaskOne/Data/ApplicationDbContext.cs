using DinarkTaskOne.Models.Authentication_Authorization;
using DinarkTaskOne.Models.Institution;
using DinarkTaskOne.Models.MakeQuiz;
using DinarkTaskOne.Models.ManageCourse;
using DinarkTaskOne.Models.UserSpecficModels;
using Microsoft.EntityFrameworkCore;

namespace DinarkTaskOne.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        // DbSets for Authentication and Authorization
        public DbSet<UsersModel> Users { get; set; } = null!;
        public DbSet<RolesModel> Roles { get; set; } = null!;
        public DbSet<SessionCache> SessionCaches { get; set; } = null!;

        // DbSets for Course Management
        public DbSet<CourseModel> Courses { get; set; } = null!;
        public DbSet<EnrollModel> Enrollments { get; set; } = null!;
        public DbSet<MaterialsModel> Materials { get; set; } = null!;
        public DbSet<AnnouncementModel> Announcements { get; set; } = null!;

        // DbSets for Quiz System
        public DbSet<QuizModel> Quizzes { get; set; } = null!;
        public DbSet<QuestionModel> Questions { get; set; } = null!;
        public DbSet<AnswerModel> Answers { get; set; } = null!;
        public DbSet<AttemptModel> Attempts { get; set; } = null!;
        public DbSet<QuestionAnswerModel> QuestionAnswers { get; set; } = null!;

        // DbSets for User-Specific Models
        public DbSet<StudentModel> Students { get; set; } = null!;
        public DbSet<InstructorModel> Instructors { get; set; } = null!;
        public DbSet<AdminModel> Admins { get; set; } = null!;

        // DbSets for Departments and Majors
        public DbSet<DepartmentModel> Departments { get; set; } = null!;
        public DbSet<MajorModel> Majors { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Roles
            modelBuilder.Entity<RolesModel>()
                .Property(r => r.RoleId)
                .ValueGeneratedNever();

            modelBuilder.Entity<RolesModel>().HasData(
                new RolesModel { RoleId = 1, RoleName = "Instructor" },
                new RolesModel { RoleId = 2, RoleName = "Student" },
                new RolesModel { RoleId = 3, RoleName = "Admin" }
            );

            // UsersModel configuration with Discriminator
            modelBuilder.Entity<UsersModel>()
                .ToTable("Users")
                .HasDiscriminator<string>("UserType")
                .HasValue<UsersModel>("User")
                .HasValue<StudentModel>("Student")
                .HasValue<InstructorModel>("Instructor")
                .HasValue<AdminModel>("Admin");

            // Course relationships
            modelBuilder.Entity<CourseModel>()
                .HasOne(c => c.Instructor)
                .WithMany(i => i.Courses)
                .HasForeignKey(c => c.InstructorId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<CourseModel>()
                .HasOne(c => c.Department)
                .WithMany(d => d.Courses)
                .HasForeignKey(c => c.DepartmentId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<CourseModel>()
                .HasMany(c => c.AllowedMajors)
                .WithMany(m => m.Courses)
                .UsingEntity(j => j.ToTable("CourseMajors"));

            // Major and Department relationships
            modelBuilder.Entity<MajorModel>()
                .HasOne(m => m.Department)
                .WithMany(d => d.Majors)
                .HasForeignKey(m => m.DepartmentId)
                .OnDelete(DeleteBehavior.Restrict);

            // Instructor and Department relationship
            modelBuilder.Entity<InstructorModel>()
                .HasOne(i => i.Department)
                .WithMany(d => d.Instructors)
                .HasForeignKey(i => i.DepartmentId)
                .OnDelete(DeleteBehavior.Restrict);

            // Student and Major relationship
            modelBuilder.Entity<StudentModel>()
                .HasOne(s => s.Major)
                .WithMany(m => m.Students)
                .HasForeignKey(s => s.MajorId)
                .OnDelete(DeleteBehavior.Restrict);

            // Attempt relationships
            modelBuilder.Entity<AttemptModel>()
                .HasOne(a => a.Student)
                .WithMany(s => s.Attempts)
                .HasForeignKey(a => a.StudentId)
                .OnDelete(DeleteBehavior.Restrict);

            // QuestionAnswer relationships
            modelBuilder.Entity<QuestionAnswerModel>()
                .HasOne(qa => qa.Attempt)
                .WithMany(a => a.QuestionAnswers)
                .HasForeignKey(qa => qa.AttemptId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<QuestionAnswerModel>()
                .HasOne(qa => qa.Question)
                .WithMany(q => q.QuestionAnswers)
                .HasForeignKey(qa => qa.QuestionId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<QuestionAnswerModel>()
                .HasOne(qa => qa.SelectedOption)
                .WithMany()
                .HasForeignKey(qa => qa.SelectedOptionId)
                .OnDelete(DeleteBehavior.Restrict);


            // EnrollModel relationships
            modelBuilder.Entity<EnrollModel>()
                .HasOne(e => e.Course)
                .WithMany(c => c.Enrollments)
                .HasForeignKey(e => e.CourseId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<EnrollModel>()
                .HasOne(e => e.Student)
                .WithMany(s => s.Enrollments)
                .HasForeignKey(e => e.StudentId)
                .OnDelete(DeleteBehavior.Restrict);

            // Seed default departments and majors
            modelBuilder.Entity<DepartmentModel>().HasData(
                new DepartmentModel { DepartmentId = 1, Name = "Computer Science" },
                new DepartmentModel { DepartmentId = 2, Name = "Information Systems" },
                new DepartmentModel { DepartmentId = 3, Name = "Cyber Security" }
            );

            modelBuilder.Entity<MajorModel>().HasData(
                new MajorModel { MajorId = 1, Name = "Computer Science", DepartmentId = 1 },
                new MajorModel { MajorId = 2, Name = "Information Systems", DepartmentId = 2 },
                new MajorModel { MajorId = 3, Name = "Cyber Security", DepartmentId = 3 }
            );
        }
    }
}