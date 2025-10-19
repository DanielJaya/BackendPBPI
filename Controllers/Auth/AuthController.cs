using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using BackendPBPI.Interface.IService.Auth;
using static BackendPBPI.DTO.AuthDTO.AuthDTO;

namespace BackendPBPI.Controllers
{
    [ApiController]
    [Route("api/v1")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(IAuthService authService, ILogger<AuthController> logger)
        {
            _authService = authService;
            _logger = logger;
        }

        /// <summary>
        /// Register user baru
        /// </summary>
        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<IActionResult> Register([FromBody] RegisterRequestDto request)
        {
            try
            {
                _logger.LogInformation("Request register diterima untuk username: {Username}", request.UserName);

                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("Validasi gagal untuk register request");
                    return BadRequest(new ApiResponse<object>("Validasi gagal"));
                }

                var result = await _authService.RegisterAsync(request);

                _logger.LogInformation("Register berhasil untuk username: {Username}", request.UserName);

                return Ok(new ApiResponse<AuthResponseDto>("Registrasi berhasil", result));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error pada endpoint register");
                return BadRequest(new ApiResponse<object>(ex.Message));
            }
        }

        /// <summary>
        /// Login user
        /// </summary>
        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
        {
            try
            {
                _logger.LogInformation("Request login diterima untuk: {UsernameOrEmail}", request.UsernameOrEmail);

                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("Validasi gagal untuk login request");
                    return BadRequest(new ApiResponse<object>("Validasi gagal"));
                }

                var result = await _authService.LoginAsync(request);

                _logger.LogInformation("Login berhasil untuk: {Username}", result.Username);

                return Ok(new ApiResponse<AuthResponseDto>("Login berhasil", result));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error pada endpoint login");
                return BadRequest(new ApiResponse<object>(ex.Message));
            }
        }

        /// <summary>
        /// Refresh access token menggunakan refresh token
        /// </summary>
        [HttpPost("refresh-token")]
        [AllowAnonymous]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequestDto request)
        {
            try
            {
                _logger.LogInformation("Request refresh token diterima");

                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("Validasi gagal untuk refresh token request");
                    return BadRequest(new ApiResponse<object>("Validasi gagal"));
                }

                var result = await _authService.RefreshTokenAsync(request.RefreshToken);

                _logger.LogInformation("Refresh token berhasil untuk user: {Username}", result.Username);

                return Ok(new ApiResponse<AuthResponseDto>("Token berhasil di-refresh", result));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error pada endpoint refresh token");
                return BadRequest(new ApiResponse<object>(ex.Message));
            }
        }

        /// <summary>
        /// Revoke refresh token (logout)
        /// </summary>
        [HttpPost("revoke-token")]
        [Authorize]
        public async Task<IActionResult> RevokeToken([FromBody] RefreshTokenRequestDto request)
        {
            try
            {
                _logger.LogInformation("Request revoke token diterima");

                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("Validasi gagal untuk revoke token request");
                    return BadRequest(new ApiResponse<object>("Validasi gagal"));
                }

                await _authService.RevokeTokenAsync(request.RefreshToken);

                _logger.LogInformation("Token berhasil direvoke");

                return Ok(new ApiResponse<object>("Logout berhasil", null));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error pada endpoint revoke token");
                return BadRequest(new ApiResponse<object>(ex.Message));
            }
        }

        /// <summary>
        /// Test endpoint untuk memastikan authentication bekerja
        /// </summary>
        [HttpGet("test-auth")]
        [Authorize]
        public IActionResult TestAuth()
        {
            var userId = User.FindFirst("userId")?.Value;
            var username = User.FindFirst("username")?.Value;

            _logger.LogInformation("Test auth berhasil untuk user: {Username}", username);

            return Ok(new ApiResponse<object>("Authentication berhasil", new
            {
                userId,
                username,
                claims = User.Claims.Select(c => new { c.Type, c.Value })
            }));
        }
    }
}