using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OCSP.Application.DTOs.Admin;
using OCSP.Application.Services.Interfaces;
using System.Security.Claims;

namespace OCSP.API.Controllers
{
    [ApiController]
    [Route("api/admin")]
    [Authorize] // Chỉ cho phép user đã đăng nhập
    public class AdminController : ControllerBase
    {
        private readonly IAdminService _adminService;
        private readonly ILogger<AdminController> _logger;

        public AdminController(IAdminService adminService, ILogger<AdminController> logger)
        {
            _adminService = adminService;
            _logger = logger;
        }

        // POST api/admin/users
        [HttpPost("users")]
        [ProducesResponseType(typeof(OCSP.Application.DTOs.Auth.UserResponseDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<OCSP.Application.DTOs.Auth.UserResponseDto>> CreateUser([FromBody] CreateUserDto createUserDto)
        {
            try
            {
                // Check if current user is Admin
                if (!IsAdmin())
                {
                    return Forbid("Chỉ admin mới có quyền tạo người dùng");
                }

                var user = await _adminService.CreateUserAsync(createUserDto);
                return CreatedAtAction(nameof(GetUserById), new { userId = user.Id }, user);
            }
            catch (OCSP.Application.Common.Exceptions.ValidationException ex)
            {
                _logger.LogWarning(ex, "Validation error creating user");
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled error creating user");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        // GET api/admin/users
        [HttpGet("users")]
        [ProducesResponseType(typeof(List<OCSP.Application.DTOs.Auth.UserResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<List<OCSP.Application.DTOs.Admin.AdminUserDto>>> GetAllUsers([FromQuery] bool includeProjects = true)
        {
            try
            {
                // Check if current user is Admin
                if (!IsAdmin())
                {
                    return Forbid("Chỉ admin mới có quyền xem danh sách người dùng");
                }

                if (includeProjects)
                {
                    var users = await _adminService.GetAllUsersWithProjectsAsync();
                    return Ok(users);
                }
                else
                {
                    var users = await _adminService.GetAllUsersAsync();
                    return Ok(users);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled error getting users");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        // GET api/admin/users/{userId}
        [HttpGet("users/{userId:guid}")]
        [ProducesResponseType(typeof(OCSP.Application.DTOs.Auth.UserResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<OCSP.Application.DTOs.Admin.AdminUserDto>> GetUserById([FromRoute] Guid userId, [FromQuery] bool includeProjects = true)
        {
            try
            {
                // Check if current user is Admin
                if (!IsAdmin())
                {
                    return Forbid("Chỉ admin mới có quyền xem thông tin người dùng");
                }

                if (includeProjects)
                {
                    var user = await _adminService.GetUserByIdWithProjectsAsync(userId);
                    if (user == null)
                        return NotFound(new { message = "Người dùng không tồn tại" });
                    return Ok(user);
                }
                else
                {
                    var user = await _adminService.GetUserByIdAsync(userId);
                    if (user == null)
                        return NotFound(new { message = "Người dùng không tồn tại" });
                    return Ok(user);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled error getting user");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        // DELETE api/admin/users/{userId}
        [HttpDelete("users/{userId:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteUser([FromRoute] Guid userId)
        {
            try
            {
                // Check if current user is Admin
                if (!IsAdmin())
                {
                    return Forbid("Chỉ admin mới có quyền xóa người dùng");
                }

                await _adminService.DeleteUserAsync(userId);
                return Ok(new { message = "Người dùng đã được xóa thành công" });
            }
            catch (OCSP.Application.Common.Exceptions.ValidationException ex)
            {
                _logger.LogWarning(ex, "Validation error deleting user");
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled error deleting user");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        // POST api/admin/users/{userId}/ban
        [HttpPost("users/{userId:guid}/ban")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> BanUser([FromRoute] Guid userId)
        {
            try
            {
                // Check if current user is Admin
                if (!IsAdmin())
                {
                    return Forbid("Chỉ admin mới có quyền ban người dùng");
                }

                await _adminService.BanUserAsync(userId);
                return Ok(new { message = "Người dùng đã bị ban thành công" });
            }
            catch (OCSP.Application.Common.Exceptions.ValidationException ex)
            {
                _logger.LogWarning(ex, "Validation error banning user");
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled error banning user");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        // POST api/admin/users/{userId}/unban
        [HttpPost("users/{userId:guid}/unban")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UnbanUser([FromRoute] Guid userId)
        {
            try
            {
                // Check if current user is Admin
                if (!IsAdmin())
                {
                    return Forbid("Chỉ admin mới có quyền unban người dùng");
                }

                await _adminService.UnbanUserAsync(userId);
                return Ok(new { message = "Người dùng đã được unban thành công" });
            }
            catch (OCSP.Application.Common.Exceptions.ValidationException ex)
            {
                _logger.LogWarning(ex, "Validation error unbanning user");
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled error unbanning user");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        // GET api/admin/dashboard/stats
        [HttpGet("dashboard/stats")]
        [ProducesResponseType(typeof(OCSP.Application.DTOs.Admin.AdminDashboardStatsDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<OCSP.Application.DTOs.Admin.AdminDashboardStatsDto>> GetDashboardStats()
        {
            try
            {
                // Check if current user is Admin
                if (!IsAdmin())
                {
                    return Forbid("Chỉ admin mới có quyền xem thống kê dashboard");
                }

                var stats = await _adminService.GetDashboardStatsAsync();
                return Ok(stats);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled error getting dashboard stats");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        // GET api/admin/dashboard/recent-projects
        [HttpGet("dashboard/recent-projects")]
        [ProducesResponseType(typeof(List<OCSP.Application.DTOs.Admin.RecentProjectDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<List<OCSP.Application.DTOs.Admin.RecentProjectDto>>> GetRecentProjects([FromQuery] int limit = 10)
        {
            try
            {
                // Check if current user is Admin
                if (!IsAdmin())
                {
                    return Forbid("Chỉ admin mới có quyền xem dự án gần đây");
                }

                var projects = await _adminService.GetRecentProjectsAsync(limit);
                return Ok(projects);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled error getting recent projects");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        // GET api/admin/dashboard/recent-users
        [HttpGet("dashboard/recent-users")]
        [ProducesResponseType(typeof(List<OCSP.Application.DTOs.Admin.RecentUserDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<List<OCSP.Application.DTOs.Admin.RecentUserDto>>> GetRecentUsers([FromQuery] int limit = 10)
        {
            try
            {
                // Check if current user is Admin
                if (!IsAdmin())
                {
                    return Forbid("Chỉ admin mới có quyền xem người dùng gần đây");
                }

                var users = await _adminService.GetRecentUsersAsync(limit);
                return Ok(users);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled error getting recent users");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        // GET api/admin/projects
        [HttpGet("projects")]
        [ProducesResponseType(typeof(List<OCSP.Application.DTOs.Admin.AdminProjectListDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<List<OCSP.Application.DTOs.Admin.AdminProjectListDto>>> GetAllProjects(
            [FromQuery] string? search = null,
            [FromQuery] string? status = null,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            try
            {
                // Check if current user is Admin
                if (!IsAdmin())
                {
                    return Forbid("Chỉ admin mới có quyền xem danh sách dự án");
                }

                var projects = await _adminService.GetAllProjectsAsync(search, status, page, pageSize);
                return Ok(projects);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled error getting projects");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        // GET api/admin/reports/financial
        [HttpGet("reports/financial")]
        [ProducesResponseType(typeof(OCSP.Application.DTOs.Admin.FinancialReportDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<OCSP.Application.DTOs.Admin.FinancialReportDto>> GetFinancialReport()
        {
            try
            {
                // Check if current user is Admin
                if (!IsAdmin())
                {
                    return Forbid("Chỉ admin mới có quyền xem báo cáo tài chính");
                }

                var report = await _adminService.GetFinancialReportAsync();
                return Ok(report);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled error getting financial report");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        private bool IsAdmin()
        {
            var roleClaim = User.FindFirstValue(ClaimTypes.Role) 
                         ?? User.FindFirstValue("role");
            
            // Admin role = 0
            return roleClaim == "Admin" || roleClaim == "0";
        }

        private Guid GetCurrentUserId()
        {
            var id = User.FindFirstValue(ClaimTypes.NameIdentifier)
                     ?? User.FindFirstValue("sub");
            return Guid.TryParse(id, out var g) ? g : Guid.Empty;
        }
    }
}