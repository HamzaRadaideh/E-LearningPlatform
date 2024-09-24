using DinarkTaskOne.Models.Authentication_Authorization;
using DinarkTaskOne.Models.Institution;
using DinarkTaskOne.Models.ManageCourse;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace DinarkTaskOne.Models.UserSpecficModels
{
    public class InstructorModel : UsersModel
    {
        // Unique Instructor Identifier (Support Key)
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int InstructorId { get; set; } = 0; // Unique instructor identifier, not a primary key

        [ForeignKey("Department")]
        public int DepartmentId { get; set; } = -1; // Reference to DepartmentModel
        public DepartmentModel Department { get; set; } = null!; // Department relationship

        // Relationship with courses they teach
        public ICollection<CourseModel> Courses { get; set; } = [];
    }
}
