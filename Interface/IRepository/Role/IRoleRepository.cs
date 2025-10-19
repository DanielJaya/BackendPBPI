using BackendPBPI.Models.RoleModel;

namespace BackendPBPI.Interface.IRepository.Role
{
    public interface IRoleRepository
    {
        // Role CRUD Operations
        Task<RoleModel> CreateRoleAsync(RoleModel role);
        Task<RoleModel> GetRoleByIdAsync(int roleId);
        Task<RoleModel> GetRoleByNameAsync(string roleName);
        Task<List<RoleModel>> GetAllRolesAsync();
        Task<RoleModel> UpdateRoleAsync(RoleModel role);
        Task<bool> DeleteRoleAsync(int roleId);
        Task<bool> RoleExistsAsync(string roleName);

        // Role-User Assignment Operations
        Task<RoleUserModel> AssignRoleToUserAsync(int userId, int roleId);
        Task<bool> RemoveRoleFromUserAsync(int userId, int roleId);
        Task<List<RoleModel>> GetUserRolesAsync(int userId);
        Task<List<int>> GetUserRoleIdsAsync(int userId);
        Task<bool> UserHasRoleAsync(int userId, int roleId);
        Task<bool> UserHasAnyRoleAsync(int userId, List<int> roleIds);
        Task<int> GetUserRolesCountAsync(int userId);

        // Get Users by Role
        Task<List<int>> GetUserIdsByRoleAsync(int roleId);
    }
}