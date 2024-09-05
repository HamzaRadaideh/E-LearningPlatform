using DinarkTaskOne.Models.ManageCourse;

namespace DinarkTaskOne.Models.ViewModels
{
    public class CourseViewModel
    {
        public int CourseId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public List<AnnouncementModel> AnnouncementModelData { get; set; } = [];
        //public List<QuizModel> QuizModelData { get; set; } = new();
        public List<MaterialsModel> MaterialsModelData { get; set; } = [];
    }
}
