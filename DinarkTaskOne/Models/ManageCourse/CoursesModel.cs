using DinarkTaskOne.Models.UserSpecficModels;
using DinarkTaskOne.Models.Institution;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using DinarkTaskOne.Models.MakeQuiz;

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

        // Relationship with Enrollments and Grades
        public ICollection<EnrollModel> Enrollments { get; set; } = [];


        public ICollection<QuizModel> Quizzes { get; set; } = [];
        public ICollection<MaterialsModel> Materials { get; set; } = [];
        public ICollection<AnnouncementModel> Announcements { get; set; } = [];
    }

    public enum StatusType
    {
        Active = 0,
        Completed = 1,
        Cancelled = 2,
        Soon = 3,
        unavailable = 4
    }

}
