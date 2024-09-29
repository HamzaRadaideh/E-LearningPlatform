using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DinarkTaskOne.Models.MakeQuiz
{
    [Table("Question")]
    public class QuestionModel
    {
        [Key]
        public int QuestionId { get; set; }

        [Required, MaxLength(500)]
        public string Text { get; set; } = string.Empty;

        [Required]
        public QuestionType Type { get; set; }

        [ForeignKey("Quiz")]
        public int QuizId { get; set; }
        public virtual QuizModel Quiz { get; set; } = null!;

        [Range(1, 100)]
        public int Marks { get; set; }

        public int DifficultyLevel { get; set; } = 1; // Difficulty level (1 = Easy, 2 = Medium, 3 = Hard)
        public string? Tag { get; set; } // A tag for categorization (e.g., "Math", "History")

        public virtual ICollection<AnswerModel> Answers { get; set; } = [];

        public virtual ICollection<QuestionAnswerModel> QuestionAnswers { get; set; } = [];
    }


    public enum QuestionType
    {
        MultipleChoice,
        TrueFalse,
        MultipleAnswers
    }
}
