using DinarkTaskOne.Models.ManageCourse;
using DinarkTaskOne.Models.UserSpecficModels;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DinarkTaskOne.Models.Institution
{
    public class MajorModel
    {
        [Key]
        public int MajorId { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        public int DepartmentId { get; set; }
        public DepartmentModel Department { get; set; } = null!;

        // Remove the CourseMajors relationship as it's no longer needed
        // public ICollection<CourseMajor> CourseMajors { get; set; } = new List<CourseMajor>();

        // Relationship with Students
        public ICollection<StudentModel> Students { get; set; } = [];

        // Optional: Add a helper property if needed to access related courses based on the AllowedMajors in the CourseModel
        public ICollection<CourseModel> RelatedCourses { get; set; } = [];
    }
}
