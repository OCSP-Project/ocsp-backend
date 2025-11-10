using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OCSP.Application.DTOs.ModelAnalysis;
using OCSP.Application.Services;
using System.Security.Claims;

namespace OCSP.API.Controllers
{
    [ApiController]
    [Route("api/model-analysis")]
    [Authorize]
    public class ModelAnalysisController : ControllerBase
    {
        private readonly IModelAnalysisService _service;
        private readonly ILogger<ModelAnalysisController> _logger;

        public ModelAnalysisController(
            IModelAnalysisService service,
            ILogger<ModelAnalysisController> logger)
        {
            _service = service;
            _logger = logger;
        }

        /// <summary>
        /// Upload GLB file for 3D model
        /// </summary>
        /// <remarks>
        /// ⚠️ CHỈ SUPERVISOR MỚI CÓ QUYỀN UPLOAD
        /// </remarks>
        [HttpPost("upload")]
        [Authorize(Roles = "Supervisor")]
        [Consumes("multipart/form-data")]
        [RequestSizeLimit(52428800)]
        [ProducesResponseType(typeof(UploadGLBResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<UploadGLBResponse>> UploadGLB(
            [FromForm] UploadGLBRequest request,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (userId == Guid.Empty) return Unauthorized(new { message = "User not authenticated" });

                var result = await _service.UploadGLBAsync(request, userId, cancellationToken);
                if (!result.IsValid)
                {
                    return BadRequest(new { message = result.ValidationMessage, fileSizeMB = result.FileSizeMB });
                }
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception)
            {
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        /// <summary>
        /// Get latest 3D model info by project ID
        /// </summary>
        /// <remarks>
        /// ✅ TẤT CẢ USERS ĐỀU CÓ THỂ XEM (Supervisor, Contractor, Homeowner)
        /// </remarks>
        [HttpGet("projects/{projectId:guid}/model")]
        [ProducesResponseType(typeof(Project3DModelDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<Project3DModelDto>> GetProjectModel(
            [FromRoute] Guid projectId,
            CancellationToken cancellationToken = default)
        {
            var model = await _service.GetProjectModelAsync(projectId, cancellationToken);
            if (model == null) return NotFound(new { message = "3D model not found for this project" });
            return Ok(model);
        }

        /// <summary>
        /// List all 3D models of a project (latest first)
        /// </summary>
        [HttpGet("projects/{projectId:guid}/models")]
        [ProducesResponseType(typeof(List<Project3DModelDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<List<Project3DModelDto>>> ListProjectModels(
            [FromRoute] Guid projectId,
            CancellationToken cancellationToken = default)
        {
            var models = await _service.ListProjectModelsAsync(projectId, cancellationToken);
            return Ok(models);
        }

        /// <summary>
        /// Get model detail by model ID
        /// </summary>
        [HttpGet("models/{modelId:guid}")]
        [ProducesResponseType(typeof(Project3DModelDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<Project3DModelDto>> GetModelById(
            [FromRoute] Guid modelId,
            CancellationToken cancellationToken = default)
        {
            var model = await _service.GetModelByIdAsync(modelId, cancellationToken);
            if (model == null) return NotFound();
            return Ok(model);
        }

        private Guid GetCurrentUserId()
        {
            var id = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
            return Guid.TryParse(id, out var userId) ? userId : Guid.Empty;
        }
    }
}
