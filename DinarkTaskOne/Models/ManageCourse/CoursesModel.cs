using DinarkTaskOne.Models.Institution;
using DinarkTaskOne.Models.MakeQuiz;
using DinarkTaskOne.Models.UserSpecficModels;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;

namespace DinarkTaskOne.Models.ManageCourse
{
    public class CourseModel
    {
        [Key]
        public int CourseId { get; set; }

        [Required]
        public string Title { get; set; } = string.Empty;

        public string? Description { get; set; }

        public int InstructorId { get; set; }
        [ForeignKey("InstructorId")]
        public InstructorModel Instructor { get; set; } = null!;

        public int DepartmentId { get; set; }
        [ForeignKey("DepartmentId")]
        public DepartmentModel Department { get; set; } = null!;

        // Removed CourseMajor relationship
        [Required]
        public string AllowedMajors { get; set; } = ""; // Store comma-separated major IDs

        public ICollection<QuizModel> Quizzes { get; set; } = new List<QuizModel>();
        public ICollection<MaterialsModel> Materials { get; set; } = new List<MaterialsModel>();
        public ICollection<EnrollModel> Enrollments { get; set; } = new List<EnrollModel>();
        public ICollection<AnnouncementModel> Announcements { get; set; } = new List<AnnouncementModel>();

        [Required]
        [Range(1, 200)]
        public int MaxCapacity { get; set; } = 200;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        [Required]
        [MaxLength(50)]
        public string Status { get; set; } = "Active";

        // Property to store the course end date and time
        [Required]
        public DateTime CourseEndTime { get; set; }
    }
}
