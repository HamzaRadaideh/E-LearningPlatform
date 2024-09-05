using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace DinarkTaskOne.Models.ManageCourse
{
    [Table("CourseAnnouncements")]
    public class AnnouncementModel
    {
        [Key]
        public int AnnouncementId { get; set; }

        public int CourseId { get; set; }

        [ForeignKey("CourseId")]
        public virtual CourseModel Course { get; set; } = null!;

        [Required]
        public string Content { get; set; } = string.Empty;

        [Required]
        public string Type { get; set; } = "Info"; // Can be "Info", "Warning", "Error", etc.
    }
}
