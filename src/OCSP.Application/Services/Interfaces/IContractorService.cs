
using OCSP.Application.DTOs.Contractor;
using OCSP.Application.DTOs.Contracts;


using OCSP.Domain.Enums;
namespace OCSP.Application.Services.Interfaces
{
    public interface IContractorService
    {
        Task<BulkContractorResponseDto> BulkCreateContractorsAsync(BulkContractorRequestDto request);
        Task<ContractorListResponseDto> SearchContractorsAsync(ContractorSearchDto searchDto);
        Task<ContractorListResponseDto> GetAllContractorsAsync(int page = 1, int pageSize = 10);
        Task<ContractorProfileDto?> GetContractorProfileAsync(Guid contractorId);
        Task<List<ContractorRecommendationDto>> GetAIRecommendationsAsync(AIRecommendationRequestDto requestDto);
        Task<CommunicationWarningDto?> ValidateCommunicationAsync(string content, Guid fromUserId, Guid toUserId);
        Task LogCommunicationAsync(Guid fromUserId, Guid toUserId, string content, Domain.Enums.CommunicationType type, Guid? projectId = null);
        Task<ContractorPostDto> CreatePostAsync(Guid contractorId, ContractorPostCreateDto dto);
        Task<List<ContractorPostDto>> GetContractorPostsAsync(Guid contractorId, int page = 1, int pageSize = 10);
        Task DeletePostAsync(Guid contractorId, Guid postId);
    }
}