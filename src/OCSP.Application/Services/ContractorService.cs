using AutoMapper;
using OCSP.Application.DTOs.Contractor;
using OCSP.Application.Services.Interfaces;
using OCSP.Domain.Entities;
using OCSP.Domain.Enums;
using OCSP.Infrastructure.Repositories.Interfaces;
using System.Text.RegularExpressions;



namespace OCSP.Application.Services
{
    public class ContractorService : IContractorService
    {
        private readonly IContractorRepository _contractorRepository;
        private readonly ICommunicationRepository _communicationRepository;
        private readonly IMapper _mapper;
        private readonly IAIRecommendationService _aiService;

        // Regex patterns for contact info detection
        private static readonly List<Regex> ContactPatterns = new()
        {
            new Regex(@"\b\d{3}[-.]?\d{3}[-.]?\d{4}\b", RegexOptions.Compiled), // Phone numbers
            new Regex(@"\b[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\.[A-Z|a-z]{2,}\b", RegexOptions.Compiled), // Email
            new Regex(@"\b(zalo|viber|telegram|whatsapp|facebook|fb|messenger)\b", RegexOptions.Compiled | RegexOptions.IgnoreCase), // Social media
            new Regex(@"\b(liên hệ|gọi|call|sdt|phone|số điện thoại|gmail|yahoo|hotmail)\b", RegexOptions.Compiled | RegexOptions.IgnoreCase), // Vietnamese contact keywords
        };

        public ContractorService(
            IContractorRepository contractorRepository,
            ICommunicationRepository communicationRepository,
            IMapper mapper,
            IAIRecommendationService aiService)
        {
            _contractorRepository = contractorRepository;
            _communicationRepository = communicationRepository;
            _mapper = mapper;
            _aiService = aiService;
        }

        // UC-16: Search Contractors
        public async Task<ContractorListResponseDto> SearchContractorsAsync(ContractorSearchDto searchDto)
        {
            var contractors = await _contractorRepository.SearchContractorsAsync(
                query: searchDto.Query,
                location: searchDto.Location,
                specialties: searchDto.Specialties,
                minBudget: searchDto.MinBudget,
                maxBudget: searchDto.MaxBudget,
                minRating: searchDto.MinRating,
                minYearsExperience: searchDto.MinYearsExperience,
                isVerified: searchDto.IsVerified,
                isPremium: searchDto.IsPremium,
                sortBy: searchDto.SortBy,
                page: searchDto.Page,
                pageSize: searchDto.PageSize
            );

            var totalCount = await _contractorRepository.GetSearchCountAsync(
                query: searchDto.Query,
                location: searchDto.Location,
                specialties: searchDto.Specialties,
                minBudget: searchDto.MinBudget,
                maxBudget: searchDto.MaxBudget,
                minRating: searchDto.MinRating,
                minYearsExperience: searchDto.MinYearsExperience,
                isVerified: searchDto.IsVerified,
                isPremium: searchDto.IsPremium
            );

            var contractorDtos = _mapper.Map<List<ContractorSummaryDto>>(contractors);
            
            return new ContractorListResponseDto
            {
                Contractors = contractorDtos,
                TotalCount = totalCount,
                Page = searchDto.Page,
                PageSize = searchDto.PageSize,
                TotalPages = (int)Math.Ceiling((double)totalCount / searchDto.PageSize),
                HasNextPage = searchDto.Page * searchDto.PageSize < totalCount,
                HasPreviousPage = searchDto.Page > 1
            };
        }

        // UC-17: View Contractor List
        public async Task<ContractorListResponseDto> GetAllContractorsAsync(int page = 1, int pageSize = 10)
        {
            var searchDto = new ContractorSearchDto
            {
                Page = page,
                PageSize = pageSize,
                SortBy = Domain.Enums.SearchSortBy.Premium // Premium contractors first
            };

            return await SearchContractorsAsync(searchDto);
        }

        // UC-18: View Contractor Profile
        public async Task<ContractorProfileDto?> GetContractorProfileAsync(Guid contractorId)
        {
            var contractor = await _contractorRepository.GetByIdWithDetailsAsync(contractorId);
            
            if (contractor == null)
                return null;

            var contractorDto = _mapper.Map<ContractorProfileDto>(contractor);

            // Get recent reviews
            var recentReviews = await _contractorRepository.GetRecentReviewsAsync(contractorId, 5);
            contractorDto.RecentReviews = _mapper.Map<List<ReviewSummaryDto>>(recentReviews);

            return contractorDto;
        }

