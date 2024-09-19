using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DinarkTaskOne.Models.ManageCourse
{
    [Table("CourseAnnouncements")]  // Table name is aligned with the previous structure
    public class AnnouncementModel
    {
        [Key]
        public int AnnouncementId { get; set; }

        public int CourseId { get; set; }

        [ForeignKey("CourseId")]
        public virtual CourseModel Course { get; set; } = null!;

        [Required]
        [StringLength(500)] // Optional: Restrict the length of the content to a reasonable maximum
        public string Content { get; set; } = string.Empty;

        [Required]
        [StringLength(20)] // Restrict the type string length to avoid excessive input
        public string Type { get; set; } = "Info"; // Can be "Info", "Warning", "Error", etc.

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow; // Use UTC time for consistency

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow; // This will store the last update timestamp

    }
}
