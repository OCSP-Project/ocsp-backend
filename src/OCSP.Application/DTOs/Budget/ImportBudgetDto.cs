using Microsoft.AspNetCore.Http;

namespace OCSP.Application.DTOs.Budget
{
    public class ImportBudgetDto
    {
        public Guid ProjectId { get; set; }
        public IFormFile File { get; set; } = null!;
        public bool OverwriteExisting { get; set; } = false;             // Overwrite existing items
        public string? Notes { get; set; }
    }

    public class ImportBudgetResponseDto
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public int ImportedCount { get; set; }
        public int ErrorCount { get; set; }
        public List<string> Errors { get; set; } = new();
        public List<string> Warnings { get; set; } = new();
        public List<WorkItemDto> ImportedItems { get; set; } = new();
    }
}
