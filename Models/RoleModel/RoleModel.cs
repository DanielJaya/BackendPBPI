using System.ComponentModel.DataAnnotations;

namespace BackendPBPI.Models.RoleModel
{
    public class RoleModel
    {
        [Key]
        public int RoleID { get; set; }

        [Required]
        [StringLength(50)]
        public string RoleName { get; set; }

        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }

        // Navigation property
        public virtual ICollection<RoleUserModel> RoleUsers { get; set; }
    }
}
