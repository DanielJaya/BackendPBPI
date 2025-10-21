using BackendPBPI.DTO.RoleDTO;
using BackendPBPI.Interface.IService.Role;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using static BackendPBPI.DTO.AuthDTO.AuthDTO;

namespace BackendPBPI.Controllers.Role
{

        [ApiController]
        [Route("api/v1/[controller]")]
        [Authorize] // Semua endpoint butuh authentication
        public class RoleController : ControllerBase
        {
            private readonly IRoleService _roleService;
            private readonly ILogger<RoleController> _logger;

            public RoleController(IRoleService roleService, ILogger<RoleController> logger)
            {
                _roleService = roleService;
                _logger = logger;
            }

            // ============================================
            // Role CRUD Endpoints
            // ============================================

            /// <summary>
            /// Create role baru
            /// </summary>
            [HttpPost]
            public async Task<IActionResult> CreateRole([FromBody] CreateRoleRequestDto request)
            {
                try
                {
                    _logger.LogInformation("Request create role: {RoleName}", request.RoleName);

                    if (!ModelState.IsValid)
                    {
                        _logger.LogWarning("Validasi gagal untuk create role request");
                        return BadRequest(new ApiResponse<object>("Validasi gagal"));
                    }

                    var result = await _roleService.CreateRoleAsync(request);

                    _logger.LogInformation("Role berhasil dibuat: {RoleID}", result.RoleID);

                    return CreatedAtAction(
                        nameof(GetRoleById),
                        new { id = result.RoleID },
                        new ApiResponse<RoleResponseDto>("Role berhasil dibuat", result)
                    );
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error pada endpoint create role");
                    return BadRequest(new ApiResponse<object>(ex.Message));
                }
            }

            /// <summary>
            /// Get role by ID
            /// </summary>
            [HttpGet("{id}")]
            public async Task<IActionResult> GetRoleById(int id)
            {
                try
                {
                    _logger.LogInformation("Request get role: {RoleID}", id);

                    var result = await _roleService.GetRoleByIdAsync(id);

                    return Ok(new ApiResponse<RoleResponseDto>("Role ditemukan", result));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error pada endpoint get role by id");
                    return NotFound(new ApiResponse<object>(ex.Message));
                }
            }

            /// <summary>
            /// Get semua roles
            /// </summary>
            [HttpGet]
            public async Task<IActionResult> GetAllRoles()
            {
                try
                {
                    _logger.LogInformation("Request get all roles");

                    var result = await _roleService.GetAllRolesAsync();

                    return Ok(new ApiResponse<List<RoleResponseDto>>($"Ditemukan {result.Count} role", result));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error pada endpoint get all roles");
                    return BadRequest(new ApiResponse<object>(ex.Message));
                }
            }

            /// <summary>
            /// Update role
            /// </summary>
            [HttpPut("{id}")]
            public async Task<IActionResult> UpdateRole(int id, [FromBody] UpdateRoleRequestDto request)
            {
                try
                {
                    _logger.LogInformation("Request update role: {RoleID}", id);

                    if (!ModelState.IsValid)
                    {
                        _logger.LogWarning("Validasi gagal untuk update role request");
                        return BadRequest(new ApiResponse<object>("Validasi gagal"));
                    }

                    var result = await _roleService.UpdateRoleAsync(id, request);

                    _logger.LogInformation("Role berhasil diupdate: {RoleID}", id);

                    return Ok(new ApiResponse<RoleResponseDto>("Role berhasil diupdate", result));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error pada endpoint update role");
                    return BadRequest(new ApiResponse<object>(ex.Message));
                }
            }

