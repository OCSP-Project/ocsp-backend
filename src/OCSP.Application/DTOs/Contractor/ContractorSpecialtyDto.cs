using OCSP.Domain.Enums;

namespace OCSP.Application.DTOs.Contractor
{
public class ContractorSpecialtyDto
    {
        public Guid Id { get; set; }
        public string SpecialtyName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int ExperienceYears { get; set; }
    }}