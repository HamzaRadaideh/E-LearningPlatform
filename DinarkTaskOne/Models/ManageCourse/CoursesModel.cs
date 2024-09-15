using DinarkTaskOne.Models.Institution;
using DinarkTaskOne.Models.MakeQuiz;
using DinarkTaskOne.Models.ManageCourse;
using DinarkTaskOne.Models.UserSpecficModels;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

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
    public DepartmentModel Department { get; set; } = null!;

    // Many-to-many relationship with Majors
    public ICollection<MajorModel> AllowedMajors { get; set; } = new List<MajorModel>();

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
    }
}
