using DinarkTaskOne.Models.UserSpecficModels;
using DinarkTaskOne.Models.Institution;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using DinarkTaskOne.Models.MakeQuiz;
using DinarkTaskOne.Models.student;

namespace DinarkTaskOne.Models.ManageCourse
{
    public class CourseModel
    {
        [Key]
        public int CourseId { get; set; }

        [Required]
        public string Title { get; set; } = string.Empty;

        [Required]
        public string Description { get; set; } = string.Empty;

        [Required]
        public int InstructorId { get; set; }
        public InstructorModel Instructor { get; set; } = null!; // Reference to Instructor

        [Required]
        public int DepartmentId { get; set; }
        public DepartmentModel Department { get; set; } = null!; // Reference to Department

        [Required]
        public int LevelId { get; set; }
        public LevelModel Level { get; set; } = null!; // Reference to Level

        [Required]
        [Range(1, 200)]
        public int MaxCapacity { get; set; } = 200;

        [Required]
        public string AllowedMajors { get; set; } = string.Empty; // Comma-separated Major IDs

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        [Required]
        public StatusType Status { get; set; }

        [Required]
        public DateTime CourseEndTime { get; set; }

        // New property for Remaining Marks
        public double RemainingMarks { get; set; } = 100; // Default remaining marks

        // Relationship with Enrollments and Grades
        public ICollection<EnrollModel> Enrollments { get; set; } = new List<EnrollModel>();

        // New relationship with CourseGrades
        public ICollection<CourseGradeModel> CourseGrades { get; set; } = new List<CourseGradeModel>();

        // New relationship with StudentProgress
        public ICollection<StudentProgressModel> StudentProgress { get; set; } = new List<StudentProgressModel>();

        public ICollection<QuizModel> Quizzes { get; set; } = new List<QuizModel>();
        public ICollection<MaterialsModel> Materials { get; set; } = new List<MaterialsModel>();
        public ICollection<AnnouncementModel> Announcements { get; set; } = new List<AnnouncementModel>();
    }

    public enum StatusType
    {
        Active = 0,
        Completed = 1,
        Cancelled = 2,
        Soon = 3,
        Unavailable = 4 // Update to match the view
    }
}