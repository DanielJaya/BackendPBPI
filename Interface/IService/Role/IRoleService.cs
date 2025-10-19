using BackendPBPI.DTO.RoleDTO;

namespace BackendPBPI.Interface.IService.Role
{
    public interface IRoleService
    {
        // Role CRUD Operations
        Task<RoleResponseDto> CreateRoleAsync(CreateRoleRequestDto request);
        Task<RoleResponseDto> GetRoleByIdAsync(int roleId);
        Task<List<RoleResponseDto>> GetAllRolesAsync();
        Task<RoleResponseDto> UpdateRoleAsync(int roleId, UpdateRoleRequestDto request);
        Task<bool> DeleteRoleAsync(int roleId);

        // Role-User Assignment Operations
        Task<bool> AssignRoleToUserAsync(AssignRoleRequestDto request);
        Task<bool> RemoveRoleFromUserAsync(int userId, int roleId);
        Task<UserRolesResponseDto> GetUserWithRolesAsync(int userId);
        Task<RoleUsersResponseDto> GetRoleWithUsersAsync(int roleId);
        Task<bool> UserHasRoleAsync(int userId, int roleId);
        Task<bool> UserHasAnyRoleAsync(int userId, List<int> roleIds);
    }
}
