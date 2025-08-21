// OCSP.API/Controllers/SupervisorController.cs
using Microsoft.AspNetCore.Mvc;
using OCSP.Application.DTOs.Common;
using OCSP.Application.DTOs.Supervisor;
using OCSP.Application.Services.Interfaces;

namespace OCSP.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SupervisorController : ControllerBase
{
    private readonly ISupervisorService _svc;
    public SupervisorController(ISupervisorService svc) => _svc = svc;

    /// <summary>Danh sách supervisor theo filter cơ bản</summary>
    [HttpPost("filter")]
    [ProducesResponseType(typeof(PagedResult<SupervisorListItemDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Filter([FromBody] FilterSupervisorsRequest req, CancellationToken ct)
    {
        var result = await _svc.FilterAsync(req, ct);
        return Ok(result);
    }

    /// <summary>Xem hồ sơ chi tiết 1 supervisor</summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(SupervisorDetailsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById([FromRoute] Guid id, CancellationToken ct)
    {
        var result = await _svc.GetByIdAsync(id, ct);
        if (result is null) return NotFound();
        return Ok(result);
    }

    [HttpGet("all")]
    [ProducesResponseType(typeof(List<SupervisorListItemDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var result = await _svc.GetAllAsync(ct);
        return Ok(result);
    }
}