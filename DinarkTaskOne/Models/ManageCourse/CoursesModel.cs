using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using DinarkTaskOne.Models.MakeQuiz;
using DinarkTaskOne.Models.UserSpecficModels;

namespace DinarkTaskOne.Models.ManageCourse
{
    [Table("Courses")]
    public class CourseModel
    {
        [Key]
        public int CourseId { get; set; }

        [Required]
        public string Title { get; set; } = string.Empty;

        public string? Description { get; set; }

        public int InstructorId { get; set; }

        [ForeignKey("InstructorId")]
        public virtual InstructorModel Instructor { get; set; } = null!;  // Ensure Instructor is non-nullable

        // Include Quizzes
        public virtual ICollection<QuizModel> Quizzes { get; set; } = [];  // Ensure it's initialized properly

        // Existing collections
        public virtual ICollection<MaterialsModel> Materials { get; set; } = [];
        public virtual ICollection<EnrollModel> Enrollments { get; set; } = [];
        public virtual ICollection<AnnouncementModel> Announcements { get; set; } = [];

    }
}
