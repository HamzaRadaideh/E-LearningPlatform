using DinarkTaskOne.Models.Authentication_Authorization;
using DinarkTaskOne.Models.MakeQuiz;
using DinarkTaskOne.Models.ManageCourse;

public class StudentModel : UsersModel
{
    // Collection to keep track of course enrollments
    public virtual ICollection<EnrollModel> Enrollments { get; set; } = [];
    public virtual ICollection<AttemptModel> Attempts { get; set; } = [];

    // Method to count the number of quiz attempts with a score for a specific course
    public int GetQuizAttemptsCount(int courseId)
    {
        return Attempts
            .Where(a => a.Quiz.CourseId == courseId && a.Score.HasValue)
            .Count();
    }
}
