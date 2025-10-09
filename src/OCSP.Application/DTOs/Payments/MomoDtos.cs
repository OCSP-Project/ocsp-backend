namespace OCSP.Application.DTOs.Payments
{
    public class MomoCreatePaymentDto
    {
        public decimal Amount { get; set; }
        public string? Description { get; set; }
        public Guid? ContractId { get; set; }
    }

    public class MomoCreatePaymentResultDto
    {
        public string PayUrl { get; set; } = string.Empty;
        public string OrderId { get; set; } = string.Empty;
        public string RequestId { get; set; } = string.Empty;
    }

    public class MomoWebhookDto
    {
        public string OrderId { get; set; } = string.Empty;
        public string RequestId { get; set; } = string.Empty;
        public long Amount { get; set; }
        public int ResultCode { get; set; }
        public string Message { get; set; } = string.Empty;
        public string OrderInfo { get; set; } = string.Empty;
        public string Signature { get; set; } = string.Empty;
        public string ExtraData { get; set; } = string.Empty;
        public string ResponseTime { get; set; } = string.Empty;
        public string TransId { get; set; } = string.Empty;
        public string PayType { get; set; } = string.Empty;
    }
}


