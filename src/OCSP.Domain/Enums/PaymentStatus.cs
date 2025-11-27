namespace OCSP.Domain.Enums
{
    // Payment transaction status (for wallet transactions, escrow, etc.)
    public enum PaymentStatus
    {
        Pending = 0,
        Succeeded = 1,
        Failed = 2
    }

    // Payment request status (for budget payment requests workflow)
    public enum PaymentRequestStatus
    {
        Pending = 0,
        Approved = 1,
        Paid = 2,
        Rejected = 3,
        Cancelled = 4
    }
}
