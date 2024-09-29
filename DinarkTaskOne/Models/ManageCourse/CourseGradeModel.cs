using DinarkTaskOne.Models.UserSpecficModels;
using System.ComponentModel.DataAnnotations;

namespace DinarkTaskOne.Models.ManageCourse
{
    public class CourseGradeModel
    {
        [Key]
        public int CourseGradeId { get; set; }

        // Make CourseId nullable to support nullable foreign key
        public int? CourseId { get; set; } // Nullable CourseId
        public CourseModel? Course { get; set; } // Nullable navigation property

        [Required]
        public int StudentId { get; set; }
        public StudentModel Student { get; set; } = null!;

        [Range(0, 100)]
        public double Score { get; set; } // Score for this course

        public bool HasPassed { get; set; } // True if score >= 50

        public string LetterGrade { get; set; } = "F"; // A, B, C, D, E, F

        public static string CalculateLetterGrade(double score)
        {
            if (score >= 90) return "A";
            if (score >= 80) return "B";
            if (score >= 70) return "C";
            if (score >= 60) return "D";
            if (score >= 50) return "E";
            return "F";
        }
    }
}
