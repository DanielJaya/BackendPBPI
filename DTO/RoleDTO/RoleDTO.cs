using System.ComponentModel.DataAnnotations;

namespace BackendPBPI.DTO.RoleDTO
{
    public class CreateRoleRequestDto
    {
        [Required(ErrorMessage = "Role name wajib diisi")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "Role name harus antara 3-50 karakter")]
        public string RoleName { get; set; }
    }

    // ============================================
    // Update Role Request
    // ============================================
    public class UpdateRoleRequestDto
    {
        [Required(ErrorMessage = "Role name wajib diisi")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "Role name harus antara 3-50 karakter")]
        public string RoleName { get; set; }
    }

    // ============================================
    // Assign Role to User Request
    // ============================================
    public class AssignRoleRequestDto
    {
        [Required(ErrorMessage = "User ID wajib diisi")]
        public int UserID { get; set; }

        [Required(ErrorMessage = "Role ID wajib diisi")]
        public int RoleID { get; set; }
    }

    // ============================================
    // Role Response DTO
    // ============================================
    public class RoleResponseDto
    {
        public int RoleID { get; set; }
        public string RoleName { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    // ============================================
    // User with Roles Response DTO
    // ============================================
    public class UserRolesResponseDto
    {
        public int UserID { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public List<RoleResponseDto> Roles { get; set; }
    }

    // ============================================
    // Role with Users Response DTO
    // ============================================
    public class RoleUsersResponseDto
    {
        public int RoleID { get; set; }
        public string RoleName { get; set; }
        public List<UserBasicDto> Users { get; set; }
    }

    // ============================================
    // User Basic Info DTO
    // ============================================
    public class UserBasicDto
    {
        public int UserID { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
    }
}