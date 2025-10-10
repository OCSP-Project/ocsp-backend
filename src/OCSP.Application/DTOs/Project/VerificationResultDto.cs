// OCSP.Application/DTOs/Project/VerificationResultDto.cs
namespace OCSP.Application.DTOs.Project
{
    public class VerificationResultDto
    {
        public bool IsValid { get; set; }
        public string Status { get; set; } = string.Empty; // "pass", "warning", "fail"
        public List<VerificationRuleDto> Rules { get; set; } = new();
        public string Message { get; set; } = string.Empty;
    }

    public class VerificationRuleDto
    {
        public string Name { get; set; } = string.Empty;
        public bool IsValid { get; set; }
        public string Message { get; set; } = string.Empty;
        public string Severity { get; set; } = "info"; // "error", "warning", "info"
    }
}