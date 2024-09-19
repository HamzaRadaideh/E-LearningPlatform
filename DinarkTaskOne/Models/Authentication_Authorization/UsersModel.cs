using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using DinarkTaskOne.Models.Institution;

namespace DinarkTaskOne.Models.Authentication_Authorization
{
    [Table("Users")]
    public class UsersModel
    {
        [Key]
        public int UserId { get; set; }

        [Required]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        public string LastName { get; set; } = string.Empty;

        [Required]
        public string UserName { get; set; } = string.Empty;

        [Required]
        public string Email { get; set; } = string.Empty;

        // Change PhoneNumber to string to handle different formats
        [Required]
        public string PhoneNumber { get; set; } = string.Empty;

        [Required]
        public string PasswordHash { get; set; } = string.Empty;

        public int RoleId { get; set; }

        [Required]
        [MaxLength(13)]
        public string UserType { get; set; } = string.Empty;

        // Nullable DepartmentId and MajorId
        public int? DepartmentId { get; set; }
        public DepartmentModel? Department { get; set; }

        public int? MajorId { get; set; }
        public MajorModel? Major { get; set; }

        // Role should not be nullable
        public RolesModel Role { get; set; } = null!;
    }
}
