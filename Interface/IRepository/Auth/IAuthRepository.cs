using BackendPBPI.Models.AuthModel;
using BackendPBPI.Models.UserModel;

namespace BackendPBPI.Interface.IRepository.Auth
{
    public interface IAuthRepository
    {
        Task<UserModel> GetUserByEmailAsync(string email);
        Task<UserModel> GetUserByUsernameAsync(string username);
        Task<UserModel> GetUserByIdAsync(int userId);
        Task<bool> IsEmailExistsAsync(string email);
        Task<bool> IsUsernameExistsAsync(string username);
        Task<UserModel> CreateUserAsync(UserModel user);
        Task<RefreshToken> SaveRefreshTokenAsync(RefreshToken refreshToken);
        Task<RefreshToken> GetRefreshTokenAsync(string token);
        Task RevokeRefreshTokenAsync(string token);
        Task RevokeAllUserRefreshTokensAsync(int userId);
    }
}
