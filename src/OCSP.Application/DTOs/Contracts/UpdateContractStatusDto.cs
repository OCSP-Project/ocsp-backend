using System;
using OCSP.Domain.Enums; // Enum ContractStatus

namespace OCSP.Application.DTOs.Contracts
{
    public class UpdateContractStatusDto
    {
        public Guid ContractId { get; set; }
        public ContractStatus Status { get; set; }  // New status
    }
}
