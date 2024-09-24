using DinarkTaskOne.Models.ManageCourse;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;

namespace DinarkTaskOne.Models.ViewModels
{
    public class CourseViewModel
    {
        public int CourseId { get; set; }

        public string Title { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public int MaxCapacity { get; set; } = 200;

        public DateTime CourseEndTime { get; set; }

        // Use a strongly-typed model for quiz and other content if necessary
        public List<CourseContentViewModel>? SortedContent { get; set; }

        // Properties for quiz scoring
        public int CombinedQuizScore { get; set; }

        public int MaxPossibleScore { get; set; }

        public double PercentageScore { get; set; }

        // New properties for completed quizzes
        public List<int> CompletedQuizzes { get; set; } = [];

        public Dictionary<int, int> CompletedAttempts { get; set; } = [];

        // Property to hold the selected majors as a list of integers
        public List<int> SelectedMajors { get; set; } = [];

        // List of available majors for selection
        public List<SelectListItem>? AvailableMajors { get; set; }

        // List of available levels for selection
        public List<SelectListItem>? AvailableLevels { get; set; }

        // Selected level ID
        public int SelectedLevelId { get; set; }

        public ICollection<EnrollModel> Enrollments { get; set; } = [];

        // New properties to display in ViewCourse
        public StatusType Status { get; set; } // Status of the course

        public double? StudentGrade { get; set; } // Student's grade in the course, if available

        public string? OverallGrade { get; set; } // Overall grade letter
        
        public bool HasPassed { get; set; } // Whether the student has passed the course
    }
}
