namespace OCSP.Application.DTOs.Project
{
    public class CreateProjectDto
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string Address { get; set; } = string.Empty;
        public decimal Budget { get; set; }

        // ✅ OCR data từ frontend (client-side OCR)
        public decimal FloorArea { get; set; }
        public int NumberOfFloors { get; set; }
        public string? PermitNumber { get; set; }
        public string? BuildingType { get; set; }
    }
}
