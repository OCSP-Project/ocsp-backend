namespace OCSP.Application.Options
{
    public class MomoOptions
    {
        public string PartnerCode { get; set; } = string.Empty;
        public string AccessKey { get; set; } = string.Empty;
        public string SecretKey { get; set; } = string.Empty;
        public string Endpoint { get; set; } = string.Empty; // e.g., https://test-payment.momo.vn/v2/gateway/api/create
        public string RedirectUrl { get; set; } = string.Empty; // FE URL to return
        public string IpnUrl { get; set; } = string.Empty;      // Webhook URL
    }
}


