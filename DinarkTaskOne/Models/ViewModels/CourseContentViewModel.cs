using DinarkTaskOne.Models.ManageCourse;
using DinarkTaskOne.Models.MakeQuiz;

namespace DinarkTaskOne.Models.ViewModels
{
    public class CourseContentViewModel
    {
        public string Type { get; set; } = string.Empty; // Can be "Material", "Announcement", "Quiz"
        public MaterialsModel? Material { get; set; }
        public AnnouncementModel? Announcement { get; set; }
        public QuizModel? Quiz { get; set; }
        public DateTime CreatedAt { get; set; } // Unified CreatedAt field for sorting
    }
}
