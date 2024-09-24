using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DinarkTaskOne.Models.Authentication_Authorization
{
    [Table("Users")]
    public class UsersModel
    {
        [Key]
        public int UserId { get; set; } // Primary key for all user types

        [Required]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        public string LastName { get; set; } = string.Empty;

        [Required]
        public string UserName { get; set; } = string.Empty;

        [Required]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string PhoneNumber { get; set; } = string.Empty;

        [Required]
        public string PasswordHash { get; set; } = string.Empty;

        public int RoleId { get; set; }

        [Required]
        [MaxLength(13)]
        public string UserType { get; set; } = string.Empty; // User type discriminator

        // Role Relationship
        public RolesModel Role { get; set; } = null!;
    }
}
