using OCSP.Domain.Enums;

namespace OCSP.Application.DTOs.Quotes
{
    public class QuoteRequestDto
    {
        public Guid Id { get; set; }
        public Guid ProjectId { get; set; }
        public string Scope { get; set; } = string.Empty;
        public DateTime? DueDate { get; set; }
        public string Status { get; set; } = QuoteStatus.Draft.ToString();
        public List<Guid> InviteeUserIds { get; set; } = new();
    }
}