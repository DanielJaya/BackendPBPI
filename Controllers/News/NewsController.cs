using BackendPBPI.DTO.NewsDTO;
using BackendPBPI.Interface.IService.News;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BackendPBPI.Controllers.News
{
    [ApiController]
    [Route("api/v1/[controller]")]
    [Authorize] // Semua endpoint butuh authentication
    public class NewsController : ControllerBase
    {
        private readonly INewsService _newsService;
        private readonly ILogger<NewsController> _logger;

        public NewsController(INewsService newsService, ILogger<NewsController> logger)
        {
            _newsService = newsService;
            _logger = logger;
        }

        // Helper method untuk get user ID dari JWT token
        private int GetUserIdFromToken()
        {
            var userIdClaim = User.FindFirst("userId")?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                throw new UnauthorizedAccessException("User ID tidak ditemukan dalam token");
            }
            return userId;
        }

        // Helper method untuk cek apakah user adalah admin
        private bool IsAdmin()
        {
            var roles = User.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();
            return roles.Contains("1"); // RoleID 1 = Admin
        }

        /// <summary>
        /// Create News (Admin Only)
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "1")] // Hanya Admin (RoleID = 1)
        public async Task<IActionResult> CreateNews([FromBody] CreateNewsRequestDto request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var userId = GetUserIdFromToken();
                _logger.LogInformation("Admin {UserId} membuat news baru", userId);

                var result = await _newsService.CreateNewsAsync(userId, request);

                return CreatedAtAction(nameof(GetNewsById), new { id = result.NewsID }, new
                {
                    success = true,
                    message = "News berhasil dibuat",
                    data = result
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saat create news");
                return StatusCode(500, new
                {
                    success = false,
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Get News By ID (All authenticated users)
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetNewsById(int id)
        {
            try
            {
                var result = await _newsService.GetNewsByIdAsync(id);

                return Ok(new
                {
                    success = true,
                    message = "News berhasil diambil",
                    data = result
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saat get news by ID: {NewsID}", id);
                return NotFound(new
                {
                    success = false,
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Get All News (All authenticated users)
        /// Admin bisa lihat semua termasuk yang inactive
        /// User biasa hanya bisa lihat yang active
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAllNews([FromQuery] bool includeInactive = false)
        {
            try
            {
                // Hanya admin yang bisa lihat inactive news
                var canSeeInactive = IsAdmin() && includeInactive;

                var result = await _newsService.GetAllNewsAsync(canSeeInactive);

                return Ok(new
                {
                    success = true,
                    message = "News berhasil diambil",
                    data = result
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saat get all news");
                return StatusCode(500, new
                {
                    success = false,
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Get My News (Admin Only)
        /// </summary>
        [HttpGet("my-news")]
        [Authorize(Roles = "1")] // Hanya Admin
        public async Task<IActionResult> GetMyNews()
        {
            try
            {
                var userId = GetUserIdFromToken();
                var result = await _newsService.GetMyNewsAsync(userId);

                return Ok(new
                {
                    success = true,
                    message = "News berhasil diambil",
                    data = result
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saat get my news");
                return StatusCode(500, new
                {
                    success = false,
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Update News (Admin Only - Owner)
        /// </summary>
        [HttpPut("{id}")]
        [Authorize(Roles = "1")] // Hanya Admin
        public async Task<IActionResult> UpdateNews(int id, [FromBody] UpdateNewsRequestDto request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var userId = GetUserIdFromToken();
                var result = await _newsService.UpdateNewsAsync(id, userId, request);

                return Ok(new
                {
                    success = true,
                    message = "News berhasil diupdate",
                    data = result
                });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saat update news: {NewsID}", id);
                return StatusCode(500, new
                {
                    success = false,
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Delete News (Admin Only - Owner)
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize(Roles = "1")] // Hanya Admin
        public async Task<IActionResult> DeleteNews(int id)
        {
            try
            {
                var userId = GetUserIdFromToken();
                var result = await _newsService.DeleteNewsAsync(id, userId);

                return Ok(new
                {
                    success = true,
                    message = "News berhasil dihapus"
                });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saat delete news: {NewsID}", id);
                return StatusCode(500, new
                {
                    success = false,
                    message = ex.Message
                });
            }
        }
    }
}