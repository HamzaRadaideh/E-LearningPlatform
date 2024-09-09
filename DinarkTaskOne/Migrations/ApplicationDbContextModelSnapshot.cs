﻿// <auto-generated />
using System;
using DinarkTaskOne.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace DinarkTaskOne.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    partial class ApplicationDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "8.0.8")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder);

            modelBuilder.Entity("DinarkTaskOne.Models.Authentication_Authorization.RolesModel", b =>
                {
                    b.Property<int>("RoleId")
                        .HasColumnType("int");

                    b.Property<string>("RoleName")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("RoleId");

                    b.ToTable("Roles");

                    b.HasData(
                        new
                        {
                            RoleId = 1,
                            RoleName = "Instructor"
                        },
                        new
                        {
                            RoleId = 2,
                            RoleName = "Student"
                        },
                        new
                        {
                            RoleId = 3,
                            RoleName = "Admin"
                        });
                });

            modelBuilder.Entity("DinarkTaskOne.Models.Authentication_Authorization.SessionCache", b =>
                {
                    b.Property<string>("Id")
                        .HasMaxLength(449)
                        .HasColumnType("nvarchar(449)");

                    b.Property<DateTimeOffset?>("AbsoluteExpiration")
                        .HasColumnType("datetimeoffset");

                    b.Property<DateTimeOffset>("ExpiresAtTime")
                        .HasColumnType("datetimeoffset");

                    b.Property<long?>("SlidingExpirationInSeconds")
                        .HasColumnType("bigint");

                    b.Property<byte[]>("Value")
                        .IsRequired()
                        .HasColumnType("varbinary(max)");

                    b.HasKey("Id");

                    b.ToTable("SessionCache");
                });

            modelBuilder.Entity("DinarkTaskOne.Models.Authentication_Authorization.UsersModel", b =>
                {
                    b.Property<int>("UserId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("UserId"));

                    b.Property<string>("Email")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("FirstName")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("LastName")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("PasswordHash")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("PhoneNumber")
                        .HasColumnType("int");

                    b.Property<int>("RoleId")
                        .HasColumnType("int");

                    b.Property<string>("UserName")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("UserType")
                        .IsRequired()
                        .HasMaxLength(13)
                        .HasColumnType("nvarchar(13)");

                    b.HasKey("UserId");

                    b.HasIndex("RoleId");

                    b.ToTable("Users", (string)null);

                    b.HasDiscriminator<string>("UserType").HasValue("User");

                    b.UseTphMappingStrategy();
                });

            modelBuilder.Entity("DinarkTaskOne.Models.MakeQuiz.AnswerModel", b =>
                {
                    b.Property<int>("AnswerId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("AnswerId"));

                    b.Property<bool>("IsCorrect")
                        .HasColumnType("bit");

                    b.Property<int>("QuestionId")
                        .HasColumnType("int");

                    b.Property<string>("Text")
                        .IsRequired()
                        .HasMaxLength(200)
                        .HasColumnType("nvarchar(200)");

                    b.HasKey("AnswerId");

                    b.HasIndex("QuestionId");

                    b.ToTable("Answer");
                });

            modelBuilder.Entity("DinarkTaskOne.Models.MakeQuiz.AttemptModel", b =>
                {
                    b.Property<int>("AttemptId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("AttemptId"));

                    b.Property<DateTime>("AttemptDate")
                        .HasColumnType("datetime2");

                    b.Property<bool>("Completed")
                        .HasColumnType("bit");

                    b.Property<DateTime>("EndTime")
                        .HasColumnType("datetime2");

                    b.Property<int>("QuizId")
                        .HasColumnType("int");

                    b.Property<int?>("Score")
                        .HasColumnType("int");

                    b.Property<DateTime>("StartTime")
                        .HasColumnType("datetime2");

                    b.Property<int>("StudentId")
                        .HasColumnType("int");

                    b.HasKey("AttemptId");

                    b.HasIndex("QuizId");

                    b.HasIndex("StudentId");

                    b.ToTable("Attempt");
                });

            modelBuilder.Entity("DinarkTaskOne.Models.MakeQuiz.QuestionAnswerModel", b =>
                {
                    b.Property<int>("QuestionAnswerId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("QuestionAnswerId"));

                    b.Property<int>("AttemptId")
                        .HasColumnType("int");

                    b.Property<int>("QuestionId")
                        .HasColumnType("int");

                    b.Property<int>("SelectedOptionId")
                        .HasColumnType("int");

                    b.HasKey("QuestionAnswerId");

                    b.HasIndex("AttemptId");

                    b.HasIndex("QuestionId");

                    b.HasIndex("SelectedOptionId");

                    b.ToTable("QuestionAnswer");
                });

            modelBuilder.Entity("DinarkTaskOne.Models.MakeQuiz.QuestionModel", b =>
                {
                    b.Property<int>("QuestionId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("QuestionId"));

                    b.Property<int>("Marks")
                        .HasColumnType("int");

                    b.Property<int>("QuizId")
                        .HasColumnType("int");

                    b.Property<string>("Text")
                        .IsRequired()
                        .HasMaxLength(500)
                        .HasColumnType("nvarchar(500)");

                    b.Property<int>("Type")
                        .HasColumnType("int");

                    b.HasKey("QuestionId");

                    b.HasIndex("QuizId");

                    b.ToTable("Question");
                });

            modelBuilder.Entity("DinarkTaskOne.Models.MakeQuiz.QuizModel", b =>
                {
                    b.Property<int>("QuizId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("QuizId"));

                    b.Property<int>("CourseId")
                        .HasColumnType("int");

                    b.Property<TimeSpan>("Duration")
                        .HasColumnType("time");

                    b.Property<int>("NumberOfQuestions")
                        .HasColumnType("int");

                    b.Property<string>("Title")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("nvarchar(100)");

                    b.HasKey("QuizId");

                    b.HasIndex("CourseId");

                    b.ToTable("Quiz");
                });

            modelBuilder.Entity("DinarkTaskOne.Models.ManageCourse.AnnouncementModel", b =>
                {
                    b.Property<int>("AnnouncementId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("AnnouncementId"));

                    b.Property<string>("Content")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("CourseId")
                        .HasColumnType("int");

                    b.Property<string>("Type")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("AnnouncementId");

                    b.HasIndex("CourseId");

                    b.ToTable("CourseAnnouncements");
                });

            modelBuilder.Entity("DinarkTaskOne.Models.ManageCourse.CourseModel", b =>
                {
                    b.Property<int>("CourseId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("CourseId"));

                    b.Property<string>("Description")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("InstructorId")
                        .HasColumnType("int");

                    b.Property<string>("Title")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("CourseId");

                    b.HasIndex("InstructorId");

                    b.ToTable("Courses");
                });

            modelBuilder.Entity("DinarkTaskOne.Models.ManageCourse.EnrollModel", b =>
                {
                    b.Property<int>("EnrollmentId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("EnrollmentId"));

                    b.Property<int>("CourseId")
                        .HasColumnType("int");

                    b.Property<int>("StudentId")
                        .HasColumnType("int");

                    b.HasKey("EnrollmentId");

                    b.HasIndex("CourseId");

                    b.HasIndex("StudentId");

                    b.ToTable("Enrollments");
                });

            modelBuilder.Entity("DinarkTaskOne.Models.ManageCourse.MaterialsModel", b =>
                {
                    b.Property<int>("MaterialId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("MaterialId"));

                    b.Property<int>("CourseId")
                        .HasColumnType("int");

                    b.Property<string>("Description")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("FilePath")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("FileType")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("MaterialId");

                    b.HasIndex("CourseId");

                    b.ToTable("CourseMaterials");
                });

            modelBuilder.Entity("DinarkTaskOne.Models.UserSpecficModels.AdminModel", b =>
                {
                    b.HasBaseType("DinarkTaskOne.Models.Authentication_Authorization.UsersModel");

                    b.ToTable("Users");

                    b.HasDiscriminator().HasValue("Admin");
                });

            modelBuilder.Entity("DinarkTaskOne.Models.UserSpecficModels.InstructorModel", b =>
                {
                    b.HasBaseType("DinarkTaskOne.Models.Authentication_Authorization.UsersModel");

                    b.ToTable("Users");

                    b.HasDiscriminator().HasValue("Instructor");
                });

            modelBuilder.Entity("DinarkTaskOne.Models.UserSpecficModels.StudentModel", b =>
                {
                    b.HasBaseType("DinarkTaskOne.Models.Authentication_Authorization.UsersModel");

                    b.ToTable("Users");

                    b.HasDiscriminator().HasValue("Student");
                });

            modelBuilder.Entity("DinarkTaskOne.Models.Authentication_Authorization.UsersModel", b =>
                {
                    b.HasOne("DinarkTaskOne.Models.Authentication_Authorization.RolesModel", "Role")
                        .WithMany("Users")
                        .HasForeignKey("RoleId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Role");
                });

            modelBuilder.Entity("DinarkTaskOne.Models.MakeQuiz.AnswerModel", b =>
                {
                    b.HasOne("DinarkTaskOne.Models.MakeQuiz.QuestionModel", "Question")
                        .WithMany("Answers")
                        .HasForeignKey("QuestionId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Question");
                });

            modelBuilder.Entity("DinarkTaskOne.Models.MakeQuiz.AttemptModel", b =>
                {
                    b.HasOne("DinarkTaskOne.Models.MakeQuiz.QuizModel", "Quiz")
                        .WithMany("Attempts")
                        .HasForeignKey("QuizId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("DinarkTaskOne.Models.UserSpecficModels.StudentModel", "Student")
                        .WithMany("Attempts")
                        .HasForeignKey("StudentId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.Navigation("Quiz");

                    b.Navigation("Student");
                });

            modelBuilder.Entity("DinarkTaskOne.Models.MakeQuiz.QuestionAnswerModel", b =>
                {
                    b.HasOne("DinarkTaskOne.Models.MakeQuiz.AttemptModel", "Attempt")
                        .WithMany("QuestionAnswers")
                        .HasForeignKey("AttemptId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("DinarkTaskOne.Models.MakeQuiz.QuestionModel", "Question")
                        .WithMany("QuestionAnswers")
                        .HasForeignKey("QuestionId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.HasOne("DinarkTaskOne.Models.MakeQuiz.AnswerModel", "SelectedOption")
                        .WithMany()
                        .HasForeignKey("SelectedOptionId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.Navigation("Attempt");

                    b.Navigation("Question");

                    b.Navigation("SelectedOption");
                });

            modelBuilder.Entity("DinarkTaskOne.Models.MakeQuiz.QuestionModel", b =>
                {
                    b.HasOne("DinarkTaskOne.Models.MakeQuiz.QuizModel", "Quiz")
                        .WithMany("Questions")
                        .HasForeignKey("QuizId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Quiz");
                });

            modelBuilder.Entity("DinarkTaskOne.Models.MakeQuiz.QuizModel", b =>
                {
                    b.HasOne("DinarkTaskOne.Models.ManageCourse.CourseModel", "Course")
                        .WithMany("Quizzes")
                        .HasForeignKey("CourseId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Course");
                });

            modelBuilder.Entity("DinarkTaskOne.Models.ManageCourse.AnnouncementModel", b =>
                {
                    b.HasOne("DinarkTaskOne.Models.ManageCourse.CourseModel", "Course")
                        .WithMany("Announcements")
                        .HasForeignKey("CourseId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Course");
                });

            modelBuilder.Entity("DinarkTaskOne.Models.ManageCourse.CourseModel", b =>
                {
                    b.HasOne("DinarkTaskOne.Models.UserSpecficModels.InstructorModel", "Instructor")
                        .WithMany("Courses")
                        .HasForeignKey("InstructorId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Instructor");
                });

            modelBuilder.Entity("DinarkTaskOne.Models.ManageCourse.EnrollModel", b =>
                {
                    b.HasOne("DinarkTaskOne.Models.ManageCourse.CourseModel", "Course")
                        .WithMany("Enrollments")
                        .HasForeignKey("CourseId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.HasOne("DinarkTaskOne.Models.UserSpecficModels.StudentModel", "Student")
                        .WithMany("Enrollments")
                        .HasForeignKey("StudentId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Course");

                    b.Navigation("Student");
                });

            modelBuilder.Entity("DinarkTaskOne.Models.ManageCourse.MaterialsModel", b =>
                {
                    b.HasOne("DinarkTaskOne.Models.ManageCourse.CourseModel", "Course")
                        .WithMany("Materials")
                        .HasForeignKey("CourseId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Course");
                });

            modelBuilder.Entity("DinarkTaskOne.Models.Authentication_Authorization.RolesModel", b =>
                {
                    b.Navigation("Users");
                });

            modelBuilder.Entity("DinarkTaskOne.Models.MakeQuiz.AttemptModel", b =>
                {
                    b.Navigation("QuestionAnswers");
                });

            modelBuilder.Entity("DinarkTaskOne.Models.MakeQuiz.QuestionModel", b =>
                {
                    b.Navigation("Answers");

                    b.Navigation("QuestionAnswers");
                });

            modelBuilder.Entity("DinarkTaskOne.Models.MakeQuiz.QuizModel", b =>
                {
                    b.Navigation("Attempts");

                    b.Navigation("Questions");
                });

            modelBuilder.Entity("DinarkTaskOne.Models.ManageCourse.CourseModel", b =>
                {
                    b.Navigation("Announcements");

                    b.Navigation("Enrollments");

                    b.Navigation("Materials");

                    b.Navigation("Quizzes");
                });

            modelBuilder.Entity("DinarkTaskOne.Models.UserSpecficModels.InstructorModel", b =>
                {
                    b.Navigation("Courses");
                });

            modelBuilder.Entity("DinarkTaskOne.Models.UserSpecficModels.StudentModel", b =>
                {
                    b.Navigation("Attempts");

                    b.Navigation("Enrollments");
                });
#pragma warning restore 612, 618
        }
    }
}
