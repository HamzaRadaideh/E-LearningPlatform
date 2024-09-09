using DinarkTaskOne.Models.MakeQuiz;
using DinarkTaskOne.Models.ManageCourse;

namespace DinarkTaskOne.Models.ViewModels
{
    public class CourseViewModel
    {
        public int CourseId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public List<AnnouncementModel> AnnouncementModelData { get; set; } = [];
        public List<QuizModel> Quizzes { get; set; } = []; // Add this property to hold the list of quizzes
        public List<MaterialsModel> MaterialsModelData { get; set; } = [];
    }
}
