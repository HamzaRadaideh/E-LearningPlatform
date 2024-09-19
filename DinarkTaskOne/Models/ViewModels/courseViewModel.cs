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
        public List<int> CompletedQuizzes { get; set; } = new List<int>();

        public Dictionary<int, int> CompletedAttempts { get; set; } = new Dictionary<int, int>();

        // Property to hold the selected majors as a list of integers
        public List<int> SelectedMajors { get; set; } = new List<int>();

        // List of available majors for selection
        public List<SelectListItem>? AvailableMajors { get; set; }
    }
}
