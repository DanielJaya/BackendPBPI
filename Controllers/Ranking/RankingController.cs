using BackendPBPI.DTO.RankingDTO;
using BackendPBPI.Helper;
using BackendPBPI.Interface.IService.Ranking;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BackendPBPI.Controllers.Ranking
{
    [ApiController]
    [Route("api/v1/[controller]")]
    [Authorize] // Require JWT authentication
    public class RankingController : ControllerBase
    {
        private readonly IRankingService _rankingService;
        private readonly ILogger<RankingController> _logger;

        public RankingController(IRankingService rankingService, ILogger<RankingController> logger)
        {
            _rankingService = rankingService;
            _logger = logger;
        }

        // ============ PLAYER ENDPOINTS ============

        /// <summary>
        /// Add new player (Admin only)
        /// </summary>
        [HttpPost("player")]
        [Authorize(Roles = "1")]
        public async Task<IActionResult> AddPlayer([FromForm] AddPlayerRequestDTO request)
        {
            try
            {
                // Validate admin
                if (!JwtHelper.IsAdmin(User))
                {
                    return StatusCode(403, new { message = "Hanya admin yang dapat menambahkan player" });
                }

                var userId = JwtHelper.GetUserId(User);
                var result = await _rankingService.AddPlayerAsync(request, userId);

                return CreatedAtAction(nameof(GetPlayerDetail), new { id = result.RankingID }, new
                {
                    success = true,
                    message = result.Message,
                    data = result
                });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error pada AddPlayer endpoint");
                return StatusCode(500, new { success = false, message = "Terjadi kesalahan pada server" });
            }
        }

        /// <summary>
        /// Get list of players with pagination and search
        /// </summary>
        [HttpGet("players")]
        public async Task<IActionResult> GetPlayersList(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string searchQuery = "")
        {
            try
            {
                if (pageNumber < 1 || pageSize < 1)
                {
                    return BadRequest(new { success = false, message = "Page number dan page size harus lebih dari 0" });
                }

                var result = await _rankingService.GetPlayersListAsync(pageNumber, pageSize, searchQuery);

                return Ok(new
                {
                    success = true,
                    message = "Data players berhasil diambil",
                    data = result
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error pada GetPlayersList endpoint");
                return StatusCode(500, new { success = false, message = "Terjadi kesalahan pada server" });
            }
        }

        /// <summary>
        /// Get player detail by ID
        /// </summary>
        [HttpGet("player/{id}")]
        public async Task<IActionResult> GetPlayerDetail(int id)
        {
            try
            {
                var result = await _rankingService.GetPlayerDetailAsync(id);

                return Ok(new
                {
                    success = true,
                    message = "Detail player berhasil diambil",
                    data = result
                });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error pada GetPlayerDetail endpoint untuk ID {PlayerId}", id);
                return StatusCode(500, new { success = false, message = "Terjadi kesalahan pada server" });
            }
        }

        /// <summary>
        /// Update player (Admin only)
        /// </summary>
        [HttpPut("player/{id}")]
        [Authorize(Roles = "1")]
        public async Task<IActionResult> UpdatePlayer(int id, [FromForm] UpdatePlayerRequestDTO request)
        {
            try
            {
                if (!JwtHelper.IsAdmin(User))
                {
                    return StatusCode(403, new { message = "Hanya admin yang dapat mengupdate player" });
                }

                var result = await _rankingService.UpdatePlayerAsync(id, request);

                return Ok(new
                {
                    success = true,
                    message = result.Message,
                    data = result
                });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { success = false, message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error pada UpdatePlayer endpoint untuk ID {PlayerId}", id);
                return StatusCode(500, new { success = false, message = "Terjadi kesalahan pada server" });
            }
        }

        /// <summary>
        /// Delete player - soft delete (Admin only)
        /// </summary>
        [HttpDelete("player/{id}")]
        [Authorize(Roles = "1")]
        public async Task<IActionResult> DeletePlayer(int id)
        {
            try
            {
                if (!JwtHelper.IsAdmin(User))
                {
                    return StatusCode(403, new { message = "Hanya admin yang dapat menghapus player" });
                }

                await _rankingService.DeletePlayerAsync(id);

                return Ok(new
                {
                    success = true,
                    message = "Player berhasil dihapus"
                });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error pada DeletePlayer endpoint untuk ID {PlayerId}", id);
                return StatusCode(500, new { success = false, message = "Terjadi kesalahan pada server" });
            }
        }

        // ============ MATCH HISTORY ENDPOINTS ============

        /// <summary>
        /// Add match history for a player (Admin only)
        /// </summary>
        [HttpPost("match-history")]
        [Authorize(Roles = "1")]
        public async Task<IActionResult> AddMatchHistory([FromBody] AddMatchHistoryRequestDTO request)
        {
            try
            {
                if (!JwtHelper.IsAdmin(User))
                {
                    return StatusCode(403, new { message = "Hanya admin yang dapat menambahkan match history" });
                }

                var userId = JwtHelper.GetUserId(User);
                var result = await _rankingService.AddMatchHistoryAsync(request, userId);

                return CreatedAtAction(nameof(GetPlayerDetail), new { id = result.RankingID }, new
                {
                    success = true,
                    message = result.Message,
                    data = result
                });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error pada AddMatchHistory endpoint");
                return StatusCode(500, new { success = false, message = "Terjadi kesalahan pada server" });
            }
        }

        /// <summary>
        /// Update match history (Admin only)
        /// </summary>
        [HttpPut("match-history/{id}")]
        [Authorize(Roles = "1")]
        public async Task<IActionResult> UpdateMatchHistory(int id, [FromBody] UpdateMatchHistoryRequestDTO request)
        {
            try
            {
                if (!JwtHelper.IsAdmin(User))
                {
                    return StatusCode(403, new { message = "Hanya admin yang dapat mengupdate match history" });
                }

                var result = await _rankingService.UpdateMatchHistoryAsync(id, request);

                return Ok(new
                {
                    success = true,
                    message = result.Message,
                    data = result
                });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error pada UpdateMatchHistory endpoint untuk ID {MatchId}", id);
                return StatusCode(500, new { success = false, message = "Terjadi kesalahan pada server" });
            }
        }

        /// <summary>
        /// Delete match history (Admin only)
        /// </summary>
        [HttpDelete("match-history/{id}")]
        [Authorize(Roles = "1")]
        public async Task<IActionResult> DeleteMatchHistory(int id)
        {
            try
            {
                if (!JwtHelper.IsAdmin(User))
                {
                    return StatusCode(403, new { message = "Hanya admin yang dapat menghapus match history" });
                }

                await _rankingService.DeleteMatchHistoryAsync(id);

                return Ok(new
                {
                    success = true,
                    message = "Match history berhasil dihapus dan points player telah disesuaikan"
                });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error pada DeleteMatchHistory endpoint untuk ID {MatchId}", id);
                return StatusCode(500, new { success = false, message = "Terjadi kesalahan pada server" });
            }
        }
    }
}