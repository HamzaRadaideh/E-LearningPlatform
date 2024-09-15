using DinarkTaskOne.Models.Institution;
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

        // Relationship with Courses (many-to-many)
        public ICollection<CourseModel> Courses { get; set; } = new List<CourseModel>();

        // Relationship with Students
        public ICollection<StudentModel> Students { get; set; } = new List<StudentModel>();
    }
}