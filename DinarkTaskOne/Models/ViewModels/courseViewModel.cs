using DinarkTaskOne.Models.ManageCourse;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
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

        public StatusType Status { get; set; } // Status of the course

        public ICollection<EnrollModel> Enrollments { get; set; } = new List<EnrollModel>();

        // New Properties for Course Configuration
        public double RemainingMarks { get; set; } = 100; // Default remaining marks
        public double CourseCompletionPercentage { get; set; }
        public double QuizCompletionPercentage { get; set; }

        // New Properties for Dashboard and Statistics
        public int TotalStudentsEnrolled { get; set; }
        public int TotalMaterialsUploaded { get; set; }
        public int TotalQuizzesCreated { get; set; }

        // Properties for quiz scoring and completion
        public int CombinedQuizScore { get; set; }
        public int MaxPossibleScore { get; set; }
        public double PercentageScore { get; set; }

        // Properties for completed quizzes
        public List<int> CompletedQuizzes { get; set; } = new List<int>();
        public Dictionary<int, int> CompletedAttempts { get; set; } = new Dictionary<int, int>();

        // For displaying student grades and progress in the course
        public double? StudentGrade { get; set; } // Student's grade in the course, if available
        public string? OverallGrade { get; set; } // Overall grade letter
        public bool HasPassed { get; set; } // Whether the student has passed the course

        // Property to hold the selected majors as a list of integers
        public List<int> SelectedMajors { get; set; } = new List<int>();

        // List of available majors for selection
        public List<SelectListItem>? AvailableMajors { get; set; } = new List<SelectListItem>();

        // List of available levels for selection
        public List<SelectListItem>? AvailableLevels { get; set; } = new List<SelectListItem>();

        // Selected level ID
        public int SelectedLevelId { get; set; }

        // Use a strongly-typed model for quiz and other content if necessary
        public List<CourseContentViewModel> SortedContent { get; set; } = new List<CourseContentViewModel>();
    }
}
