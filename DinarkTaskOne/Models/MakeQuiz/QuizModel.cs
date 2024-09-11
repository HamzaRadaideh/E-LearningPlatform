using DinarkTaskOne.Models.ManageCourse;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DinarkTaskOne.Models.MakeQuiz
{
    [Table("Quiz")]
    public class QuizModel
    {
        [Key]
        public int QuizId { get; set; }

        [Required]
        [StringLength(100)]
        public string Title { get; set; } = string.Empty;

        [ForeignKey("Course")]
        public int CourseId { get; set; }
        public virtual CourseModel? Course { get; set; }

        [Range(2, 10)]
        public int NumberOfQuestions { get; set; }

        [Range(typeof(TimeSpan), "00:00:01", "23:59:59", ErrorMessage = "Duration must be between 00:00:01 and 23:59:59.")]
        public TimeSpan Duration { get; set; }

        public virtual ICollection<QuestionModel> Questions { get; set; } = [];

        public virtual ICollection<AttemptModel> Attempts { get; set; } = [];

        public DateTime CreatedAt { get; set; } = DateTime.Now;

    }
}
