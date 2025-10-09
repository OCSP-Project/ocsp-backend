using OCSP.Domain.Enums;

namespace OCSP.Domain.Entities
{
    public class Wallet
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid UserId { get; set; }
        public decimal Available { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }

    public class WalletTransaction
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid UserId { get; set; }
        public string MomoOrderId { get; set; } = string.Empty;
        public string MomoRequestId { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string Status { get; set; } = string.Empty; // raw status or enum text
        public string? RawResponse { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

    public class LedgerEntry
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid WalletId { get; set; }
        public LedgerEntryType Type { get; set; }
        public decimal Amount { get; set; }
        public string? RefId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}


