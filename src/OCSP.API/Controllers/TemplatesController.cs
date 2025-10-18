using Microsoft.AspNetCore.Mvc;
using OCSP.Application.Services;

namespace OCSP.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TemplatesController : ControllerBase
    {
        private readonly ITemplateService _templateService;
        private readonly ILogger<TemplatesController> _logger;

        public TemplatesController(ITemplateService templateService, ILogger<TemplatesController> logger)
        {
            _templateService = templateService;
            _logger = logger;
        }

        /// <summary>
        /// Download Excel template for proposal creation
        /// </summary>
        /// <returns>Excel template file</returns>
        [HttpGet("proposal-excel")]
        public async Task<IActionResult> DownloadProposalTemplate()
        {
            try
            {
                var fileBytes = await _templateService.GetProposalTemplateAsync();
                var fileName = "proposal-template.xlsx";
                
                _logger.LogInformation("Template downloaded successfully: {FileName}", fileName);
                
                return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error downloading template");
                return StatusCode(500, new { message = "Error downloading template" });
            }
        }

        /// <summary>
        /// Get template information
        /// </summary>
        /// <returns>Template metadata</returns>
        [HttpGet("proposal-excel/info")]
        public async Task<IActionResult> GetTemplateInfo()
        {
            try
            {
                var info = await _templateService.GetTemplateInfoAsync();
                
                return Ok(new
                {
                    fileName = "proposal-template.xlsx",
                    description = "Excel template for creating construction proposals",
                    instructions = new[]
                    {
                        "1. Download this template",
                        "2. Fill in your project information",
                        "3. Add cost items with amounts and percentages",
                        "4. Upload the completed file to create your proposal"
                    },
                    info = info
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting template info");
                return StatusCode(500, new { message = "Error getting template info" });
            }
        }
    }
}
