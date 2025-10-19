using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using BackendPBPI.Data;
using BackendPBPI.Models.UserModel;
using BackendPBPI.Interface.IRepository.Auth;
using BackendPBPI.Models.AuthModel;

namespace BackendPBPI.Repositories
{
    public class AuthRepository : IAuthRepository
    {
        private readonly AppDbContext _context;
        private readonly ILogger<AuthRepository> _logger;

        public AuthRepository(AppDbContext context, ILogger<AuthRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<UserModel> GetUserByEmailAsync(string email)
        {
            try
            {
                _logger.LogInformation("Mencari user dengan email: {Email}", email);
                var user = await _context.Users
                    .FirstOrDefaultAsync(u => u.Email == email && u.Status == 1 && u.DeletedAt == null);

                if (user != null)
                    _logger.LogInformation("User dengan email {Email} ditemukan", email);
                else
                    _logger.LogWarning("User dengan email {Email} tidak ditemukan", email);

                return user;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saat mencari user dengan email: {Email}", email);
                throw;
            }
        }

        public async Task<UserModel> GetUserByUsernameAsync(string username)
        {
            try
            {
                _logger.LogInformation("Mencari user dengan username: {Username}", username);
                var user = await _context.Users
                    .FirstOrDefaultAsync(u => u.UserName == username && u.Status == 1 && u.DeletedAt == null);

                if (user != null)
                    _logger.LogInformation("User dengan username {Username} ditemukan", username);
                else
                    _logger.LogWarning("User dengan username {Username} tidak ditemukan", username);

                return user;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saat mencari user dengan username: {Username}", username);
                throw;
            }
        }

        public async Task<UserModel> GetUserByIdAsync(int userId)
        {
            try
            {
                _logger.LogInformation("Mencari user dengan ID: {UserId}", userId);
                return await _context.Users
                    .FirstOrDefaultAsync(u => u.UserID == userId && u.Status == 1 && u.DeletedAt == null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saat mencari user dengan ID: {UserId}", userId);
                throw;
            }
        }

        public async Task<bool> IsEmailExistsAsync(string email)
        {
            try
            {
                _logger.LogInformation("Mengecek keberadaan email: {Email}", email);
                return await _context.Users.AnyAsync(u => u.Email == email && u.DeletedAt == null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saat mengecek email: {Email}", email);
                throw;
            }
        }

        public async Task<bool> IsUsernameExistsAsync(string username)
        {
            try
            {
                _logger.LogInformation("Mengecek keberadaan username: {Username}", username);
                return await _context.Users.AnyAsync(u => u.UserName == username && u.DeletedAt == null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saat mengecek username: {Username}", username);
                throw;
            }
        }

        public async Task<UserModel> CreateUserAsync(UserModel user)
        {
            try
            {
                _logger.LogInformation("Membuat user baru dengan username: {Username}", user.UserName);
                user.CreatedAt = DateTime.UtcNow;
                _context.Users.Add(user);
                await _context.SaveChangesAsync();
                _logger.LogInformation("User berhasil dibuat dengan ID: {UserId}", user.UserID);
                return user;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saat membuat user: {Username}", user.UserName);
                throw;
            }
        }

        public async Task<RefreshToken> SaveRefreshTokenAsync(RefreshToken refreshToken)
        {
            try
            {
                _logger.LogInformation("Menyimpan refresh token untuk user ID: {UserId}", refreshToken.UserId);
                _context.RefreshTokens.Add(refreshToken);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Refresh token berhasil disimpan dengan ID: {TokenId}", refreshToken.Id);
                return refreshToken;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saat menyimpan refresh token untuk user ID: {UserId}", refreshToken.UserId);
                throw;
            }
        }

        public async Task<RefreshToken> GetRefreshTokenAsync(string token)
        {
            try
            {
                _logger.LogInformation("Mencari refresh token");
                var refreshToken = await _context.RefreshTokens
                    .Include(rt => rt.User)
                    .FirstOrDefaultAsync(rt => rt.Token == token && !rt.IsRevoked);

                if (refreshToken != null)
                    _logger.LogInformation("Refresh token ditemukan untuk user ID: {UserId}", refreshToken.UserId);
                else
                    _logger.LogWarning("Refresh token tidak ditemukan atau sudah direvoke");

                return refreshToken;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saat mencari refresh token");
                throw;
            }
        }

        public async Task RevokeRefreshTokenAsync(string token)
        {
            try
            {
                _logger.LogInformation("Merevoke refresh token");
                var refreshToken = await _context.RefreshTokens
                    .FirstOrDefaultAsync(rt => rt.Token == token);

                if (refreshToken != null)
                {
                    refreshToken.IsRevoked = true;
                    refreshToken.RevokedAt = DateTime.UtcNow;
                    await _context.SaveChangesAsync();
                    _logger.LogInformation("Refresh token berhasil direvoke");
                }
                else
                {
                    _logger.LogWarning("Refresh token tidak ditemukan untuk direvoke");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saat merevoke refresh token");
                throw;
            }
        }

        public async Task RevokeAllUserRefreshTokensAsync(int userId)
        {
            try
            {
                _logger.LogInformation("Merevoke semua refresh token untuk user ID: {UserId}", userId);
                var tokens = await _context.RefreshTokens
                    .Where(rt => rt.UserId == userId && !rt.IsRevoked)
                    .ToListAsync();

                foreach (var token in tokens)
                {
                    token.IsRevoked = true;
                    token.RevokedAt = DateTime.UtcNow;
                }

                await _context.SaveChangesAsync();
                _logger.LogInformation("{Count} refresh token berhasil direvoke untuk user ID: {UserId}", tokens.Count, userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saat merevoke semua refresh token untuk user ID: {UserId}", userId);
                throw;
            }
        }
    }
}