            /// <summary>
            /// Delete role (soft delete)
            /// </summary>
            [HttpDelete("{id}")]
            public async Task<IActionResult> DeleteRole(int id)
            {
                try
                {
                    _logger.LogInformation("Request delete role: {RoleID}", id);

                    var result = await _roleService.DeleteRoleAsync(id);

                    _logger.LogInformation("Role berhasil dihapus: {RoleID}", id);

                    return Ok(new ApiResponse<bool>("Role berhasil dihapus", result));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error pada endpoint delete role");
                    return BadRequest(new ApiResponse<object>(ex.Message));
                }
            }

            // ============================================
            // Role-User Assignment Endpoints
            // ============================================

            /// <summary>
            /// Assign role ke user
            /// </summary>
            [HttpPost("assign")]
            public async Task<IActionResult> AssignRoleToUser([FromBody] AssignRoleRequestDto request)
            {
                try
                {
                    _logger.LogInformation("Request assign role {RoleID} ke user {UserID}",
                        request.RoleID, request.UserID);

                    if (!ModelState.IsValid)
                    {
                        _logger.LogWarning("Validasi gagal untuk assign role request");
                        return BadRequest(new ApiResponse<object>("Validasi gagal"));
                    }

                    var result = await _roleService.AssignRoleToUserAsync(request);

                    _logger.LogInformation("Role berhasil di-assign ke user");

                    return Ok(new ApiResponse<bool>("Role berhasil di-assign ke user", result));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error pada endpoint assign role");
                    return BadRequest(new ApiResponse<object>(ex.Message));
                }
            }

            /// <summary>
            /// Remove role dari user
            /// </summary>
            [HttpDelete("remove/{userId}/{roleId}")]
            public async Task<IActionResult> RemoveRoleFromUser(int userId, int roleId)
            {
                try
                {
                    _logger.LogInformation("Request remove role {RoleID} dari user {UserID}",
                        roleId, userId);

                    var result = await _roleService.RemoveRoleFromUserAsync(userId, roleId);

                    _logger.LogInformation("Role berhasil dihapus dari user");

                    return Ok(new ApiResponse<bool>("Role berhasil dihapus dari user", result));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error pada endpoint remove role");
                    return BadRequest(new ApiResponse<object>(ex.Message));
                }
            }

            /// <summary>
            /// Get user dengan semua roles-nya
            /// </summary>
            [HttpGet("user/{userId}")]
            public async Task<IActionResult> GetUserWithRoles(int userId)
            {
                try
                {
                    _logger.LogInformation("Request get user dengan roles: {UserID}", userId);

                    var result = await _roleService.GetUserWithRolesAsync(userId);

                    return Ok(new ApiResponse<UserRolesResponseDto>("Data user dengan roles", result));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error pada endpoint get user with roles");
                    return NotFound(new ApiResponse<object>(ex.Message));
                }
            }

            /// <summary>
            /// Get role dengan semua users yang memilikinya
            /// </summary>
            [HttpGet("users/{roleId}")]
            public async Task<IActionResult> GetRoleWithUsers(int roleId)
            {
                try
                {
                    _logger.LogInformation("Request get role dengan users: {RoleID}", roleId);

                    var result = await _roleService.GetRoleWithUsersAsync(roleId);

                    return Ok(new ApiResponse<RoleUsersResponseDto>("Data role dengan users", result));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error pada endpoint get role with users");
                    return NotFound(new ApiResponse<object>(ex.Message));
                }
            }

            /// <summary>
            /// Check apakah user memiliki role tertentu
            /// </summary>
            [HttpGet("check/{userId}/{roleId}")]
            public async Task<IActionResult> CheckUserHasRole(int userId, int roleId)
            {
                try
                {
                    _logger.LogInformation("Request check user {UserID} has role {RoleID}", userId, roleId);

                    var result = await _roleService.UserHasRoleAsync(userId, roleId);

                    return Ok(new ApiResponse<bool>(
                        result ? "User memiliki role ini" : "User tidak memiliki role ini",
                        result
                    ));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error pada endpoint check user has role");
                    return BadRequest(new ApiResponse<object>(ex.Message));
                }
            }
        }
    }