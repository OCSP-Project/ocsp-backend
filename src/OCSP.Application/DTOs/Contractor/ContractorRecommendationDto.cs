using OCSP.Domain.Enums;

namespace OCSP.Application.DTOs.Contractor
{

 public class ContractorRecommendationDto
    {
        public ContractorSummaryDto Contractor { get; set; } = null!;
        public decimal MatchScore { get; set; }
        public string MatchReason { get; set; } = string.Empty;
        public List<string> MatchingFactors { get; set; } = new List<string>();
    }
}