using System.Threading.Tasks;
using OCSP.Domain.Entities;

namespace OCSP.Infrastructure.ExternalServices.Interfaces
{
    public interface IPdfService
    {
        Task<byte[]> GenerateContractPdfAsync(
            Contract contract, 
            Profile homeownerProfile, 
            Profile contractorProfile, 
            Contractor? contractorCompany,
            Proposal proposal,
            string? homeownerSignatureBase64 = null,
            string? contractorSignatureBase64 = null);
            
        Task<byte[]> AddSignaturesToPdfAsync(
            byte[] pdfBytes, 
            string? homeownerSignatureBase64, 
            string? contractorSignatureBase64);
            
        Task<byte[]> GenerateSupervisorContractPdfAsync(
            SupervisorContract contract,
            Profile homeownerProfile,
            Profile supervisorProfile,
            Project project,
            string? homeownerSignatureBase64 = null,
            string? supervisorSignatureBase64 = null);
    }
}

