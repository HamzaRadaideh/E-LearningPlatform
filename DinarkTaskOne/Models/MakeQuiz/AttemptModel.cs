using DinarkTaskOne.Models.UserSpecficModels;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DinarkTaskOne.Models.MakeQuiz
{
    [Table("Attempt")]
    public class AttemptModel
    {
        [Key]
        public int AttemptId { get; set; }

        [ForeignKey("Quiz")]
        public int QuizId { get; set; }
        public virtual QuizModel Quiz { get; set; } = null!;

        [ForeignKey("Student")]
        public int StudentId { get; set; }
        public virtual StudentModel Student { get; set; } = null!;

        public DateTime AttemptDate { get; set; }
        public bool Completed { get; set; }
        public int? Score { get; set; }

        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }

        public int TotalMarks { get; set; } // Maximum marks for this attempt
        public string? Remarks { get; set; } // Feedback or remarks for the attempt

        public virtual ICollection<QuestionAnswerModel> QuestionAnswers { get; set; } = [];
    }

}
