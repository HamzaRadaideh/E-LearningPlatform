using DinarkTaskOne.Models.Authentication_Authorization;
using DinarkTaskOne.Models.MakeQuiz;
using DinarkTaskOne.Models.ManageCourse;
using System.Collections.Generic;

namespace DinarkTaskOne.Models.UserSpecficModels
{
    public class StudentModel : UsersModel
    {
        // Collection to keep track of course enrollments
        public virtual ICollection<EnrollModel> Enrollments { get; set; } = [];

        public virtual ICollection<AttemptModel> Attempts { get; set; } = [];

    }
}