        // UC-22: Receive AI Contractor Recommendations
        public async Task<List<ContractorRecommendationDto>> GetAIRecommendationsAsync(AIRecommendationRequestDto requestDto)
        {
            // Get pool of eligible contractors
            var contractors = await _contractorRepository.SearchContractorsAsync(
                location: requestDto.PreferredLocation,
                specialties: requestDto.RequiredSpecialties,
                minBudget: requestDto.Budget.HasValue ? requestDto.Budget * 0.8m : null, // Allow 20% variance
                maxBudget: requestDto.Budget.HasValue ? requestDto.Budget * 1.2m : null,
                isVerified: true,
                sortBy: Domain.Enums.SearchSortBy.Rating,
                page: 1,
                pageSize: 50 // Get top 50 for AI analysis
            );

            // Use AI to rank and match contractors
            var recommendations = new List<ContractorRecommendationDto>();
            
            foreach (var contractor in contractors)
            {
                var matchScore = await _aiService.CalculateMatchScoreAsync(contractor, requestDto);
                var matchReason = await _aiService.GenerateMatchReasonAsync(contractor, requestDto);
                var matchingFactors = await _aiService.GetMatchingFactorsAsync(contractor, requestDto);

                recommendations.Add(new ContractorRecommendationDto
                {
                    Contractor = _mapper.Map<ContractorSummaryDto>(contractor),
                    MatchScore = matchScore,
                    MatchReason = matchReason,
                    MatchingFactors = matchingFactors
                });
            }

            return recommendations
                .OrderByDescending(r => r.MatchScore)
                .Take(10)
                .ToList();
        }

        // Anti-Circumvention: Validate Communication
        public async Task<CommunicationWarningDto?> ValidateCommunicationAsync(string content, Guid fromUserId, Guid toUserId)
        {
            var containsContactInfo = false;
            var detectedPatterns = new List<string>();

            foreach (var pattern in ContactPatterns)
            {
                if (pattern.IsMatch(content))
                {
                    containsContactInfo = true;
                    detectedPatterns.Add(pattern.ToString());
                }
            }

            if (containsContactInfo)
            {
                // Check user's warning history
                var warningCount = await _communicationRepository.GetUserWarningCountAsync(fromUserId);
                
                var warningLevel = warningCount switch
                {
                    0 => 1, // First warning
                    1 or 2 => 2, // Second-third warning
                    >= 3 => 3 // Final warning
                };

                var warningMessage = warningLevel switch
                {
                    1 => "Cảnh báo: Chúng tôi phát hiện tin nhắn có thể chứa thông tin liên hệ. Vui lòng sử dụng hệ thống chat của OCSP để đảm bảo bảo mật và quyền lợi của bạn.",
                    2 => "Cảnh báo lần 2: Việc chia sẻ thông tin liên hệ vi phạm chính sách platform. Tiếp tục vi phạm có thể dẫn đến hạn chế tài khoản.",
                    _ => "Cảnh báo cuối: Tài khoản của bạn sẽ bị hạn chế nếu tiếp tục vi phạm chính sách. Mọi giao dịch phải thực hiện qua OCSP."
                };

                return new CommunicationWarningDto
                {
                    Message = warningMessage,
                    Reason = "Phát hiện thông tin liên hệ trong tin nhắn",
                    WarningLevel = warningLevel,
                    RequiresAcknowledgment = warningLevel >= 2
                };
            }

            return null;
        }

        // Anti-Circumvention: Log Communication
        public async Task LogCommunicationAsync(Guid fromUserId, Guid toUserId, string content, Domain.Enums.CommunicationType type, Guid? projectId = null)
        {
            var containsContactInfo = ContactPatterns.Any(pattern => pattern.IsMatch(content));

            var communication = new Communication
            {
                FromUserId = fromUserId,
                ToUserId = toUserId,
                ProjectId = projectId,
                Content = content,
                Type = type,
                ContainsContactInfo = containsContactInfo,
                IsFlagged = containsContactInfo
            };

            await _communicationRepository.AddAsync(communication);

            // If contact info detected, increment warning count
            if (containsContactInfo)
            {
                await _communicationRepository.IncrementWarningCountAsync(fromUserId);
            }
        }
    }
}