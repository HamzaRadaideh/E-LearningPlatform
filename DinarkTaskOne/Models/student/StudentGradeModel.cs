using DinarkTaskOne.Models.ManageCourse;
using DinarkTaskOne.Models.UserSpecficModels;
using System.ComponentModel.DataAnnotations;

namespace DinarkTaskOne.Models.student
{
    public class StudentGradeModel
    {
        [Key]
        public int StudentGradeId { get; set; }

        [Required]
        public int StudentId { get; set; }
        public StudentModel Student { get; set; } = null!; // Reference to the student

        [Required]
        public int LevelId { get; set; }
        public LevelModel Level { get; set; } = null!; // Reference to the academic level

        [Required]
        [Range(0, 100)]
        public double AverageScore { get; set; } // Average score for the level

        public bool HasPassed { get; set; } // If the student passed the level

        [Required]
        public string OverallGrade { get; set; } // Overall grade for the level

        public DateTime CalculatedAt { get; set; } = DateTime.UtcNow; // Date of grade calculation
    }
}
