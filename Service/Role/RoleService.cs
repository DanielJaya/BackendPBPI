using BackendPBPI.DTO.RoleDTO;
using BackendPBPI.Interface.IRepository.Auth;
using BackendPBPI.Interface.IRepository.Role;
using BackendPBPI.Interface.IService.Role;
using BackendPBPI.Models.RoleModel;

namespace BackendPBPI.Service.Role
{
    public class RoleService : IRoleService
    {
        private readonly IRoleRepository _roleRepository;
        private readonly IAuthRepository _authRepository;
        private readonly ILogger<RoleService> _logger;

        public RoleService(
            IRoleRepository roleRepository,
            IAuthRepository authRepository,
            ILogger<RoleService> logger)
        {
            _roleRepository = roleRepository;
            _authRepository = authRepository;
            _logger = logger;
        }

        // ============================================
        // Role CRUD Operations
        // ============================================

        public async Task<RoleResponseDto> CreateRoleAsync(CreateRoleRequestDto request)
        {
            try
            {
                _logger.LogInformation("Memulai proses create role: {RoleName}", request.RoleName);

                // Validasi role sudah ada
                if (await _roleRepository.RoleExistsAsync(request.RoleName))
                {
                    _logger.LogWarning("Role {RoleName} sudah ada", request.RoleName);
                    throw new Exception($"Role '{request.RoleName}' sudah ada");
                }

                var role = new RoleModel
                {
                    RoleName = request.RoleName
                };

                var createdRole = await _roleRepository.CreateRoleAsync(role);
                _logger.LogInformation("Role berhasil dibuat: {RoleID}", createdRole.RoleID);

                return MapToRoleResponseDto(createdRole);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saat create role: {RoleName}", request.RoleName);
                throw;
            }
        }

        public async Task<RoleResponseDto> GetRoleByIdAsync(int roleId)
        {
            try
            {
                _logger.LogInformation("Mengambil role dengan ID: {RoleID}", roleId);

                var role = await _roleRepository.GetRoleByIdAsync(roleId);

                if (role == null)
                {
                    _logger.LogWarning("Role tidak ditemukan: {RoleID}", roleId);
                    throw new Exception($"Role dengan ID {roleId} tidak ditemukan");
                }

                return MapToRoleResponseDto(role);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saat mengambil role: {RoleID}", roleId);
                throw;
            }
        }

        public async Task<List<RoleResponseDto>> GetAllRolesAsync()
        {
            try
            {
                _logger.LogInformation("Mengambil semua role");

                var roles = await _roleRepository.GetAllRolesAsync();

                return roles.Select(r => MapToRoleResponseDto(r)).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saat mengambil semua role");
                throw;
            }
        }

        public async Task<RoleResponseDto> UpdateRoleAsync(int roleId, UpdateRoleRequestDto request)
        {
            try
            {
                _logger.LogInformation("Update role ID: {RoleID}", roleId);

                var role = await _roleRepository.GetRoleByIdAsync(roleId);

                if (role == null)
                {
                    _logger.LogWarning("Role tidak ditemukan: {RoleID}", roleId);
                    throw new Exception($"Role dengan ID {roleId} tidak ditemukan");
                }

                // Cek apakah nama role baru sudah digunakan role lain
                var existingRole = await _roleRepository.GetRoleByNameAsync(request.RoleName);
                if (existingRole != null && existingRole.RoleID != roleId)
                {
                    _logger.LogWarning("Role name {RoleName} sudah digunakan", request.RoleName);
                    throw new Exception($"Role name '{request.RoleName}' sudah digunakan");
                }

                role.RoleName = request.RoleName;

                var updatedRole = await _roleRepository.UpdateRoleAsync(role);
                _logger.LogInformation("Role berhasil diupdate: {RoleID}", roleId);

                return MapToRoleResponseDto(updatedRole);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saat update role: {RoleID}", roleId);
                throw;
            }
        }

        public async Task<bool> DeleteRoleAsync(int roleId)
        {
            try
            {
                _logger.LogInformation("Delete role ID: {RoleID}", roleId);

                var role = await _roleRepository.GetRoleByIdAsync(roleId);

                if (role == null)
                {
                    _logger.LogWarning("Role tidak ditemukan: {RoleID}", roleId);
                    throw new Exception($"Role dengan ID {roleId} tidak ditemukan");
                }

                // Cek apakah role masih digunakan
                var userCount = (await _roleRepository.GetUserIdsByRoleAsync(roleId)).Count;
                if (userCount > 0)
                {
                    _logger.LogWarning("Role {RoleID} masih digunakan oleh {Count} user", roleId, userCount);
                    throw new Exception($"Role tidak dapat dihapus karena masih digunakan oleh {userCount} user");
                }

                var result = await _roleRepository.DeleteRoleAsync(roleId);
                _logger.LogInformation("Role berhasil dihapus: {RoleID}", roleId);

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saat delete role: {RoleID}", roleId);
                throw;
            }
        }

        // ============================================
        // Role-User Assignment Operations
        // ============================================

