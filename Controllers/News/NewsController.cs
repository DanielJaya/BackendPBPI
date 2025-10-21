using BackendPBPI.DTO.NewsDTO;
using BackendPBPI.Helper;
using BackendPBPI.Interface.IService.News;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

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

        /// <summary>
        /// Create News dengan Image (Admin Only - Wajib Upload Gambar)
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "1")] // Hanya Admin (RoleID = 1)
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> CreateNews([FromForm] IFormFile newsPic, [FromForm] string newsData)
        {
            try
            {
                // Validasi gambar wajib diupload
                if (newsPic == null || newsPic.Length == 0)
                {
                    return BadRequest(new { success = false, message = "Gambar wajib diupload saat membuat news" });
                }

                // Validasi gambar
                var (isValid, errorMessage) = ImageHelper.ValidateImage(newsPic);
                if (!isValid)
                {
                    return BadRequest(new { success = false, message = errorMessage });
                }

                // Parse JSON data dari form
                var request = JsonSerializer.Deserialize<CreateNewsRequestDto>(newsData, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (request == null)
                {
                    return BadRequest(new { success = false, message = "Data news tidak valid" });
                }

                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var userId = GetUserIdFromToken();
                _logger.LogInformation("Admin {UserId} membuat news baru", userId);

                // Convert ke byte array
                var imageBytes = await ImageHelper.ConvertToByteArrayAsync(newsPic);

                // Create news dengan gambar
                var result = await _newsService.CreateNewsAsync(userId, request, imageBytes, newsPic.FileName, newsPic.ContentType);

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
                var roles = User.FindAll(System.Security.Claims.ClaimTypes.Role).Select(c => c.Value).ToList();
                var isAdmin = roles.Contains("1"); // RoleID 1 = Admin
                var canSeeInactive = isAdmin && includeInactive;

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
        /// Update News (Admin Only - Support Update Gambar)
        /// Hanya update field yang berubah
        /// </summary>
        [HttpPut("{id}")]
        [Authorize(Roles = "1")] // Hanya Admin
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UpdateNews(int id, [FromForm] IFormFile? newsPic, [FromForm] string newsData)
        {
            try
            {
                // Parse JSON data dari form
                var request = JsonSerializer.Deserialize<UpdateNewsRequestDto>(newsData, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (request == null)
                {
                    return BadRequest(new { success = false, message = "Data news tidak valid" });
                }

                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                _logger.LogInformation("Admin mengupdate news ID: {NewsID}", id);

                byte[]? imageBytes = null;
                string? fileName = null;
                string? contentType = null;

                // Jika ada upload gambar baru
                if (newsPic != null && newsPic.Length > 0)
                {
                    // Validasi gambar
                    var (isValid, errorMessage) = ImageHelper.ValidateImage(newsPic);
                    if (!isValid)
                    {
                        return BadRequest(new { success = false, message = errorMessage });
                    }

                    // Convert ke byte array
                    imageBytes = await ImageHelper.ConvertToByteArrayAsync(newsPic);
                    fileName = newsPic.FileName;
                    contentType = newsPic.ContentType;

                    _logger.LogInformation("Gambar baru akan diupdate untuk news ID: {NewsID}", id);
                }

                // Update news (dengan atau tanpa gambar baru)
                var result = await _newsService.UpdateNewsAsync(id, request, imageBytes, fileName, contentType);

                return Ok(new
                {
                    success = true,
                    message = "News berhasil diupdate",
                    data = result
                });
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
        /// Delete News (Admin Only)
        /// Admin bisa delete semua news tanpa cek ownership
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize(Roles = "1")] // Hanya Admin
        public async Task<IActionResult> DeleteNews(int id)
        {
            try
            {
                _logger.LogInformation("Admin menghapus news ID: {NewsID}", id);

                var result = await _newsService.DeleteNewsAsync(id);

                return Ok(new
                {
                    success = true,
                    message = "News berhasil dihapus"
                });
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