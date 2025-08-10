using Microsoft.AspNetCore.Mvc;
using OCSP.Application.Services.Interfaces;

namespace OCSP.API.Controllers
{
    [ApiController]
    [Route("api/supervisors")]
    public class SupervisorController : ControllerBase
    {
        private readonly ISupervisorService _svc;
        public SupervisorController(ISupervisorService svc) => _svc = svc;

        [HttpGet]
        public async Task<IActionResult> GetAll()
            => Ok(await _svc.GetAllAsync());

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var data = await _svc.GetByIdAsync(id);
            return data is null ? NotFound() : Ok(data);
        }
    }
}