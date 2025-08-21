// OCSP.Application/DTOs/Supervisor/FilterSupervisorsRequest.cs
namespace OCSP.Application.DTOs.Supervisor
{
    public sealed class FilterSupervisorsRequest
    {
        public string? District { get; set; }   // quận ở Đà Nẵng
        public double? MinRating { get; set; }
        public decimal? PriceMin { get; set; }
        public decimal? PriceMax { get; set; }
        public bool? AvailableNow { get; set; }

        // paging
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }
}
