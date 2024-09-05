using DinarkTaskOne.Models.Authentication_Authorization;
using DinarkTaskOne.Models.ManageCourse;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace DinarkTaskOne.Models.UserSpecficModels
{
    public class InstructorModel : UsersModel
    {
        public virtual ICollection<CourseModel> Courses { get; set; } = [];
    }
}