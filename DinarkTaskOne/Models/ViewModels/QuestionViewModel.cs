using DinarkTaskOne.Models.MakeQuiz;

namespace DinarkTaskOne.Models.ViewModels
{
    public class QuestionViewModel
    {
        public int QuestionId { get; set; }
        public required string Text { get; set; }
        public QuestionType Type { get; set; }
        public List<AnswerViewModel> Answers { get; set; } = [];
    }
}
