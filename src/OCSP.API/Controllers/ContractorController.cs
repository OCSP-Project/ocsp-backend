using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OCSP.Application.DTOs.Contractor;
using OCSP.Application.Services.Interfaces;
using OCSP.Infrastructure.Repositories.Interfaces;
using OCSP.Infrastructure.Services;
using AutoMapper;
using System.Security.Claims;
using System.IO;

namespace OCSP.API.Controllers
{

    [ApiController]
    [Route("api/[controller]")]
    public class ContractorController : ControllerBase
    {
        private readonly IContractorService _contractorService;
        private readonly ILogger<ContractorController> _logger;
        private readonly IContractorRepository _contractorRepository;
        private readonly IMapper _mapper;
        private readonly IFileStorageService _fileStorageService;

        public ContractorController(
            IContractorService contractorService,
            ILogger<ContractorController> logger,
            IContractorRepository contractorRepository,
            IMapper mapper,
            IFileStorageService fileStorageService)
        {
            _contractorService = contractorService;
            _logger = logger;
            _contractorRepository = contractorRepository;
            _mapper = mapper;
            _fileStorageService = fileStorageService;
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
        /// Get contractor profile by current authenticated user
        /// </summary>
        [HttpGet("me")]
        [Authorize(Roles = "Contractor")]
        public async Task<ActionResult<ContractorProfileDto>> GetMyContractorProfile()
        {
            try
            {
                var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                    ?? throw new UnauthorizedAccessException("User ID not found in token"));

                _logger.LogInformation("Getting contractor profile for user ID: {UserId}", userId);

                var contractor = await _contractorRepository.GetByUserIdAsync(userId);

                if (contractor == null)
                {
                    _logger.LogWarning("Contractor not found for user ID: {UserId}", userId);
                    return NotFound(new { Message = "Contractor profile not found for this user." });
                }

                var contractorDto = _mapper.Map<ContractorProfileDto>(contractor);
                return Ok(contractorDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting contractor profile");
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

        // ===== Company Information Management =====
        /// <summary>
        /// Update contractor company information including Google Maps place URL and featured image
        /// </summary>
        [HttpPut("me/company-info")]
        [Authorize(Roles = "Contractor")]
        public async Task<ActionResult<ContractorProfileDto>> UpdateCompanyInfo(
            [FromForm] UpdateContractorCompanyInfoDto dto,
            [FromForm] IFormFile? featuredImage = null)
        {
            try
            {
                var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                    ?? throw new UnauthorizedAccessException("User ID not found in token"));

                _logger.LogInformation("Updating company info for user ID: {UserId}", userId);

                var contractor = await _contractorRepository.GetByUserIdAsync(userId);
                if (contractor == null)
                {
                    return NotFound(new { Message = "Contractor profile not found." });
                }

                // Update basic company information
                contractor.CompanyName = dto.CompanyName;
                contractor.Description = dto.Description;
                contractor.Website = dto.Website;
                contractor.ContactPhone = dto.ContactPhone;
                contractor.ContactEmail = dto.ContactEmail;
                contractor.Address = dto.Address;
                contractor.City = dto.City;
                contractor.Province = dto.Province;

                // Extract and save Google Maps data_id from URL
                if (!string.IsNullOrWhiteSpace(dto.GoogleMapsPlaceUrl))
                {
                    contractor.GoogleMapsPlaceUrl = dto.GoogleMapsPlaceUrl;
                    contractor.GoogleMapsDataId = ExtractDataIdFromUrl(dto.GoogleMapsPlaceUrl);

                    // Fetch reviews and calculate average rating
                    if (!string.IsNullOrWhiteSpace(contractor.GoogleMapsDataId))
                    {
                        try
                        {
                            // Fetch 5 latest reviews from Google Maps
                            var reviews = await FetchGoogleMapsReviewsAsync(contractor.GoogleMapsDataId);

                            if (reviews != null && reviews.Any())
                            {
                                // Calculate average rating from reviews
                                var averageRating = reviews.Average(r => r.Rating);
                                contractor.GoogleMapsRating = (decimal)averageRating;
                                contractor.GoogleMapsReviewCount = reviews.Count;

                                _logger.LogInformation("Calculated Google Maps rating: {Rating} from {Count} reviews",
                                    averageRating, reviews.Count);
                            }
                            else
                            {
                                _logger.LogWarning("No reviews found for Google Maps place");
                                contractor.GoogleMapsRating = null;
                                contractor.GoogleMapsReviewCount = 0;
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning(ex, "Failed to fetch Google Maps reviews, continuing anyway");
                            // Don't fail the whole request if Google Maps fetch fails
                        }
                    }
                }
                else
                {
                    contractor.GoogleMapsPlaceUrl = null;
                    contractor.GoogleMapsDataId = null;
                    contractor.GoogleMapsRating = null;
                    contractor.GoogleMapsReviewCount = null;
                }

                // Handle featured image upload
                if (featuredImage != null && featuredImage.Length > 0)
                {
                    // Validate file size (max 5MB)
                    if (featuredImage.Length > 5 * 1024 * 1024)
                    {
                        return BadRequest(new { Message = "File size exceeds 5MB limit." });
                    }

                    // Validate file type
                    var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
                    var fileExtension = Path.GetExtension(featuredImage.FileName).ToLowerInvariant();
                    if (!allowedExtensions.Contains(fileExtension))
                    {
                        return BadRequest(new { Message = "Invalid file type. Allowed types: JPG, PNG, GIF, WEBP" });
                    }

                    // Delete old image if exists
                    if (!string.IsNullOrEmpty(contractor.FeaturedImageUrl))
                    {
                        try
                        {
                            await _fileStorageService.DeleteFileAsync(contractor.FeaturedImageUrl);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning(ex, "Failed to delete old featured image");
                        }
                    }

                    // Upload new image
                    var fileName = $"contractor-{contractor.Id}-{Guid.NewGuid()}{fileExtension}";
                    var imageUrl = await _fileStorageService.UploadFileAsync(featuredImage, "contractor-images", fileName);
                    contractor.FeaturedImageUrl = imageUrl;

                    _logger.LogInformation("Uploaded featured image for contractor {ContractorId}: {ImageUrl}",
                        contractor.Id, imageUrl);
                }

                await _contractorRepository.UpdateAsync(contractor);

                var updatedDto = _mapper.Map<ContractorProfileDto>(contractor);
                return Ok(updatedDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating company info");
                return StatusCode(500, new { Message = "An error occurred while updating company info." });
            }
        }

        /// <summary>
        /// Refresh Google Maps rating and review count for current contractor
        /// </summary>
        [HttpPost("me/refresh-google-maps-rating")]
        [Authorize(Roles = "Contractor")]
        public async Task<ActionResult> RefreshGoogleMapsRating()
        {
            try
            {
                var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                    ?? throw new UnauthorizedAccessException("User ID not found in token"));

                var contractor = await _contractorRepository.GetByUserIdAsync(userId);
                if (contractor == null)
                {
                    return NotFound(new { Message = "Contractor profile not found." });
                }

                if (string.IsNullOrWhiteSpace(contractor.GoogleMapsDataId))
                {
                    return BadRequest(new { Message = "Google Maps data ID not found. Please set Google Maps URL first." });
                }

                _logger.LogInformation("Refreshing Google Maps rating for contractor {ContractorId} with dataId: {DataId}",
                    contractor.Id, contractor.GoogleMapsDataId);

                var placeDetails = await FetchGoogleMapsPlaceDetailsAsync(contractor.GoogleMapsDataId);
                contractor.GoogleMapsRating = placeDetails.Rating;
                contractor.GoogleMapsReviewCount = placeDetails.ReviewCount;

                await _contractorRepository.UpdateAsync(contractor);

                _logger.LogInformation("Successfully updated Google Maps rating: {Rating}, reviews: {Count}",
                    placeDetails.Rating, placeDetails.ReviewCount);

                return Ok(new
                {
                    Rating = placeDetails.Rating,
                    ReviewCount = placeDetails.ReviewCount,
                    Message = "Google Maps rating updated successfully"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error refreshing Google Maps rating");
                return StatusCode(500, new { Message = "An error occurred while refreshing Google Maps rating.", Detail = ex.Message });
            }
        }

        /// <summary>
        /// Get Google Maps reviews for contractor
        /// </summary>
        [HttpGet("google-maps-reviews")]
        [Authorize(Roles = "Contractor")]
        public async Task<ActionResult<GoogleMapsReviewsResponseDto>> GetGoogleMapsReviews([FromQuery] string dataId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(dataId))
                {
                    return BadRequest(new { Message = "dataId is required." });
                }

                _logger.LogInformation("Fetching Google Maps reviews for dataId: {DataId}", dataId);

                var reviews = await FetchGoogleMapsReviewsAsync(dataId);

                return Ok(new GoogleMapsReviewsResponseDto { Reviews = reviews });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching Google Maps reviews");
                return StatusCode(500, new { Message = "An error occurred while fetching Google Maps reviews." });
            }
        }

        // ===== Helper Methods =====
        private string? ExtractDataIdFromUrl(string url)
        {
            try
            {
                _logger.LogInformation("Attempting to extract data_id from URL: {Url}", url);

                // If it's a shortened URL (goo.gl, maps.app.goo.gl), we need to follow redirects
                if (url.Contains("goo.gl") || url.Contains("maps.app.goo.gl"))
                {
                    _logger.LogInformation("Detected shortened URL, following redirects...");
                    url = FollowRedirects(url);
                    _logger.LogInformation("Full URL after redirect: {Url}", url);
                }

                // Method 1: Extract from !1s format (most common in share URLs)
                // Format: !1s0x89c259af336b3341:0xa4969e07ce3108de
                var match1s = System.Text.RegularExpressions.Regex.Match(url, @"!1s([0-9a-fxA-FX:]+)", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                if (match1s.Success && match1s.Groups.Count > 1)
                {
                    var dataId = match1s.Groups[1].Value;
                    _logger.LogInformation("Extracted data_id using !1s pattern: {DataId}", dataId);
                    return dataId;
                }

                // Method 2: Extract from !3m format
                // Format: !3m1!1s0x89c259af336b3341:0xa4969e07ce3108de
                var match3m = System.Text.RegularExpressions.Regex.Match(url, @"!3m\d+!1s([0-9a-fxA-FX:]+)", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                if (match3m.Success && match3m.Groups.Count > 1)
                {
                    var dataId = match3m.Groups[1].Value;
                    _logger.LogInformation("Extracted data_id using !3m pattern: {DataId}", dataId);
                    return dataId;
                }

                // Method 3: Extract from ftid parameter (Feature ID)
                // Format: ftid=0x89c259af336b3341:0xa4969e07ce3108de
                var matchFtid = System.Text.RegularExpressions.Regex.Match(url, @"ftid=([0-9a-fxA-FX:]+)", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                if (matchFtid.Success && matchFtid.Groups.Count > 1)
                {
                    var dataId = matchFtid.Groups[1].Value;
                    _logger.LogInformation("Extracted data_id using ftid parameter: {DataId}", dataId);
                    return dataId;
                }

                // Method 4: Extract from /place/ path with CID
                // Format: /place/.../@...,.../data=!4m...!1s0x89c259af336b3341:0xa4969e07ce3108de
                var matchData = System.Text.RegularExpressions.Regex.Match(url, @"data=.*?!1s([0-9a-fxA-FX:]+)", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                if (matchData.Success && matchData.Groups.Count > 1)
                {
                    var dataId = matchData.Groups[1].Value;
                    _logger.LogInformation("Extracted data_id using data= pattern: {DataId}", dataId);
                    return dataId;
                }

                _logger.LogWarning("Could not extract data_id from URL using any pattern: {Url}", url);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error extracting data_id from URL: {Url}", url);
                return null;
            }
        }

        private string FollowRedirects(string url)
        {
            try
            {
                using var httpClient = new HttpClient(new HttpClientHandler
                {
                    AllowAutoRedirect = false
                });
                httpClient.Timeout = TimeSpan.FromSeconds(10);

                var currentUrl = url;
                var maxRedirects = 10;
                var redirectCount = 0;

                while (redirectCount < maxRedirects)
                {
                    var request = new HttpRequestMessage(HttpMethod.Get, currentUrl);
                    var response = httpClient.Send(request);

                    if (response.StatusCode == System.Net.HttpStatusCode.Moved ||
                        response.StatusCode == System.Net.HttpStatusCode.MovedPermanently ||
                        response.StatusCode == System.Net.HttpStatusCode.Redirect ||
                        response.StatusCode == System.Net.HttpStatusCode.TemporaryRedirect ||
                        response.StatusCode == System.Net.HttpStatusCode.PermanentRedirect)
                    {
                        var location = response.Headers.Location;
                        if (location == null)
                        {
                            break;
                        }

                        currentUrl = location.IsAbsoluteUri ? location.ToString() : new Uri(new Uri(currentUrl), location).ToString();
                        redirectCount++;
                        _logger.LogInformation("Redirect #{Count}: {Url}", redirectCount, currentUrl);
                    }
                    else
                    {
                        // No more redirects
                        break;
                    }
                }

                return currentUrl;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error following redirects for URL: {Url}", url);
                return url; // Return original URL if error
            }
        }

        private async Task<List<GoogleMapsReviewDto>> FetchGoogleMapsReviewsAsync(string dataId)
        {
            try
            {
                var apiKey = Environment.GetEnvironmentVariable("SERPAPI_KEY");
                if (string.IsNullOrWhiteSpace(apiKey))
                {
                    _logger.LogError("SERPAPI_KEY environment variable not set");
                    throw new Exception("SERPAPI_KEY not configured");
                }

                var ht = new System.Collections.Hashtable
                {
                    { "engine", "google_maps_reviews" },
                    { "data_id", dataId },
                    { "hl", "vi" } // Vietnamese language
                };

                var search = new SerpApi.GoogleSearch(ht, apiKey);
                var data = search.GetJson();

                var reviews = new List<GoogleMapsReviewDto>();

                // Parse reviews from SerpAPI response
                var reviewsArray = data["reviews"] as Newtonsoft.Json.Linq.JArray;
                if (reviewsArray != null)
                {
                    foreach (var review in reviewsArray.Take(5))
                    {
                        // Extract user info
                        var user = review["user"];
                        var authorName = user?["name"]?.ToString() ?? "Unknown";
                        var authorUrl = user?["link"]?.ToString();
                        var profilePhotoUrl = user?["thumbnail"]?.ToString();

                        // Extract review text from details
                        var details = review["details"] as Newtonsoft.Json.Linq.JObject;
                        var reviewText = "";
                        if (details != null)
                        {
                            // Combine all detail values into text
                            var detailTexts = new List<string>();
                            foreach (var prop in details.Properties())
                            {
                                detailTexts.Add($"{prop.Name}: {prop.Value}");
                            }
                            reviewText = string.Join(", ", detailTexts);
                        }

                        // Extract time
                        var isoDate = review["iso_date"]?.ToString();
                        long timestamp = 0;
                        if (!string.IsNullOrEmpty(isoDate) && DateTime.TryParse(isoDate, out DateTime parsedDate))
                        {
                            timestamp = new DateTimeOffset(parsedDate).ToUnixTimeSeconds();
                        }

                        reviews.Add(new GoogleMapsReviewDto
                        {
                            AuthorName = authorName,
                            AuthorUrl = authorUrl,
                            ProfilePhotoUrl = profilePhotoUrl,
                            Rating = (int)(review["rating"]?.ToObject<double>() ?? 0),
                            Text = reviewText,
                            Time = timestamp,
                            RelativeTimeDescription = review["date"]?.ToString()
                        });
                    }
                }

                _logger.LogInformation("Successfully fetched {Count} Google Maps reviews", reviews.Count);

                return reviews;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calling SerpAPI");
                throw;
            }
        }

        private async Task<GoogleMapsPlaceDetails> FetchGoogleMapsPlaceDetailsAsync(string dataId)
        {
            try
            {
                var apiKey = Environment.GetEnvironmentVariable("SERPAPI_KEY");
                if (string.IsNullOrWhiteSpace(apiKey))
                {
                    _logger.LogError("SERPAPI_KEY environment variable not set");
                    throw new Exception("SERPAPI_KEY not configured");
                }

                var ht = new System.Collections.Hashtable
                {
                    { "engine", "google_maps" },
                    { "type", "place" },
                    { "data_id", dataId },
                    { "hl", "vi" } // Vietnamese language
                };

                var search = new SerpApi.GoogleSearch(ht, apiKey);
                var data = search.GetJson();

                // Extract rating and review count from place details
                var rating = data["place_results"]?["rating"]?.ToObject<decimal?>() ?? 0m;
                var reviewCount = data["place_results"]?["reviews"]?.ToObject<int?>() ?? 0;

                _logger.LogInformation("Fetched place details - Rating: {Rating}, Reviews: {Count}", rating, reviewCount);

                return new GoogleMapsPlaceDetails
                {
                    Rating = rating,
                    ReviewCount = reviewCount
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calling SerpAPI for place details");
                throw;
            }
        }

        // Helper class for place details
        private class GoogleMapsPlaceDetails
        {
            public decimal? Rating { get; set; }
            public int? ReviewCount { get; set; }
        }

        // ===== Contractor Posts =====
        [HttpPost("posts")]
        [Authorize(Roles = "Contractor")]
        [Consumes("application/json")]
        public async Task<ActionResult<ContractorPostDto>> CreatePost([FromBody] ContractorPostCreateDto dto)
        {
            try
            {
                // Lấy UserId từ JWT token
                var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                    ?? throw new UnauthorizedAccessException("User ID not found in token"));

                var result = await _contractorService.CreatePostAsync(userId, dto);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating contractor post: {Message}. StackTrace: {StackTrace}",
                    ex.Message, ex.StackTrace);
                return StatusCode(500, new { Message = ex.Message, Detail = ex.InnerException?.Message });
            }
        }

        [HttpPost("posts/multipart")]
        [Authorize(Roles = "Contractor")]
        [Consumes("multipart/form-data")]
        public async Task<ActionResult<ContractorPostDto>> CreatePostWithFiles([FromForm] string title, [FromForm] string? description, [FromForm] IFormFileCollection? images)
        {
            try
            {
                // Lấy UserId từ JWT token
                var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                    ?? throw new UnauthorizedAccessException("User ID not found in token"));

                var imageUrls = new List<string>();

                // ✅ Process uploaded files
                if (images != null && images.Count > 0)
                {
                    // Ensure upload directory exists
                    var uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "uploads", "contractor-posts");
                    Directory.CreateDirectory(uploadPath);

                    foreach (var image in images)
                    {
                        if (image.Length > 0)
                        {
                            try
                            {
                                // Generate unique filename
                                var fileName = $"{Guid.NewGuid()}_{Path.GetFileName(image.FileName)}";
                                var filePath = Path.Combine(uploadPath, fileName);

                                // Save file
                                using (var stream = new FileStream(filePath, FileMode.Create))
                                {
                                    await image.CopyToAsync(stream);
                                }

                                // Add URL (accessible via /uploads/...)
                                var fileUrl = $"/uploads/contractor-posts/{fileName}";
                                imageUrls.Add(fileUrl);

                                _logger.LogInformation("File uploaded successfully: {FileName} -> {FilePath}", image.FileName, fileUrl);
                            }
                            catch (Exception fileEx)
                            {
                                _logger.LogError(fileEx, "Error uploading file: {FileName}", image.FileName);
                                // Continue with other files instead of failing completely
                            }
                        }
                    }
                }

                var dto = new ContractorPostCreateDto
                {
                    Title = title,
                    Description = description,
                    ImageUrls = imageUrls
                };

                var result = await _contractorService.CreatePostAsync(userId, dto);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating contractor post with files: {Message}. StackTrace: {StackTrace}",
                    ex.Message, ex.StackTrace);
                return StatusCode(500, new { Message = ex.Message, Detail = ex.InnerException?.Message });
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