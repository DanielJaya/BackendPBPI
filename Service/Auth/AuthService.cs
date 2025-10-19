using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using BackendPBPI.Configuration;
using BackendPBPI.Models.UserModel;
using BackendPBPI.Interface.IRepository.Auth;
using BackendPBPI.Interface.IService.Auth;
using BackendPBPI.Models.AuthModel;
using static BackendPBPI.DTO.AuthDTO.AuthDTO;

namespace BackendPBPI.Services
{
    public class AuthService : IAuthService
    {
        private readonly IAuthRepository _authRepository;
        private readonly IPasswordHasher<UserModel> _passwordHasher;
        private readonly JwtSettings _jwtSettings;
        private readonly ILogger<AuthService> _logger;

        public AuthService(
            IAuthRepository authRepository,
            IPasswordHasher<UserModel> passwordHasher,
            IOptions<JwtSettings> jwtSettings,
            ILogger<AuthService> logger)
        {
            _authRepository = authRepository;
            _passwordHasher = passwordHasher;
            _jwtSettings = jwtSettings.Value;
            _logger = logger;
        }

        public async Task<AuthResponseDto> RegisterAsync(RegisterRequestDto request)
        {
            try
            {
                _logger.LogInformation("Memulai proses registrasi untuk username: {Username}", request.UserName);

                // Validasi email sudah digunakan
                if (await _authRepository.IsEmailExistsAsync(request.Email))
                {
                    _logger.LogWarning("Email {Email} sudah terdaftar", request.Email);
                    throw new Exception("Email sudah terdaftar");
                }

                // Validasi username sudah digunakan
                if (await _authRepository.IsUsernameExistsAsync(request.UserName))
                {
                    _logger.LogWarning("Username {Username} sudah terdaftar", request.UserName);
                    throw new Exception("Username sudah terdaftar");
                }

                // Create user object
                var user = new UserModel
                {
                    UserName = request.UserName,
                    Email = request.Email,
                    Status = 1
                };

                // Hash password
                _logger.LogInformation("Melakukan hashing password untuk user: {Username}", request.UserName);
                user.Password = _passwordHasher.HashPassword(user, request.Password);

                // Save user to database
                var createdUser = await _authRepository.CreateUserAsync(user);
                _logger.LogInformation("User berhasil dibuat dengan ID: {UserId}", createdUser.UserID);

                // Generate tokens
                var accessToken = GenerateAccessToken(createdUser);
                var refreshToken = GenerateRefreshToken();

                // Save refresh token
                await SaveRefreshTokenAsync(createdUser.UserID, refreshToken);

                _logger.LogInformation("Registrasi berhasil untuk user: {Username}", createdUser.UserName);

                return new AuthResponseDto
                {
                    Username = createdUser.UserName,
                    Role = new List<int> { 6 }, // Default role
                    AccessToken = accessToken,
                    RefreshToken = refreshToken
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saat registrasi user: {Username}", request.UserName);
                throw;
            }
        }

        public async Task<AuthResponseDto> LoginAsync(LoginRequestDto request)
        {
            try
            {
                _logger.LogInformation("Memulai proses login untuk: {UsernameOrEmail}", request.UsernameOrEmail);

                // Cari user berdasarkan email atau username
                UserModel user = null;

                // Cek apakah input adalah email
                if (request.UsernameOrEmail.Contains("@"))
                {
                    _logger.LogInformation("Login menggunakan email");
                    user = await _authRepository.GetUserByEmailAsync(request.UsernameOrEmail);
                }
                else
                {
                    _logger.LogInformation("Login menggunakan username");
                    user = await _authRepository.GetUserByUsernameAsync(request.UsernameOrEmail);
                }

                // Validasi user ditemukan
                if (user == null)
                {
                    _logger.LogWarning("User tidak ditemukan: {UsernameOrEmail}", request.UsernameOrEmail);
                    throw new Exception("Username/Email atau password salah");
                }

                // Verify password
                _logger.LogInformation("Memverifikasi password untuk user ID: {UserId}", user.UserID);
                var passwordVerification = _passwordHasher.VerifyHashedPassword(user, user.Password, request.Password);

                if (passwordVerification == PasswordVerificationResult.Failed)
                {
                    _logger.LogWarning("Password salah untuk user: {Username}", user.UserName);
                    throw new Exception("Username/Email atau password salah");
                }

                // Generate tokens
                var accessToken = GenerateAccessToken(user);
                var refreshToken = GenerateRefreshToken();

                // Revoke old refresh tokens (optional - untuk keamanan)
                await _authRepository.RevokeAllUserRefreshTokensAsync(user.UserID);

                // Save new refresh token
                await SaveRefreshTokenAsync(user.UserID, refreshToken);

                // Update last login (optional)
                user.UpdatedAt = DateTime.UtcNow;

                _logger.LogInformation("Login berhasil untuk user: {Username}", user.UserName);

                return new AuthResponseDto
                {
                    Username = user.UserName,
                    Role = new List<int> { 6 },
                    AccessToken = accessToken,
                    RefreshToken = refreshToken
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saat login: {UsernameOrEmail}", request.UsernameOrEmail);
                throw;
            }
        }

        public async Task<AuthResponseDto> RefreshTokenAsync(string refreshToken)
        {
            try
            {
                _logger.LogInformation("Memulai proses refresh token");

                // Validasi refresh token
                var storedToken = await _authRepository.GetRefreshTokenAsync(refreshToken);

                if (storedToken == null)
                {
                    _logger.LogWarning("Refresh token tidak valid atau sudah direvoke");
                    throw new Exception("Refresh token tidak valid");
                }

                if (storedToken.ExpiresAt < DateTime.UtcNow)
                {
                    _logger.LogWarning("Refresh token sudah expired");
                    throw new Exception("Refresh token sudah expired");
                }

                // Get user
                var user = await _authRepository.GetUserByIdAsync(storedToken.UserId);
                if (user == null)
                {
                    _logger.LogWarning("User tidak ditemukan untuk refresh token");
                    throw new Exception("User tidak ditemukan");
                }

                // Generate new tokens
                var newAccessToken = GenerateAccessToken(user);
                var newRefreshToken = GenerateRefreshToken();

                // Revoke old refresh token
                await _authRepository.RevokeRefreshTokenAsync(refreshToken);

                // Save new refresh token
                await SaveRefreshTokenAsync(user.UserID, newRefreshToken);

                _logger.LogInformation("Refresh token berhasil untuk user: {Username}", user.UserName);

                return new AuthResponseDto
                {
                    Username = user.UserName,
                    Role = new List<int> { 6 },
                    AccessToken = newAccessToken,
                    RefreshToken = newRefreshToken
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saat refresh token");
                throw;
            }
        }

        public async Task RevokeTokenAsync(string refreshToken)
        {
            try
            {
                _logger.LogInformation("Merevoke refresh token");
                await _authRepository.RevokeRefreshTokenAsync(refreshToken);
                _logger.LogInformation("Refresh token berhasil direvoke");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saat merevoke token");
                throw;
            }
        }

        private string GenerateAccessToken(UserModel user)
        {
            _logger.LogInformation("Generating access token untuk user ID: {UserId}", user.UserID);

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.UserID.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim(JwtRegisteredClaimNames.UniqueName, user.UserName),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim("userId", user.UserID.ToString()),
                new Claim("username", user.UserName)
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SecretKey));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _jwtSettings.Issuer,
                audience: _jwtSettings.Audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(_jwtSettings.AccessTokenExpirationMinutes),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private string GenerateRefreshToken()
        {
            _logger.LogInformation("Generating refresh token");
            var randomNumber = new byte[64];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }

        private async Task SaveRefreshTokenAsync(int userId, string refreshToken)
        {
            _logger.LogInformation("Menyimpan refresh token untuk user ID: {UserId}", userId);

            var token = new RefreshToken
            {
                UserId = userId,
                Token = refreshToken,
                ExpiresAt = DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpirationDays),
                CreatedAt = DateTime.UtcNow
            };

            await _authRepository.SaveRefreshTokenAsync(token);
        }
    }
}