using OCSP.Domain.Enums;

namespace OCSP.Application.DTOs.Contractor
    {
 public class ContractorSearchDto
    {
        public string? Query { get; set; }
        public string? Location { get; set; } = "Da Nang";
        public List<string>? Specialties { get; set; }
        public decimal? MinBudget { get; set; }
        public decimal? MaxBudget { get; set; }
        public decimal? MinRating { get; set; }
        public int? MinYearsExperience { get; set; }
        public bool? IsVerified { get; set; }
        public bool? IsPremium { get; set; }
        public SearchSortBy SortBy { get; set; } = SearchSortBy.Relevance;
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}