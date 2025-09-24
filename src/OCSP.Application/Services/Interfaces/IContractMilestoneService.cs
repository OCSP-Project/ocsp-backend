// OCSP.Application/Services/Interfaces/IContractMilestoneService.cs
using OCSP.Application.DTOs.Milestones;

namespace OCSP.Application.Services.Interfaces
{
    public interface IContractMilestoneService
    {
       Task<MilestoneDto> CreateAsync(Guid contractId, CreateMilestoneDto dto, Guid currentUserId, CancellationToken ct = default);


        Task<IEnumerable<MilestoneDto>> ListByContractAsync(Guid contractId, Guid currentUserId, CancellationToken ct = default);
        Task<MilestoneDto> SubmitAsync(SubmitMilestoneDto dto, Guid contractorUserId, CancellationToken ct = default);
        Task<IEnumerable<MilestoneDto>> BulkCreateAsync(BulkCreateMilestonesDto dto, Guid currentUserId, CancellationToken ct = default);

    }
}
