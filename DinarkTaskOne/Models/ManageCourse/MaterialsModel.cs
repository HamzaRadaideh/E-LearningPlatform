using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DinarkTaskOne.Models.ManageCourse
{
    [Table("CourseMaterials")]  // Table name is fine
    public class MaterialsModel
    {
        [Key]
        public int MaterialId { get; set; }

        public int CourseId { get; set; }

        [ForeignKey("CourseId")]
        public virtual CourseModel Course { get; set; } = null!;

        [Required]
        [StringLength(500)] // Optional: Restrict file path length
        public string FilePath { get; set; } = string.Empty;

        [Required]
        [StringLength(50)] // Optional: Restrict file type length
        public string FileType { get; set; } = string.Empty;  // E.g., "PDF", "Video", "Link"

        [StringLength(1000)] // Optional: Restrict the description length
        public string? Description { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow; // Use UTC time for consistency

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow; // This will store the last update timestamp

    }
}
