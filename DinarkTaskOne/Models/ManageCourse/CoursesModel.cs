using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
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
        public virtual InstructorModel Instructor { get; set; } = null!;

        //public virtual ICollection<QuizModel> Quizzes { get; set; } = [];
        public virtual ICollection<MaterialsModel> Materials { get; set; } = [];
        public virtual ICollection<EnrollModel> Enrollments { get; set; } = [];
        public virtual ICollection<AnnouncementModel> Announcements { get; set; } = [];
        //public virtual ICollection<CourseGradeModel> CourseGrades { get; set; } = new List<CourseGradeModel>(); // Add this line

    }
}
