using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OCSP.Application.DTOs.Contractor;
using OCSP.Application.Services.Interfaces;
using System.Security.Claims;

namespace OCSP.API.Controllers
{

    [ApiController]
    [Route("api/[controller]")]
    public class ContractorController : ControllerBase
    {
        private readonly IContractorService _contractorService;
        private readonly ILogger<ContractorController> _logger;

        public ContractorController(IContractorService contractorService, ILogger<ContractorController> logger)
        {
            _contractorService = contractorService;
            _logger = logger;
        }

        /// <summary>
        /// UC-16: Search Contractors with natural language input and filters
        /// </summary>
        [HttpPost("search")]
        public async Task<ActionResult<ContractorListResponseDto>> SearchContractors([FromBody] ContractorSearchDto searchDto)
        {
            try
            {
                _logger.LogInformation("Searching contractors with query: {Query}", searchDto.Query);

                var result = await _contractorService.SearchContractorsAsync(searchDto);

                _logger.LogInformation("Found {Count} contractors", result.TotalCount);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching contractors");
                return StatusCode(500, new { Message = "An error occurred while searching contractors." });
            }
        }
        [HttpPost("bulk-create")]
        // [Authorize(Roles = "Admin")]
        public async Task<ActionResult<BulkContractorResponseDto>> BulkCreateContractors([FromBody] BulkContractorRequestDto request)
        {
            try
            {
                _logger.LogInformation("Creating {Count} contractors", request.Contractors.Count);

                var result = await _contractorService.BulkCreateContractorsAsync(request);

                _logger.LogInformation("Successfully created {Success} contractors, {Failed} failed",
                    result.SuccessfulCount, result.FailedCount);

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating bulk contractors");
                return StatusCode(500, new { Message = "An error occurred while creating contractors." });
            }
        }
        /// <summary>
        /// UC-17: Get all contractors with pagination
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<ContractorListResponseDto>> GetAllContractors(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            try
            {
                if (page < 1) page = 1;
                if (pageSize < 1 || pageSize > 50) pageSize = 10;

                _logger.LogInformation("Getting contractors list - Page: {Page}, PageSize: {PageSize}", page, pageSize);

                var result = await _contractorService.GetAllContractorsAsync(page, pageSize);

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting contractors list");
                return StatusCode(500, new { Message = "An error occurred while retrieving contractors." });
            }
        }

        /// <summary>
        /// UC-18: Get detailed contractor profile by ID
        /// </summary>
        [HttpGet("{contractorId}")]
        public async Task<ActionResult<ContractorProfileDto>> GetContractorProfile(Guid contractorId)
        {
            try
            {
                _logger.LogInformation("Getting contractor profile for ID: {ContractorId}", contractorId);

                var contractor = await _contractorService.GetContractorProfileAsync(contractorId);

                if (contractor == null)
                {
                    _logger.LogWarning("Contractor not found with ID: {ContractorId}", contractorId);
                    return NotFound(new { Message = "Contractor not found." });
                }

                return Ok(contractor);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting contractor profile for ID: {ContractorId}", contractorId);
                return StatusCode(500, new { Message = "An error occurred while retrieving contractor profile." });
            }
        }

        /// <summary>
        /// UC-22: Get AI-powered contractor recommendations based on project requirements
        /// </summary>
        [HttpPost("recommendations")]
        [Authorize(Roles = "Homeowner")]
        public async Task<ActionResult<List<ContractorRecommendationDto>>> GetAIRecommendations([FromBody] AIRecommendationRequestDto requestDto)
        {
            try
            {
                _logger.LogInformation("Getting AI recommendations for project: {Description}", requestDto.ProjectDescription);

                var recommendations = await _contractorService.GetAIRecommendationsAsync(requestDto);

                _logger.LogInformation("Generated {Count} recommendations", recommendations.Count);
                return Ok(recommendations);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting AI recommendations");
                return StatusCode(500, new { Message = "An error occurred while generating recommendations." });
            }
        }

        /// <summary>
        /// Get featured/premium contractors for homepage
        /// </summary>
        [HttpGet("featured")]
        public async Task<ActionResult<List<ContractorProfileSummaryDto>>> GetFeaturedContractors([FromQuery] int count = 6)
        {
            try
            {
                if (count < 1 || count > 20) count = 6;

                var searchDto = new ContractorSearchDto
                {
                    IsPremium = true,
                    IsVerified = true,
                    PageSize = count,
                    SortBy = Domain.Enums.SearchSortBy.Premium
                };

                var result = await _contractorService.SearchContractorsAsync(searchDto);
                return Ok(result.Contractors);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting featured contractors");
                return StatusCode(500, new { Message = "An error occurred while retrieving featured contractors." });
            }
        }

