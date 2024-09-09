using DinarkTaskOne.Models.UserSpecficModels;
using DinarkTaskOne.Models.Authentication_Authorization;
using DinarkTaskOne.Models.ManageCourse;
using DinarkTaskOne.Models.MakeQuiz;
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

        // DbSets for the Quiz System
        public DbSet<QuizModel> Quizzes { get; set; } = null!;
        public DbSet<QuestionModel> Questions { get; set; } = null!;
        public DbSet<AnswerModel> Answers { get; set; } = null!;
        public DbSet<AttemptModel> Attempts { get; set; } = null!;
        public DbSet<QuestionAnswerModel> QuestionAnswers { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configuring the RolesModel
            modelBuilder.Entity<RolesModel>()
                .Property(r => r.RoleId)
                .ValueGeneratedNever();

            modelBuilder.Entity<RolesModel>().HasData(
                new RolesModel { RoleId = 1, RoleName = "Instructor" },
                new RolesModel { RoleId = 2, RoleName = "Student" },
                new RolesModel { RoleId = 3, RoleName = "Admin" }
            );

            // Configuring UsersModel with Discriminator for different user types
            modelBuilder.Entity<UsersModel>()
                .ToTable("Users")
                .HasDiscriminator<string>("UserType")
                .HasValue<UsersModel>("User")
                .HasValue<StudentModel>("Student")
                .HasValue<InstructorModel>("Instructor")
                .HasValue<AdminModel>("Admin");

            // Configuring Course relationships
            modelBuilder.Entity<CourseModel>()
                .HasOne(c => c.Instructor)
                .WithMany(i => i.Courses)
                .HasForeignKey(c => c.InstructorId)
                .OnDelete(DeleteBehavior.Cascade);

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

            // Configuring Quiz relationships
            modelBuilder.Entity<QuizModel>()
                .HasMany(q => q.Questions)
                .WithOne(q => q.Quiz)
                .HasForeignKey(q => q.QuizId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configuring Question relationships
            modelBuilder.Entity<QuestionModel>()
                .HasMany(q => q.Answers)
                .WithOne(a => a.Question)
                .HasForeignKey(a => a.QuestionId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<QuestionModel>()
                .HasMany(q => q.QuestionAnswers)
                .WithOne(qa => qa.Question)
                .HasForeignKey(qa => qa.QuestionId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configuring Attempt relationships
            modelBuilder.Entity<AttemptModel>()
                .HasOne(a => a.Quiz)
                .WithMany(q => q.Attempts)
                .HasForeignKey(a => a.QuizId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<AttemptModel>()
                .HasOne(a => a.Student)
                .WithMany(s => s.Attempts)
                .HasForeignKey(a => a.StudentId)
                .OnDelete(DeleteBehavior.Restrict);

            // Configuring QuestionAnswer relationships
            modelBuilder.Entity<QuestionAnswerModel>()
                .HasOne(qa => qa.Attempt)
                .WithMany(a => a.QuestionAnswers)
                .HasForeignKey(qa => qa.AttemptId)
                .OnDelete(DeleteBehavior.Cascade);  

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
        }
    }
}
