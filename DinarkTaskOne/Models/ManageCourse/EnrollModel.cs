using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using DinarkTaskOne.Models.UserSpecficModels;

namespace DinarkTaskOne.Models.ManageCourse
{
    [Table("Enrollments")]
    public class EnrollModel
    {
        [Key]
        public int EnrollmentId { get; set; }

        public int CourseId { get; set; }

        [ForeignKey("CourseId")]
        public virtual CourseModel Course { get; set; } = null!;

        public int StudentId { get; set; }

        [ForeignKey("StudentId")]
        public virtual StudentModel Student { get; set; } = null!;

        // New fields
        public DateTime EnrolledAt { get; set; } = DateTime.UtcNow;

        [Required]
        [MaxLength(20)]
        public string Status { get; set; } = "Active"; // Default status
    }
}
