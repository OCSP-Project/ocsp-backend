using Microsoft.AspNetCore.Mvc;
using OCSP.Application.Services.Interfaces;

namespace OCSP.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProjectController : ControllerBase
{
    private readonly IProjectService _svc;
    public ProjectController(IProjectService svc) => _svc = svc;

    [HttpGet]
    public IActionResult GetAll() => Ok(new[] { new { Id = 1, Name = "Sample" } });

}
