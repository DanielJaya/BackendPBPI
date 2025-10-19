using BackendPBPI.Data;
using BackendPBPI.Interface.IRepository.Role;
using BackendPBPI.Models.RoleModel;
using Microsoft.EntityFrameworkCore;

namespace BackendPBPI.Repository.Role
{
    public class RoleRepository : IRoleRepository
    {
        private readonly AppDbContext _context;
        private readonly ILogger<RoleRepository> _logger;

        public RoleRepository(AppDbContext context, ILogger<RoleRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        // ============================================
        // Role CRUD Operations
        // ============================================

        public async Task<RoleModel> CreateRoleAsync(RoleModel role)
        {
            try
            {
                _logger.LogInformation("Membuat role baru: {RoleName}", role.RoleName);
                role.CreatedAt = DateTime.UtcNow;
                _context.Roles.Add(role);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Role berhasil dibuat dengan ID: {RoleID}", role.RoleID);
                return role;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saat membuat role: {RoleName}", role.RoleName);
                throw;
            }
        }

        public async Task<RoleModel> GetRoleByIdAsync(int roleId)
        {
            try
            {
                _logger.LogInformation("Mencari role dengan ID: {RoleID}", roleId);
                return await _context.Roles
                    .FirstOrDefaultAsync(r => r.RoleID == roleId && r.DeletedAt == null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saat mencari role dengan ID: {RoleID}", roleId);
                throw;
            }
        }

        public async Task<RoleModel> GetRoleByNameAsync(string roleName)
        {
            try
            {
                _logger.LogInformation("Mencari role dengan nama: {RoleName}", roleName);
                return await _context.Roles
                    .FirstOrDefaultAsync(r => r.RoleName == roleName && r.DeletedAt == null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saat mencari role dengan nama: {RoleName}", roleName);
                throw;
            }
        }

        public async Task<List<RoleModel>> GetAllRolesAsync()
        {
            try
            {
                _logger.LogInformation("Mengambil semua role");
                return await _context.Roles
                    .Where(r => r.DeletedAt == null)
                    .OrderBy(r => r.RoleName)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saat mengambil semua role");
                throw;
            }
        }

        public async Task<RoleModel> UpdateRoleAsync(RoleModel role)
        {
            try
            {
                _logger.LogInformation("Mengupdate role ID: {RoleID}", role.RoleID);
                role.UpdatedAt = DateTime.UtcNow;
                _context.Roles.Update(role);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Role berhasil diupdate: {RoleID}", role.RoleID);
                return role;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saat mengupdate role: {RoleID}", role.RoleID);
                throw;
            }
        }

        public async Task<bool> DeleteRoleAsync(int roleId)
        {
            try
            {
                _logger.LogInformation("Menghapus role ID: {RoleID}", roleId);
                var role = await GetRoleByIdAsync(roleId);

                if (role == null)
                {
                    _logger.LogWarning("Role tidak ditemukan: {RoleID}", roleId);
                    return false;
                }

                // Soft delete
                role.DeletedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                _logger.LogInformation("Role berhasil dihapus: {RoleID}", roleId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saat menghapus role: {RoleID}", roleId);
                throw;
            }
        }

        public async Task<bool> RoleExistsAsync(string roleName)
        {
            try
            {
                _logger.LogInformation("Mengecek keberadaan role: {RoleName}", roleName);
                return await _context.Roles
                    .AnyAsync(r => r.RoleName == roleName && r.DeletedAt == null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saat mengecek role: {RoleName}", roleName);
                throw;
            }
        }

        // ============================================
        // Role-User Assignment Operations
        // ============================================

        public async Task<RoleUserModel> AssignRoleToUserAsync(int userId, int roleId)
        {
            try
            {
                _logger.LogInformation("Assign role {RoleID} ke user {UserID}", roleId, userId);

                // Cek apakah sudah ada assignment
                var existing = await _context.RoleUsers
                    .FirstOrDefaultAsync(ru => ru.UserID == userId && ru.RoleID == roleId);

                if (existing != null)
                {
                    _logger.LogWarning("User {UserID} sudah memiliki role {RoleID}", userId, roleId);
                    return existing;
                }

                var roleUser = new RoleUserModel
                {
                    UserID = userId,
                    RoleID = roleId,
                    CreatedAt = DateTime.UtcNow
                };

                _context.RoleUsers.Add(roleUser);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Role berhasil di-assign ke user");
                return roleUser;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saat assign role {RoleID} ke user {UserID}", roleId, userId);
                throw;
            }
        }

        public async Task<bool> RemoveRoleFromUserAsync(int userId, int roleId)
        {
            try
            {
                _logger.LogInformation("Menghapus role {RoleID} dari user {UserID}", roleId, userId);

                var roleUser = await _context.RoleUsers
                    .FirstOrDefaultAsync(ru => ru.UserID == userId && ru.RoleID == roleId);

                if (roleUser == null)
                {
                    _logger.LogWarning("Role assignment tidak ditemukan");
                    return false;
                }

                _context.RoleUsers.Remove(roleUser);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Role berhasil dihapus dari user");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saat menghapus role dari user");
                throw;
            }
        }

        public async Task<List<RoleModel>> GetUserRolesAsync(int userId)
        {
            try
            {
                _logger.LogInformation("Mengambil roles untuk user ID: {UserID}", userId);

                return await _context.RoleUsers
                    .Where(ru => ru.UserID == userId)
                    .Include(ru => ru.Role)
                    .Where(ru => ru.Role.DeletedAt == null)
                    .Select(ru => ru.Role)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saat mengambil roles untuk user: {UserID}", userId);
                throw;
            }
        }

        public async Task<List<int>> GetUserRoleIdsAsync(int userId)
        {
            try
            {
                _logger.LogInformation("Mengambil role IDs untuk user: {UserID}", userId);

                return await _context.RoleUsers
                    .Where(ru => ru.UserID == userId)
                    .Select(ru => ru.RoleID)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saat mengambil role IDs untuk user: {UserID}", userId);
                throw;
            }
        }

        public async Task<bool> UserHasRoleAsync(int userId, int roleId)
        {
            try
            {
                return await _context.RoleUsers
                    .AnyAsync(ru => ru.UserID == userId && ru.RoleID == roleId);
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
                return await _context.RoleUsers
                    .AnyAsync(ru => ru.UserID == userId && roleIds.Contains(ru.RoleID));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saat cek user roles");
                throw;
            }
        }

        public async Task<int> GetUserRolesCountAsync(int userId)
        {
            try
            {
                return await _context.RoleUsers
                    .CountAsync(ru => ru.UserID == userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saat menghitung user roles");
                throw;
            }
        }

        public async Task<List<int>> GetUserIdsByRoleAsync(int roleId)
        {
            try
            {
                _logger.LogInformation("Mengambil user IDs untuk role: {RoleID}", roleId);

                return await _context.RoleUsers
                    .Where(ru => ru.RoleID == roleId)
                    .Select(ru => ru.UserID)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saat mengambil user IDs untuk role: {RoleID}", roleId);
                throw;
            }
        }
    }
}