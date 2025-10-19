using System.ComponentModel.DataAnnotations;

namespace BackendPBPI.DTO.AuthDTO
{
    public class AuthDTO
    {
            // Register Request DTO
    public class RegisterRequestDto
        {
            [Required(ErrorMessage = "Username wajib diisi")]
            [StringLength(50, MinimumLength = 3, ErrorMessage = "Username harus antara 3-50 karakter")]
            public string UserName { get; set; }

            [Required(ErrorMessage = "Email wajib diisi")]
            [EmailAddress(ErrorMessage = "Format email tidak valid")]
            public string Email { get; set; }

            [Required(ErrorMessage = "Password wajib diisi")]
            [StringLength(100, MinimumLength = 6, ErrorMessage = "Password minimal 6 karakter")]
            public string Password { get; set; }
        }

        // Login Request DTO
        public class LoginRequestDto
        {
            [Required(ErrorMessage = "Username atau Email wajib diisi")]
            public string UsernameOrEmail { get; set; }

            [Required(ErrorMessage = "Password wajib diisi")]
            public string Password { get; set; }
        }

        // Refresh Token Request DTO
        public class RefreshTokenRequestDto
        {
            [Required]
            public string RefreshToken { get; set; }
        }

        // Auth Response DTO
        public class AuthResponseDto
        {
            public string Username { get; set; }
            public List<int> Role { get; set; }
            public string AccessToken { get; set; }
            public string RefreshToken { get; set; }
        }

        // Generic API Response
        public class ApiResponse<T>
        {
            public string Message { get; set; }
            public T Data { get; set; }
            public bool Success { get; set; } = true;

            public ApiResponse(string message, T data)
            {
                Message = message;
                Data = data;
            }

            public ApiResponse(string message)
            {
                Message = message;
                Success = false;
            }
        }
    }
}
