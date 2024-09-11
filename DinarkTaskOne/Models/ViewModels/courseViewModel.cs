using DinarkTaskOne.Models.MakeQuiz;
using DinarkTaskOne.Models.ManageCourse;

namespace DinarkTaskOne.Models.ViewModels
{
    public class CourseViewModel
    {
        public int CourseId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public List<dynamic> SortedContent { get; set; } 
        public int CombinedQuizScore { get; set; } 
        public int MaxPossibleScore { get; set; }  
        public double PercentageScore { get; set; } 
    }
}
