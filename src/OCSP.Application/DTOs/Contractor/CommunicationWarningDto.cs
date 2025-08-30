using OCSP.Domain.Enums;

namespace OCSP.Application.DTOs.Contractor
{
public class CommunicationWarningDto
    {
        public string Message { get; set; } = string.Empty;
        public string Reason { get; set; } = string.Empty;
        public int WarningLevel { get; set; }
        public bool RequiresAcknowledgment { get; set; } = false;
    }}
