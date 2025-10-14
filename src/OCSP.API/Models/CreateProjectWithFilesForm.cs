using Microsoft.AspNetCore.Http;

namespace OCSP.API.Models
{
    public class CreateProjectWithFilesForm
    {
        // Project fields
        public string Name { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public decimal Budget { get; set; }
        public decimal? FloorArea { get; set; }
        public int? NumberOfFloors { get; set; }
        public string? Description { get; set; }
        public string? PermitNumber { get; set; }
        public Guid? ContractorId { get; set; }

        // Files
        public IFormFile DrawingFile { get; set; } = null!;
        public IFormFile PermitFile { get; set; } = null!;
    }
}


