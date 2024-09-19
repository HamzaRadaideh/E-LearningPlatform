using DinarkTaskOne.Models.ManageCourse;
using DinarkTaskOne.Models.UserSpecficModels;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DinarkTaskOne.Models.Institution
{
    public class DepartmentModel
    {
        [Key]
        public int DepartmentId { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        // Relationship with Majors
        public ICollection<MajorModel> Majors { get; set; } = [];

        // Relationship with Courses
        public ICollection<CourseModel> Courses { get; set; } = [];

        // Relationship with Instructors
        public ICollection<InstructorModel> Instructors { get; set; } = [];
    }
}
