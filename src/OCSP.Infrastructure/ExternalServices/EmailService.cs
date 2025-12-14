
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

        public async Task SendPasswordChangeConfirmationEmailAsync(string email, string username)
        {
            var subject = "Xác nhận đổi mật khẩu";
            var htmlBody = $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background: linear-gradient(135deg, #38c1b6 0%, #667eea 100%); color: white; padding: 30px; text-align: center; border-radius: 10px 10px 0 0; }}
        .content {{ background: #f9fafb; padding: 30px; border: 1px solid #e5e7eb; }}
        .icon {{ font-size: 48px; margin-bottom: 10px; }}
        .success-box {{ background: #d1fae5; border-left: 4px solid #10b981; padding: 15px; margin: 20px 0; border-radius: 5px; }}
        .info-box {{ background: white; padding: 15px; border-left: 4px solid #38c1b6; margin: 20px 0; border-radius: 5px; }}
        .footer {{ text-align: center; padding: 20px; color: #6b7280; font-size: 12px; }}
        .warning {{ color: #dc2626; font-weight: bold; margin-top: 20px; }}
    </style>
</head>
<body>
    <div class=""container"">
        <div class=""header"">
            <div class=""icon"">🔒</div>
            <h1>Đổi mật khẩu thành công</h1>
        </div>
        <div class=""content"">
            <p>Xin chào <strong>{username}</strong>,</p>

            <div class=""success-box"">
                <p style=""margin: 0; color: #065f46;"">
                    ✅ Mật khẩu của bạn đã được thay đổi thành công vào lúc {DateTime.UtcNow.AddHours(7):dd/MM/yyyy HH:mm} (GMT+7)
                </p>
            </div>

            <div class=""info-box"">
                <strong>Để bảo mật tài khoản của bạn:</strong>
                <ul style=""margin: 10px 0; padding-left: 20px;"">
                    <li>Không chia sẻ mật khẩu với bất kỳ ai</li>
                    <li>Sử dụng mật khẩu mạnh và duy nhất cho mỗi tài khoản</li>
                    <li>Thay đổi mật khẩu định kỳ</li>
                    <li>Kích hoạt xác thực hai yếu tố nếu có thể</li>
                </ul>
            </div>

            <div class=""warning"">
                ⚠️ Nếu bạn không thực hiện thay đổi này, vui lòng liên hệ với chúng tôi ngay lập tức!
            </div>

            <p style=""margin-top: 30px; color: #6b7280;"">
                Tất cả các phiên đăng nhập hiện tại của bạn đã được duy trì. Bạn có thể tiếp tục sử dụng hệ thống với mật khẩu mới.
            </p>
        </div>
        <div class=""footer"">
            <p>© 2025 OCSP - Hệ thống quản lý dự án xây dựng</p>
            <p>Email này được gửi tự động, vui lòng không trả lời.</p>
        </div>
    </div>
</body>
</html>";

            await SendEmailAsync(email, subject, htmlBody);
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
