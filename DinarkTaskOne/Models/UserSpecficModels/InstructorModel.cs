using DinarkTaskOne.Models.Authentication_Authorization;
using DinarkTaskOne.Models.Institution;
using DinarkTaskOne.Models.ManageCourse;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace DinarkTaskOne.Models.UserSpecficModels
{
    public class InstructorModel : UsersModel
    {

        // Relationship with courses they teach
        public ICollection<CourseModel> Courses { get; set; } = [];
    }
}
