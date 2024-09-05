using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DinarkTaskOne.Models.Authentication_Authorization
{
    [Table("Roles")]
    public class RolesModel
    {
        [Key]
        public int RoleId { get; set; }

        [Required]
        public string RoleName { get; set; } = string.Empty;

        public ICollection<UsersModel> Users { get; set; } = [];
    }
}
