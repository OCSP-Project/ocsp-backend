// OCSP.Application/DTOs/Common/PagedResult.cs
namespace OCSP.Application.DTOs.Common
{
    public sealed class PagedResult<T>
    {
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalItems { get; set; }
        public int TotalPages { get; set; }
        public List<T> Items { get; set; } = new();
    }
}
