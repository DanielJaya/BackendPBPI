using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace BackendPBPI.Models.RoleModel
{
    public class RoleUserModel
    {
        [Key]
        public int RoleUserID { get; set; }

        [Required]
        public int UserID { get; set; }

        [Required]
        public int RoleID { get; set; }

        public DateTime? CreatedAt { get; set; }

        // Navigation properties
        [ForeignKey("UserID")]
        public virtual UserModels.UserModel User { get; set; }

        [ForeignKey("RoleID")]
        public virtual RoleModel Role { get; set; }
    }
}
