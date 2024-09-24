using DinarkTaskOne.Models.Authentication_Authorization;
using DinarkTaskOne.Models.Institution;
using DinarkTaskOne.Models.MakeQuiz;
using DinarkTaskOne.Models.ManageCourse;
using DinarkTaskOne.Models.student;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DinarkTaskOne.Models.UserSpecficModels
{
    public class StudentModel : UsersModel
    {
        // Unique Student Identifier (Support Key)
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int StudentId { get; set; } = 0; // Unique student identifier, not a primary key

        [ForeignKey("CurrentLevel")]
        public int CurrentLevelId { get; set; } = 1; // Reference to LevelModel
        public LevelModel CurrentLevel { get; set; } = null!; // Current Level relationship

        [ForeignKey("Major")]
        public int MajorId { get; set; } = -1; // Reference to MajorModel
        public MajorModel Major { get; set; } = null!; // Major relationship

        // Relationship with enrollments and quiz attempts
        public ICollection<EnrollModel> Enrollments { get; set; } = [];
        public ICollection<AttemptModel> Attempts { get; set; } = [];

        // Properties for tracking student's progress and grades
        public ICollection<StudentProgressModel> Progresses { get; set; } = [];
        public ICollection<StudentGradeModel> Grades { get; set; } = [];
    }
}
