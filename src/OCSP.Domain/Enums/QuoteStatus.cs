
namespace OCSP.Domain.Enums
{
    public enum QuoteStatus
    {
        Draft = 0,      // tạo nháp
        Sent = 1,       // đã gửi cho nhà thầu
        Closed = 2,     // đã chốt (sẽ qua UC-25)
        Cancelled = 3   // huỷ
    }
}
