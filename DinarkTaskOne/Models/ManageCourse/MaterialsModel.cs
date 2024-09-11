using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using DinarkTaskOne.Models.ManageCourse;

namespace DinarkTaskOne.Models.ManageCourse
{
    [Table("CourseMaterials")]  // Table name is fine
    public class MaterialsModel
    {
        [Key]
        public int MaterialId { get; set; }

        public int CourseId { get; set; }

        [ForeignKey("CourseId")]
        public virtual CourseModel Course { get; set; } = null!;  // Ensure Course is non-nullable

        [Required]
        public string FilePath { get; set; } = string.Empty;  // Ensure default empty string

        [Required]
        public string FileType { get; set; } = string.Empty;  // E.g., "PDF", "Video", "Link"

        public string? Description { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

    }
}
