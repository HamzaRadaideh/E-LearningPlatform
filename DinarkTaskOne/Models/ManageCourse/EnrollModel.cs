using DinarkTaskOne.Models.UserSpecficModels;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DinarkTaskOne.Models.ManageCourse
{
    [Table("Enrollments")]
    public class EnrollModel
    {
        [Key]
        public int EnrollmentId { get; set; }

        [Required]
        public int CourseId { get; set; }
        public CourseModel Course { get; set; } = null!; // Reference to Course

        [Required]
        public int StudentId { get; set; }
        public StudentModel Student { get; set; } = null!; // Reference to Student

        public DateTime EnrolledAt { get; set; } = DateTime.UtcNow;

        [Required]
        [MaxLength(20)]
        public string Status { get; set; } = "Active"; // e.g., Active, Completed, Dropped
    }
}