        /// <summary>
        /// Quick search endpoint for autocomplete/suggestions
        /// </summary>
        [HttpGet("search/suggestions")]
        public async Task<ActionResult<List<string>>> GetSearchSuggestions([FromQuery] string query)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(query) || query.Length < 2)
                    return Ok(new List<string>());

                var searchDto = new ContractorSearchDto
                {
                    Query = query,
                    PageSize = 10,
                    SortBy = Domain.Enums.SearchSortBy.Relevance
                };

                var result = await _contractorService.SearchContractorsAsync(searchDto);
                var suggestions = result.Contractors
                    .Select(c => c.CompanyName)
                    .Take(5)
                    .ToList();

                return Ok(suggestions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting search suggestions");
                return Ok(new List<string>());
            }
        }

        /// <summary>
        /// Validate message content for anti-circumvention (used by chat system)
        /// </summary>
        [HttpPost("validate-communication")]
        [Authorize]
        public async Task<ActionResult<CommunicationWarningDto>> ValidateCommunication([FromBody] ValidateCommunicationDto dto)
        {
            try
            {
                var fromUserId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "");

                var warning = await _contractorService.ValidateCommunicationAsync(dto.Content, fromUserId, dto.ToUserId);

                if (warning != null)
                {
                    // Log the communication with flag
                    await _contractorService.LogCommunicationAsync(
                        fromUserId,
                        dto.ToUserId,
                        dto.Content,
                        Domain.Enums.CommunicationType.Chat,
                        dto.ProjectId);

                    return Ok(warning);
                }

                // Log normal communication
                await _contractorService.LogCommunicationAsync(
                    fromUserId,
                    dto.ToUserId,
                    dto.Content,
                    Domain.Enums.CommunicationType.Chat,
                    dto.ProjectId);

                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating communication");
                return StatusCode(500, new { Message = "An error occurred while validating communication." });
            }
        }

        /// <summary>
        /// Get contractor statistics for analytics
        /// </summary>
        [HttpGet("statistics")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ContractorStatisticsDto>> GetContractorStatistics()
        {
            try
            {
                // This would be implemented in the service
                // For now, return a placeholder
                var stats = new ContractorStatisticsDto
                {
                    TotalContractors = 0,
                    VerifiedContractors = 0,
                    PremiumContractors = 0,
                    AverageRating = 0,
                    RestrictedContractors = 0
                };

                return Ok(stats);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting contractor statistics");
                return StatusCode(500, new { Message = "An error occurred while retrieving statistics." });
            }
        }

        // ===== Contractor Posts =====
        [HttpPost("{contractorId}/posts")]
        [Authorize(Roles = "Contractor")]
        public async Task<ActionResult<ContractorPostDto>> CreatePost(Guid contractorId, [FromBody] ContractorPostCreateDto dto)
        {
            try
            {
                var result = await _contractorService.CreatePostAsync(contractorId, dto);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating contractor post");
                return StatusCode(500, new { Message = "An error occurred while creating post." });
            }
        }

        [HttpGet("{contractorId}/posts")]
        [AllowAnonymous]
        public async Task<ActionResult<List<ContractorPostDto>>> GetPosts(Guid contractorId, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                var posts = await _contractorService.GetContractorPostsAsync(contractorId, page, pageSize);
                return Ok(posts);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching contractor posts");
                return StatusCode(500, new { Message = "An error occurred while fetching posts." });
            }
        }

        [HttpDelete("{contractorId}/posts/{postId}")]
        [Authorize(Roles = "Contractor")]
        public async Task<IActionResult> DeletePost(Guid contractorId, Guid postId)
        {
            try
            {
                await _contractorService.DeletePostAsync(contractorId, postId);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting contractor post");
                return StatusCode(500, new { Message = "An error occurred while deleting post." });
            }
        }
    }

    // Additional DTOs for controller endpoints
    public class ValidateCommunicationDto
    {
        public string Content { get; set; } = string.Empty;
        public Guid ToUserId { get; set; }
        public Guid? ProjectId { get; set; }
    }

    public class ContractorStatisticsDto
    {
        public int TotalContractors { get; set; }
        public int VerifiedContractors { get; set; }
        public int PremiumContractors { get; set; }
        public decimal AverageRating { get; set; }
        public int RestrictedContractors { get; set; }
    }
}