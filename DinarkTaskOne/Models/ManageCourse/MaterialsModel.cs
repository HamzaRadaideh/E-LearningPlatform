using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using DinarkTaskOne.Models.ManageCourse;

namespace DinarkTaskOne.Models.ManageCourse
{
    [Table("CourseMaterials")]
    public class MaterialsModel
    {
        [Key]
        public int MaterialId { get; set; }

        public int CourseId { get; set; }

        [ForeignKey("CourseId")]
        public virtual CourseModel Course { get; set; } = null!;

        [Required]
        public string FilePath { get; set; } = string.Empty;

        [Required]
        public string FileType { get; set; } = string.Empty; // E.g., "PDF", "Video", "Link"

        public string? Description { get; set; }
    }
}
