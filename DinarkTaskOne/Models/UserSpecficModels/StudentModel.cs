using DinarkTaskOne.Models.Authentication_Authorization;
using DinarkTaskOne.Models.ManageCourse;
using System.Collections.Generic;

namespace DinarkTaskOne.Models.UserSpecficModels
{
    public class StudentModel : UsersModel
    {
        // Collection to keep track of course enrollments
        public virtual ICollection<EnrollModel> Enrollments { get; set; } = [];

        //// Tracking grades and completion status of enrolled courses
        //public virtual ICollection<CourseGradeModel> CourseGrades { get; set; } = new List<CourseGradeModel>();

        //// Collection of quiz attempts made by the student (one attempt per quiz)
        //public virtual ICollection<QuizAttemptModel> QuizAttempts { get; set; } = new List<QuizAttemptModel>();

        //// Method to check if a student has passed a course
        //public bool IsCoursePassed(int courseId)
        //{
        //    var grade = CourseGrades?.FirstOrDefault(cg => cg.CourseId == courseId);
        //    return grade != null && grade.Grade >= 50;
        //}
    }
}
