using DinarkTaskOne.Models.Authentication_Authorization;
using DinarkTaskOne.Models.Institution;
using DinarkTaskOne.Models.MakeQuiz;
using DinarkTaskOne.Models.ManageCourse;
using DinarkTaskOne.Models.UserSpecficModels;
using DinarkTaskOne.Models.student; // Added namespace for student models
using Microsoft.EntityFrameworkCore;

namespace DinarkTaskOne.Data
{
    public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
    {
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

        // DbSets for Course Levels
        public DbSet<LevelModel> Levels { get; set; } = null!;

        // DbSets for Student Progress and Grades
        public DbSet<StudentProgressModel> StudentProgresses { get; set; } = null!;
        public DbSet<StudentGradeModel> StudentGrades { get; set; } = null!;

        // DbSet for Course Grades
        public DbSet<CourseGradeModel> CourseGrades { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure Primary Keys and Value Generation
            modelBuilder.Entity<UsersModel>()
                .Property(u => u.UserId)
                .ValueGeneratedOnAdd(); // Use Identity generation for UserId

            // Configure Roles
            modelBuilder.Entity<RolesModel>()
                .Property(r => r.RoleId)
                .ValueGeneratedNever();

            modelBuilder.Entity<RolesModel>().HasData(
                new RolesModel { RoleId = 1, RoleName = "Instructor" },
                new RolesModel { RoleId = 2, RoleName = "Student" },
                new RolesModel { RoleId = 3, RoleName = "Admin" }
            );

            // Configure Users with Discriminator
            modelBuilder.Entity<UsersModel>()
                .ToTable("Users")
                .HasDiscriminator<string>("UserType")
                .HasValue<UsersModel>("User")
                .HasValue<StudentModel>("Student")
                .HasValue<InstructorModel>("Instructor")
                .HasValue<AdminModel>("Admin");

            // Course Relationships
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
                .HasOne(c => c.Level)
                .WithMany(l => l.Courses)
                .HasForeignKey(c => c.LevelId)
                .OnDelete(DeleteBehavior.Restrict);

            // Student and Major Relationship
            modelBuilder.Entity<StudentModel>()
                .HasOne(s => s.Major)
                .WithMany(m => m.Students)
                .HasForeignKey(s => s.MajorId)
                .OnDelete(DeleteBehavior.Restrict);

            // Attempt Relationships
            modelBuilder.Entity<AttemptModel>()
                .HasOne(a => a.Student)
                .WithMany(s => s.Attempts)
                .HasForeignKey(a => a.StudentId)
                .OnDelete(DeleteBehavior.Cascade);

            // QuestionAnswer Relationships
            modelBuilder.Entity<QuestionAnswerModel>()
                .HasOne(qa => qa.Question)
                .WithMany(q => q.QuestionAnswers)
                .HasForeignKey(qa => qa.QuestionId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<QuestionAnswerModel>()
                .HasOne(qa => qa.Attempt)
                .WithMany(a => a.QuestionAnswers)
                .HasForeignKey(qa => qa.AttemptId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<QuestionAnswerModel>()
                .HasOne(qa => qa.SelectedOption)
                .WithMany()
                .HasForeignKey(qa => qa.SelectedOptionId)
                .OnDelete(DeleteBehavior.Restrict);

            // Enrollment Relationships
            modelBuilder.Entity<EnrollModel>()
                .HasOne(e => e.Course)
                .WithMany(c => c.Enrollments)
                .HasForeignKey(e => e.CourseId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<EnrollModel>()
                .HasOne(e => e.Student)
                .WithMany(s => s.Enrollments)
                .HasForeignKey(e => e.StudentId)
                .OnDelete(DeleteBehavior.Cascade);

            // Level Relationships
            modelBuilder.Entity<LevelModel>()
                .HasMany(l => l.Students)
                .WithOne(s => s.CurrentLevel)
                .HasForeignKey(s => s.CurrentLevelId)
                .OnDelete(DeleteBehavior.Restrict);

            // Student Progress Relationships
            modelBuilder.Entity<StudentProgressModel>()
                .HasOne(sp => sp.Student)
                .WithMany(s => s.Progresses)
                .HasForeignKey(sp => sp.StudentId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<StudentProgressModel>()
                .HasOne(sp => sp.Level)
                .WithMany(l => l.Progresses)
                .HasForeignKey(sp => sp.LevelId)
                .OnDelete(DeleteBehavior.Restrict);

            // Student Grade Relationships
            modelBuilder.Entity<StudentGradeModel>()
                .HasOne(sg => sg.Student)
                .WithMany(s => s.Grades)
                .HasForeignKey(sg => sg.StudentId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<StudentGradeModel>()
                .HasOne(sg => sg.Level)
                .WithMany(l => l.Grades)
                .HasForeignKey(sg => sg.LevelId)
                .OnDelete(DeleteBehavior.Restrict);

            // Course Grade Relationships
            modelBuilder.Entity<CourseGradeModel>()
                .HasOne(cg => cg.Course)
                .WithMany()
                .HasForeignKey(cg => cg.CourseId)
                .OnDelete(DeleteBehavior.SetNull); // Set CourseId to null instead of deleting related CourseGrades

            modelBuilder.Entity<CourseGradeModel>()
                .HasOne(cg => cg.Student)
                .WithMany()
                .HasForeignKey(cg => cg.StudentId)
                .OnDelete(DeleteBehavior.Cascade); // Keep as Cascade to delete grades related to deleted student

            // Seed Departments and Majors with Negative IDs
            modelBuilder.Entity<DepartmentModel>().HasData(
                new DepartmentModel { DepartmentId = -1, Name = "Not in a Department" },
                new DepartmentModel { DepartmentId = 1, Name = "Computer Science" },
                new DepartmentModel { DepartmentId = 2, Name = "Information Systems" },
                new DepartmentModel { DepartmentId = 3, Name = "Cyber Security" }
            );

            modelBuilder.Entity<MajorModel>().HasData(
                new MajorModel { MajorId = -1, Name = "Not in a major", DepartmentId = -1 },
                new MajorModel { MajorId = 1, Name = "Computer Science", DepartmentId = 1 },
                new MajorModel { MajorId = 2, Name = "Information Systems", DepartmentId = 2 },
                new MajorModel { MajorId = 3, Name = "Cyber Security", DepartmentId = 3 }
            );

            // Seed Levels (Simplified)
            modelBuilder.Entity<LevelModel>().HasData(
                new LevelModel { LevelId = -1, Type = LevelType.LevelZero }, // Use negative values for seeding
                new LevelModel { LevelId = 1, Type = LevelType.LevelOne },
                new LevelModel { LevelId = 2, Type = LevelType.LevelTwo },
                new LevelModel { LevelId = 3, Type = LevelType.LevelThree },
                new LevelModel { LevelId = 4, Type = LevelType.LevelFour }
            );
        }
    }
}
