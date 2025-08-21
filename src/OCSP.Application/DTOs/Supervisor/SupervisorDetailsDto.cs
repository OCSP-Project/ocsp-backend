// OCSP.Application/DTOs/Supervisor/SupervisorDetailsDto.cs
namespace OCSP.Application.DTOs.Supervisor
{
    public sealed class SupervisorDetailsDto
    {
        public Guid Id { get; set; }

        public string Username { get; set; } = "";
        public string Email    { get; set; } = "";
        public string Phone    { get; set; } = "";

        public string Department { get; set; } = "";
        public string Position   { get; set; } = "";
        public string? District  { get; set; }

        public double?  Rating       { get; set; }
        public int?     ReviewsCount { get; set; }
        public decimal? MinRate      { get; set; }
        public decimal? MaxRate      { get; set; }
        public bool     AvailableNow { get; set; }

        // có thể thêm Bio/YearsExperience sau này nếu cần
        public string? Bio { get; set; }
        public int? YearsExperience { get; set; }
    }
}
