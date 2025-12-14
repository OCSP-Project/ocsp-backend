namespace OCSP.Infrastructure.ExternalServices.Interfaces
{
    public interface IEmailService
    {
        Task SendEmailAsync(string to, string subject, string body);
        Task SendVerificationEmailAsync(string email, string token);
        Task SendPasswordResetEmailAsync(string email, string token);
        Task SendPasswordChangeConfirmationEmailAsync(string email, string username);

        // NEW: Project invitation emails
        Task SendInvitationEmailAsync(
            string toEmail,
            string inviterName,
            string projectName,
            string invitationLink,
            string roleName,
            string? customMessage = null,
            CancellationToken ct = default);
    }
}