        public async Task<bool> AssignRoleToUserAsync(AssignRoleRequestDto request)
        {
            try
            {
                _logger.LogInformation("Assign role {RoleID} ke user {UserID}", request.RoleID, request.UserID);

                // Validasi user exists
                var user = await _authRepository.GetUserByIdAsync(request.UserID);
                if (user == null)
                {
                    _logger.LogWarning("User tidak ditemukan: {UserID}", request.UserID);
                    throw new Exception($"User dengan ID {request.UserID} tidak ditemukan");
                }

                // Validasi role exists
                var role = await _roleRepository.GetRoleByIdAsync(request.RoleID);
                if (role == null)
                {
                    _logger.LogWarning("Role tidak ditemukan: {RoleID}", request.RoleID);
                    throw new Exception($"Role dengan ID {request.RoleID} tidak ditemukan");
                }

                // Cek apakah user sudah memiliki role ini
                if (await _roleRepository.UserHasRoleAsync(request.UserID, request.RoleID))
                {
                    _logger.LogWarning("User {UserID} sudah memiliki role {RoleID}", request.UserID, request.RoleID);
                    throw new Exception($"User sudah memiliki role '{role.RoleName}'");
                }

                await _roleRepository.AssignRoleToUserAsync(request.UserID, request.RoleID);
                _logger.LogInformation("Role berhasil di-assign ke user");

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saat assign role ke user");
                throw;
            }
        }

        public async Task<bool> RemoveRoleFromUserAsync(int userId, int roleId)
        {
            try
            {
                _logger.LogInformation("Remove role {RoleID} dari user {UserID}", roleId, userId);

                // Validasi user exists
                var user = await _authRepository.GetUserByIdAsync(userId);
                if (user == null)
                {
                    _logger.LogWarning("User tidak ditemukan: {UserID}", userId);
                    throw new Exception($"User dengan ID {userId} tidak ditemukan");
                }

                // Validasi role exists
                var role = await _roleRepository.GetRoleByIdAsync(roleId);
                if (role == null)
                {
                    _logger.LogWarning("Role tidak ditemukan: {RoleID}", roleId);
                    throw new Exception($"Role dengan ID {roleId} tidak ditemukan");
                }

                // Cek apakah user memiliki role ini
                if (!await _roleRepository.UserHasRoleAsync(userId, roleId))
                {
                    _logger.LogWarning("User {UserID} tidak memiliki role {RoleID}", userId, roleId);
                    throw new Exception($"User tidak memiliki role '{role.RoleName}'");
                }

                var result = await _roleRepository.RemoveRoleFromUserAsync(userId, roleId);
                _logger.LogInformation("Role berhasil dihapus dari user");

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saat remove role dari user");
                throw;
            }
        }

        public async Task<UserRolesResponseDto> GetUserWithRolesAsync(int userId)
        {
            try
            {
                _logger.LogInformation("Mengambil user dengan roles: {UserID}", userId);

                var user = await _authRepository.GetUserByIdAsync(userId);
                if (user == null)
                {
                    _logger.LogWarning("User tidak ditemukan: {UserID}", userId);
                    throw new Exception($"User dengan ID {userId} tidak ditemukan");
                }

                var roles = await _roleRepository.GetUserRolesAsync(userId);

                return new UserRolesResponseDto
                {
                    UserID = user.UserID,
                    UserName = user.UserName,
                    Email = user.Email,
                    Roles = roles.Select(r => MapToRoleResponseDto(r)).ToList()
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saat mengambil user dengan roles: {UserID}", userId);
                throw;
            }
        }

        public async Task<RoleUsersResponseDto> GetRoleWithUsersAsync(int roleId)
        {
            try
            {
                _logger.LogInformation("Mengambil role dengan users: {RoleID}", roleId);

                var role = await _roleRepository.GetRoleByIdAsync(roleId);
                if (role == null)
                {
                    _logger.LogWarning("Role tidak ditemukan: {RoleID}", roleId);
                    throw new Exception($"Role dengan ID {roleId} tidak ditemukan");
                }

                var userIds = await _roleRepository.GetUserIdsByRoleAsync(roleId);
                var users = new List<UserBasicDto>();

                foreach (var userId in userIds)
                {
                    var user = await _authRepository.GetUserByIdAsync(userId);
                    if (user != null)
                    {
                        users.Add(new UserBasicDto
                        {
                            UserID = user.UserID,
                            UserName = user.UserName,
                            Email = user.Email
                        });
                    }
                }

                return new RoleUsersResponseDto
                {
                    RoleID = role.RoleID,
                    RoleName = role.RoleName,
                    Users = users
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saat mengambil role dengan users: {RoleID}", roleId);
                throw;
            }
        }

        public async Task<bool> UserHasRoleAsync(int userId, int roleId)
        {
            try
            {
                return await _roleRepository.UserHasRoleAsync(userId, roleId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saat cek user role");
                throw;
            }
        }

        public async Task<bool> UserHasAnyRoleAsync(int userId, List<int> roleIds)
        {
            try
            {
                return await _roleRepository.UserHasAnyRoleAsync(userId, roleIds);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saat cek user roles");
                throw;
            }
        }

        // ============================================
        // Helper Methods
        // ============================================

        private RoleResponseDto MapToRoleResponseDto(RoleModel role)
        {
            return new RoleResponseDto
            {
                RoleID = role.RoleID,
                RoleName = role.RoleName,
                CreatedAt = role.CreatedAt,
                UpdatedAt = role.UpdatedAt
            };
        }
    }
}