namespace DinarkTaskOne.Models.ViewModels
{
    public class TakeQuizViewModel
    {
        public int QuizId { get; set; }
        public string QuizTitle { get; set; } = string.Empty; // Non-nullable by default
        public TimeSpan Duration { get; set; }
        public List<QuestionViewModel> Questions { get; set; } = []; // Initialize collection
    }
}
