using System.ComponentModel.DataAnnotations;
using BackendPBPI.Models.RoleModel;

namespace BackendPBPI.Models.UserModels
{
    public class UserModel
    {
        [Key]
        public int UserID { get; set; }
        [Required]
        public string UserName { get; set; }
        [Required]
        public string Email { get; set; }
        [Required]
        public string Password { get; set; }
        [Required]
        public byte Status { get; set; } = 1;
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }
        // Navigation property
        public virtual ICollection<RoleUserModel> RoleUsers { get; set; }
    }
}
