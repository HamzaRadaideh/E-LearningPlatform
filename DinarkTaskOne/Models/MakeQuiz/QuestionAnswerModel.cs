using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DinarkTaskOne.Models.MakeQuiz
{
    [Table("QuestionAnswer")]
    public class QuestionAnswerModel
    {
        [Key]
        public int QuestionAnswerId { get; set; }

        [ForeignKey("Attempt")]
        public int AttemptId { get; set; }
        public virtual AttemptModel Attempt { get; set; } = null!;

        [ForeignKey("Question")]
        public int QuestionId { get; set; }
        public virtual QuestionModel Question { get; set; } = null!;

        [ForeignKey("SelectedOption")]
        public int SelectedOptionId { get; set; }
        public virtual AnswerModel SelectedOption { get; set; } = null!;

        public int ScoreAwarded { get; set; } // Score received for this question
    }

}
