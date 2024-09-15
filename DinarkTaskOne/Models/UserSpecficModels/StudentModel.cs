using DinarkTaskOne.Models.Authentication_Authorization;
using DinarkTaskOne.Models.Institution;
using DinarkTaskOne.Models.MakeQuiz;
using DinarkTaskOne.Models.ManageCourse;

namespace DinarkTaskOne.Models.UserSpecficModels
{
    public class StudentModel : UsersModel
    {
        public int MajorId { get; set; }
        public MajorModel Major { get; set; } = null!;

        public ICollection<EnrollModel> Enrollments { get; set; } = new List<EnrollModel>();
        public ICollection<AttemptModel> Attempts { get; set; } = new List<AttemptModel>();

        // Method to count the number of quiz attempts with a score for a specific course
        public int GetQuizAttemptsCount(int courseId)
        {
            return Attempts
                .Where(a => a.Quiz.CourseId == courseId && a.Score.HasValue)
                .Count();
        }
    }
}