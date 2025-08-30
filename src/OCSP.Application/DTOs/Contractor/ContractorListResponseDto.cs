
using OCSP.Domain.Enums;

namespace OCSP.Application.DTOs.Contractor
{
 // UC-17: View Contractor List Response
    public class ContractorListResponseDto
    {
        public List<ContractorSummaryDto> Contractors { get; set; } = new List<ContractorSummaryDto>();
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
        public bool HasNextPage { get; set; }
        public bool HasPreviousPage { get; set; }
    }}