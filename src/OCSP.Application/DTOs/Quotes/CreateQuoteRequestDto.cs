namespace OCSP.Application.DTOs.Quotes
{
    public class CreateQuoteRequestDto
    {
        public Guid ProjectId { get; set; }
        public string Scope { get; set; } = string.Empty;
        public DateTime? DueDate { get; set; }
        public List<Guid> InviteeUserIds { get; set; } = new(); // danh sách UserId contractor
    }
}