using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;

namespace OCSP.Application.Services
{
    public interface ITemplateService
    {
        Task<byte[]> GetProposalTemplateAsync();
        Task<string> GetTemplateInfoAsync();
        Task EnsureTemplateExistsAsync();
    }

    public class TemplateService : ITemplateService
    {
        private readonly IWebHostEnvironment _environment;
        private readonly ILogger<TemplateService> _logger;

        public TemplateService(IWebHostEnvironment environment, ILogger<TemplateService> logger)
        {
            _environment = environment;
            _logger = logger;
        }

        public async Task<byte[]> GetProposalTemplateAsync()
        {
            await EnsureTemplateExistsAsync();
            
            var templatePath = GetTemplatePath();
            return await File.ReadAllBytesAsync(templatePath);
        }

        public async Task<string> GetTemplateInfoAsync()
        {
            await EnsureTemplateExistsAsync();
            
            var templatePath = GetTemplatePath();
            var fileInfo = new FileInfo(templatePath);
            
            return $"Template: proposal-template.xlsx, Size: {fileInfo.Length} bytes, Modified: {fileInfo.LastWriteTime}";
        }

        public async Task EnsureTemplateExistsAsync()
        {
            var templatePath = GetTemplatePath();
            var templateDir = Path.GetDirectoryName(templatePath);
            
            // Ensure directory exists
            if (!Directory.Exists(templateDir))
            {
                Directory.CreateDirectory(templateDir!);
            }
            
            // Only create template if it doesn't exist AND no custom template is provided
            if (!File.Exists(templatePath))
            {
                _logger.LogWarning("Template file not found at: {TemplatePath}. Please add your custom template file.", templatePath);
                throw new FileNotFoundException($"Template file not found at: {templatePath}. Please add your custom template file.");
            }
        }

        private string GetTemplatePath()
        {
            return Path.Combine(_environment.ContentRootPath, "Templates", "proposal-template.xlsx");
        }
    }
}
