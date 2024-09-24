using DinarkTaskOne.Models.student;
using DinarkTaskOne.Models.UserSpecficModels;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DinarkTaskOne.Models.ManageCourse
{
    public class LevelModel
    {
        [Key]
        public int LevelId { get; set; }

        [Required]
        public LevelType Type { get; set; } // Enum for levels

        // Relationship with Courses
        public ICollection<CourseModel> Courses { get; set; } = [];

        // Relationship with Students
        public ICollection<StudentModel> Students { get; set; } = [];
        
        // New properties for tracking student's progress and grades
        public ICollection<StudentProgressModel> Progresses { get; set; } = [];
        public ICollection<StudentGradeModel> Grades { get; set; } = [];
    }

    public enum LevelType
    {
        LevelZero,
        LevelOne,
        LevelTwo,
        LevelThree,
        LevelFour
    }
}
