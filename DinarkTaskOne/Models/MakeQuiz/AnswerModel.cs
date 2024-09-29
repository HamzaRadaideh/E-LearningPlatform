using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DinarkTaskOne.Models.MakeQuiz
{
    [Table("Answer")]
    public class AnswerModel
    {
        [Key]
        public int AnswerId { get; set; }

        [Required]
        [StringLength(200)]
        public string Text { get; set; } = string.Empty;

        public bool IsCorrect { get; set; }

        [ForeignKey("Question")]
        public int QuestionId { get; set; }
        public virtual QuestionModel? Question { get; set; }

        public DateTime DateCreated { get; set; } = DateTime.UtcNow;
        public bool IsActive { get; set; } = true; // Soft delete or disable feature
    }

}
