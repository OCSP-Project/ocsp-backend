
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
    }
}
