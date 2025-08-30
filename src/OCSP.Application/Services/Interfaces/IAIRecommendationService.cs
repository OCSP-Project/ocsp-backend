using System.Collections.Generic;
using System.Threading.Tasks;
using OCSP.Application.DTOs.Contractor;
using OCSP.Domain.Entities;

namespace OCSP.Application.Services.Interfaces
{
    public interface IAIRecommendationService
    {
        Task<decimal> CalculateMatchScoreAsync(Contractor contractor, AIRecommendationRequestDto request);
        Task<string> GenerateMatchReasonAsync(Contractor contractor, AIRecommendationRequestDto request);
        Task<List<string>> GetMatchingFactorsAsync(Contractor contractor, AIRecommendationRequestDto request);
        Task<List<string>> ExtractProjectRequirementsAsync(string projectDescription);
        Task<List<Contractor>> RankContractorsByCompatibilityAsync(List<Contractor> contractors, AIRecommendationRequestDto request);
    }
}
