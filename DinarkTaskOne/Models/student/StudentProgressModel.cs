using DinarkTaskOne.Models.ManageCourse;
using DinarkTaskOne.Models.UserSpecficModels;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DinarkTaskOne.Models.student
{
    public class StudentProgressModel
    {
        [Key]
        public int StudentProgressId { get; set; }

        [Required]
        public int StudentId { get; set; }
        public StudentModel Student { get; set; } = null!; // Reference to the student

        [Required]
        public int LevelId { get; set; }
        public LevelModel Level { get; set; } = null!; // Reference to the academic level

        [Required]
        [Range(0, 100)]
        public double Score { get; set; } // Score for the specific level

        public bool IsCompleted { get; set; } // If the student has completed this level

        public DateTime CompletedAt { get; set; } = DateTime.UtcNow; // Date of completion, if completed
    }
}
