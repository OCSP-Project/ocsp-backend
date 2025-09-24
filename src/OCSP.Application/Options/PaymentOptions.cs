namespace OCSP.Application.Options
{
    public class PaymentOptions
    {
        /// <summary>Phí platform (0.10 = 10%) – dùng cho UC-30.</summary>
        public decimal CommissionPercent { get; set; } = 0.10m;
    }
}
