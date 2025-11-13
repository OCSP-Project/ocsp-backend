
using OCSP.Infrastructure.ExternalServices.Interfaces;
using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Configuration;
namespace OCSP.Infrastructure.ExternalServices
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;

        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task SendEmailAsync(string to, string subject, string body)
        {
            var smtpHost = _configuration["Email:SmtpHost"];
            var smtpPort = int.Parse(_configuration["Email:SmtpPort"] ?? "587");
            var smtpUsername = _configuration["Email:Username"];
            var smtpPassword = _configuration["Email:Password"];
            var fromEmail = _configuration["Email:FromEmail"];

            using var client = new SmtpClient(smtpHost, smtpPort)
            {
                EnableSsl = true,
                Credentials = new NetworkCredential(smtpUsername, smtpPassword)
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress(fromEmail ?? "noreply@ocsp.com", "OCSP System"),
                Subject = subject,
                Body = body,
                IsBodyHtml = true
            };

            mailMessage.To.Add(to);

            await client.SendMailAsync(mailMessage);
        }

        public async Task SendVerificationEmailAsync(string email, string token)
        {
            var subject = "Xác thực email của bạn";
            var body = $@"
                <h2>Xác thực email</h2>
                <p>Mã xác thực của bạn là: <strong>{token}</strong></p>
                <p>Mã này sẽ hết hạn sau 15 phút.</p>
            ";

            await SendEmailAsync(email, subject, body);
        }

        public async Task SendPasswordResetEmailAsync(string email, string token)
        {
            var subject = "Đặt lại mật khẩu";
            var body = $@"
                <h2>Đặt lại mật khẩu</h2>
                <p>Mã đặt lại mật khẩu của bạn là: <strong>{token}</strong></p>
                <p>Mã này sẽ hết hạn sau 15 phút.</p>
            ";

            await SendEmailAsync(email, subject, body);
        }

        public async Task SendInvitationEmailAsync(
            string toEmail,
            string inviterName,
            string projectName,
            string invitationLink,
            string roleName,
            string? customMessage = null,
            CancellationToken ct = default)
        {
            var subject = $"Lời mời tham gia dự án: {projectName}";

            var htmlBody = $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background: #2563eb; color: white; padding: 20px; text-align: center; border-radius: 5px 5px 0 0; }}
        .content {{ background: #f9fafb; padding: 30px; border: 1px solid #e5e7eb; }}
        .button {{ display: inline-block; padding: 12px 24px; background: #2563eb; color: white; text-decoration: none; border-radius: 5px; margin: 20px 0; }}
        .footer {{ text-align: center; padding: 20px; color: #6b7280; font-size: 12px; }}
        .info-box {{ background: white; padding: 15px; border-left: 4px solid #2563eb; margin: 20px 0; }}
    </style>
</head>
<body>
    <div class=""container"">
        <div class=""header"">
            <h1>OCSP - Lời mời tham gia dự án</h1>
        </div>
        <div class=""content"">
            <p>Xin chào,</p>

            <p><strong>{inviterName}</strong> đã mời bạn tham gia dự án:</p>

            <div class=""info-box"">
                <strong>Tên dự án:</strong> {projectName}<br>
                <strong>Vai trò:</strong> {roleName}
            </div>

            {(string.IsNullOrEmpty(customMessage) ? "" : $"<p><em>Lời nhắn: {customMessage}</em></p>")}

            <p>Nhấn vào nút bên dưới để chấp nhận lời mời:</p>

            <div style=""text-align: center;"">
                <a href=""{invitationLink}"" class=""button"">Chấp nhận lời mời</a>
            </div>

            <p style=""color: #6b7280; font-size: 14px; margin-top: 30px;"">
                Lưu ý: Link này sẽ hết hạn sau 7 ngày.
            </p>
        </div>
        <div class=""footer"">
            <p>© 2025 OCSP - Hệ thống quản lý dự án xây dựng</p>
            <p>Nếu bạn không mong đợi email này, vui lòng bỏ qua.</p>
        </div>
    </div>
</body>
</html>";

            await SendEmailAsync(toEmail, subject, htmlBody);
        }
    }
}
