using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OCSP.Application.DTOs.Profile;
using OCSP.Application.Services.Interfaces;
using System.Security.Claims;

namespace OCSP.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ProfileController : ControllerBase
    {
        private readonly IProfileService _profileService;

        public ProfileController(IProfileService profileService)
        {
            _profileService = profileService;
        }

        // View Profile - Tất cả roles có thể xem profile của mình
        [HttpGet]
        public async Task<ActionResult<ProfileDto>> GetProfile()
        {
            try
            {
                var userId = GetCurrentUserId();
                var profile = await _profileService.GetProfileAsync(userId);
                return Ok(profile);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        //Edit Profile - Tất cả roles có thể chỉnh sửa profile của mình
        [HttpPut]
        public async Task<ActionResult<ProfileDto>> UpdateProfile([FromBody] UpdateProfileDto updateDto)
        {
            try
            {
                var userId = GetCurrentUserId();
                var updatedProfile = await _profileService.UpdateProfileAsync(userId, updateDto);
                return Ok(updatedProfile);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // Upload Profile Documents - Chỉ Contractor và Supervisor
        [HttpPost("documents")]
        public async Task<ActionResult<ProfileDocumentDto>> UploadDocument([FromForm] IFormFile file, [FromForm] UploadProfileDocumentDto documentDto)
        {
            try
            {
                var userId = GetCurrentUserId();
                
                // Chuyển đổi IFormFile thành Stream và thông tin cần thiết
                using var fileStream = file.OpenReadStream();
                var document = await _profileService.UploadProfileDocumentAsync(
                    userId, 
                    fileStream, 
                    file.FileName, 
                    file.Length, 
                    documentDto);
                    
                return Ok(document);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // Lấy danh sách documents của user
        [HttpGet("documents")]
        public async Task<ActionResult<IEnumerable<ProfileDocumentDto>>> GetUserDocuments()
        {
            try
            {
                var userId = GetCurrentUserId();
                var documents = await _profileService.GetUserDocumentsAsync(userId);
                return Ok(documents);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // Xóa document
        [HttpDelete("documents/{documentId}")]
        public async Task<ActionResult> DeleteDocument(Guid documentId)
        {
            try
            {
                var userId = GetCurrentUserId();
                await _profileService.DeleteProfileDocumentAsync(userId, documentId);
                return Ok(new { message = "Tài liệu đã được xóa thành công" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // Helper method để lấy UserId từ JWT token
        private Guid GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            {
                throw new UnauthorizedAccessException("Không thể xác định người dùng");
            }
            return userId;
        }
    }
}
