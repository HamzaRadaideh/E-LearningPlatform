using DinarkTaskOne.Models.MakeQuiz;
using DinarkTaskOne.Models.ManageCourse;

namespace DinarkTaskOne.Models.ViewModels
{
    public class CourseViewModel
    {
        public int CourseId { get; set; }
        public required string Title { get; set; }
        public required string Description { get; set; }
        public List<dynamic>? SortedContent { get; set; }

        // Properties for quiz scoring
        public int CombinedQuizScore { get; set; }
        public int MaxPossibleScore { get; set; }
        public double PercentageScore { get; set; }

        // New properties for completed quizzes
        public List<int> CompletedQuizzes { get; set; } = []; // List of completed quiz IDs
        public Dictionary<int, int> CompletedAttempts { get; set; } = []; // Map of quiz ID to attempt ID
    }
}
