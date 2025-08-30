 using OCSP.Domain.Enums;

namespace OCSP.Application.DTOs.Contractor
{
public class AIRecommendationRequestDto
    {
        public string ProjectDescription { get; set; } = string.Empty;
        public decimal? Budget { get; set; }
        public string? PreferredLocation { get; set; }
        public List<string>? RequiredSpecialties { get; set; }
        public DateTime? PreferredStartDate { get; set; }
        public ProjectComplexity Complexity { get; set; } = ProjectComplexity.Medium;
    }}