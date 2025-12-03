using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using OCSP.Application.DTOs.Payments;
using OCSP.Application.Options;
using OCSP.Application.Services.Interfaces;
using OCSP.Domain.Entities;
using OCSP.Domain.Enums;
using OCSP.Infrastructure.Data;

namespace OCSP.Application.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly ApplicationDbContext _db;
        private readonly MomoOptions _momo;
        private readonly HttpClient _http;
        private readonly ILogger<PaymentService> _logger;

        private readonly IProjectService _projects;
        private readonly ISupervisorContractService _supervisorContracts;

        public PaymentService(
            ApplicationDbContext db, 
            MomoOptions momo, 
            HttpClient http, 
            ILogger<PaymentService> logger, 
            IProjectService projects,
            ISupervisorContractService supervisorContracts)
        {
            _db = db;
            _momo = momo;
            _http = http;
            _logger = logger;
            _projects = projects;
            _supervisorContracts = supervisorContracts;
        }

        public async Task<MomoCreatePaymentResultDto> CreateMomoPaymentAsync(MomoCreatePaymentDto dto, Guid userId, CancellationToken ct = default)
        {
            if (dto.Amount <= 0) throw new ArgumentException("Amount must be > 0");

            // Ensure wallet exists
            await EnsureWalletExistsAsync(userId, ct);

            // Generate order info
            var orderId = $"{_momo.PartnerCode}-{Guid.NewGuid():N}";
            var requestId = Guid.NewGuid().ToString("N");
            var amount = RoundUpToThousand(dto.Amount);
            var orderInfo = SanitizeOrderInfo(dto.Description);
            var extraData = BuildExtraData(userId, dto);
            var redirectUrl = string.IsNullOrWhiteSpace(dto.RedirectUrl) ? _momo.RedirectUrl : dto.RedirectUrl!;

            // Build MoMo request
            var body = new Dictionary<string, object>
            {
                ["partnerCode"] = _momo.PartnerCode,
                ["accessKey"] = _momo.AccessKey,
                ["requestId"] = requestId,
                ["amount"] = amount,
                ["orderId"] = orderId,
                ["orderInfo"] = orderInfo,
                ["redirectUrl"] = redirectUrl,
                ["ipnUrl"] = _momo.IpnUrl,
                ["requestType"] = "payWithMethod",
                ["extraData"] = extraData,
                ["lang"] = "vi",
                ["partnerName"] = "Test",
                ["storeId"] = "MomoTestStore",
                ["autoCapture"] = true,
                ["orderGroupId"] = string.Empty,
            };

            var rawSignature = $"accessKey={_momo.AccessKey}&amount={amount}&extraData={extraData}&ipnUrl={_momo.IpnUrl}&orderId={orderId}&orderInfo={orderInfo}&partnerCode={_momo.PartnerCode}&redirectUrl={redirectUrl}&requestId={requestId}&requestType=payWithMethod";
            body["signature"] = SignHmacSha256(rawSignature, _momo.SecretKey);

            _logger.LogInformation("[MoMo] Creating payment: orderId={OrderId}, amount={Amount}, purpose={Purpose}", orderId, amount, dto.Purpose);

            // Call MoMo API
            using var req = new HttpRequestMessage(HttpMethod.Post, _momo.Endpoint) { Content = JsonContent.Create(body) };
            req.Headers.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

            var resp = await _http.SendAsync(req, ct);
            var respText = await resp.Content.ReadAsStringAsync(ct);
            
            if (!resp.IsSuccessStatusCode)
            {
                _logger.LogError("[MoMo] Payment creation failed: status={StatusCode}, body={Body}", (int)resp.StatusCode, respText);
                throw new InvalidOperationException($"MoMo create payment failed ({(int)resp.StatusCode}): {respText}");
            }

            var json = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(respText) ?? new();
            var payUrl = json.TryGetValue("payUrl", out var v) ? v?.ToString() ?? string.Empty : string.Empty;
            
            if (string.IsNullOrEmpty(payUrl)) 
                throw new InvalidOperationException("MoMo didn't return payUrl");

            // Save transaction
            _db.WalletTransactions.Add(new WalletTransaction
            {
                UserId = userId,
                MomoOrderId = orderId,
                MomoRequestId = requestId,
                Amount = amount,
                Status = "CREATED",
                RawResponse = System.Text.Json.JsonSerializer.Serialize(json)
            });
            await _db.SaveChangesAsync(ct);

            _logger.LogInformation("[MoMo] Payment created successfully: orderId={OrderId}", orderId);
            return new MomoCreatePaymentResultDto { PayUrl = payUrl, OrderId = orderId, RequestId = requestId };
        }

        private async Task EnsureWalletExistsAsync(Guid userId, CancellationToken ct)
        {
            var wallet = await _db.Wallets.FirstOrDefaultAsync(w => w.UserId == userId, ct);
            if (wallet == null)
            {
                _db.Wallets.Add(new Wallet { UserId = userId, Available = 0m });
                await _db.SaveChangesAsync(ct);
            }
        }

        private static long RoundUpToThousand(decimal amount)
        {
            if (amount <= 0) return 0;
            var rounded = (long)Math.Ceiling(amount);
            var remainder = rounded % 1000;
            return remainder == 0 ? rounded : rounded + (1000 - remainder);
        }

        private static string SanitizeOrderInfo(string? description)
        {
            var text = string.IsNullOrWhiteSpace(description) ? "OCSP Payment" : description!;
            text = RemoveDiacritics(text);
            return text.Length > 100 ? text.Substring(0, 100) : text;
        }

        private static string BuildExtraData(Guid userId, MomoCreatePaymentDto dto)
        {
            var extra = new Dictionary<string, string>
            {
                ["userId"] = userId.ToString(),
                ["contractId"] = dto.ContractId?.ToString() ?? string.Empty,
                ["purpose"] = string.IsNullOrWhiteSpace(dto.Purpose) ? "commission" : dto.Purpose!,
                ["projectId"] = dto.ProjectId?.ToString() ?? string.Empty
            };
            return Convert.ToBase64String(System.Text.Json.JsonSerializer.SerializeToUtf8Bytes(extra));
        }

        public async Task HandleMomoWebhookAsync(MomoWebhookDto payload, string rawBody, CancellationToken ct = default)
        {
            _logger.LogInformation("[MoMo] Webhook received: orderId={OrderId}, amount={Amount}, resultCode={ResultCode}", 
                payload.OrderId, payload.Amount, payload.ResultCode);

            // Verify signature (bypass for supervisor payments in demo mode)
            var (purpose, projectId) = ExtractPurposeAndProjectId(payload.ExtraData);
            if (!string.Equals(purpose, "supervisor", StringComparison.OrdinalIgnoreCase))
            {
                ValidateWebhookSignature(payload);
            }
            else
            {
                _logger.LogWarning("[MoMo] Bypassing signature check for supervisor payment (demo mode)");
            }

            var tx = await _db.WalletTransactions.FirstOrDefaultAsync(x => x.MomoOrderId == payload.OrderId && x.MomoRequestId == payload.RequestId, ct);
            if (tx == null)
            {
                // idempotency: create if not exists
                var userId = ExtractUserId(payload.ExtraData);
                if (userId == Guid.Empty)
                {
                    _logger.LogError("[MoMo] Webhook failed: unable to extract userId from ExtraData for orderId={OrderId}", payload.OrderId);
                    throw new InvalidOperationException("Invalid ExtraData: unable to extract userId");
                }
                
                tx = new WalletTransaction
                {
                    UserId = userId,
                    MomoOrderId = payload.OrderId,
                    MomoRequestId = payload.RequestId,
                    Amount = payload.Amount,
                    Status = payload.ResultCode == 0 ? "SUCCEEDED" : "FAILED",
                    RawResponse = rawBody
                };
                _db.WalletTransactions.Add(tx);
            }
            else
            {
                tx.Status = payload.ResultCode == 0 ? "SUCCEEDED" : "FAILED";
                tx.RawResponse = rawBody;
            }

            if (payload.ResultCode == 0)
            {
                // credit wallet
                _logger.LogInformation("[MoMo] Webhook success, credit wallet: orderId={OrderId}, amount={Amount}", payload.OrderId, payload.Amount);
                var wallet = await _db.Wallets.FirstOrDefaultAsync(w => w.UserId == tx.UserId, ct);
                if (wallet == null)
                {
                    wallet = new Wallet { UserId = tx.UserId, Available = 0m };
                    _db.Wallets.Add(wallet);
                    await _db.SaveChangesAsync(ct);
                }

                wallet.Available += (decimal)payload.Amount;

                _db.LedgerEntries.Add(new LedgerEntry
                {
                    WalletId = wallet.Id,
                    Type = LedgerEntryType.Credit,
                    Amount = (decimal)payload.Amount,
                    RefId = payload.OrderId
                });

                // Handle purpose-specific logic (purpose and projectId already extracted above)
                _logger.LogInformation("[MoMo] Webhook purpose={Purpose}, projectId={ProjectId}", purpose, projectId);

                if (string.Equals(purpose, "commission", StringComparison.OrdinalIgnoreCase))
                {
                    await HandleCommissionPaymentAsync(payload, ct);
                }
                else if (string.Equals(purpose, "supervisor", StringComparison.OrdinalIgnoreCase))
                {
                    await HandleSupervisorPaymentAsync(payload, projectId, ct);
                }
            }

            await _db.SaveChangesAsync(ct);
        }

        private async Task HandleCommissionPaymentAsync(MomoWebhookDto payload, CancellationToken ct)
        {
            var contractId = ExtractContractId(payload.ExtraData);
            if (contractId == Guid.Empty) return;

            var contract = await _db.Contracts.Include(c => c.Escrow).FirstOrDefaultAsync(c => c.Id == contractId, ct);
            if (contract == null) return;

            // Check if this orderId has already been processed (idempotency check)
            var existingTransaction = await _db.PaymentTransactions
                .FirstOrDefaultAsync(t => t.ContractId == contractId 
                    && t.ProviderTxnId == payload.OrderId 
                    && t.Type == PaymentType.Fund
                    && t.Status == PaymentStatus.Succeeded, ct);
            
            if (existingTransaction != null)
            {
                _logger.LogWarning("[MoMo] Commission payment already processed for orderId={OrderId}, contractId={ContractId}. Skipping duplicate.", 
                    payload.OrderId, contractId);
                return; // Already processed, skip to prevent duplicate balance increase
            }

            if (contract.Escrow == null)
            {
                contract.Escrow = new EscrowAccount
                {
                    ContractId = contract.Id,
                    Provider = PaymentProvider.MoMo,
                    Status = EscrowStatus.Funded,
                    Balance = 0m,
                    ExternalAccountId = null
                };
                _db.EscrowAccounts.Add(contract.Escrow);
            }

            contract.Escrow.Balance += (decimal)payload.Amount;
            _db.PaymentTransactions.Add(new PaymentTransaction
            {
                ContractId = contract.Id,
                MilestoneId = null,
                Provider = PaymentProvider.MoMo,
                Type = PaymentType.Fund,
                Status = PaymentStatus.Succeeded,
                Amount = (decimal)payload.Amount,
                Description = "Phí môi giới",
                ProviderTxnId = payload.OrderId // Store OrderId to prevent duplicates
            });

            _logger.LogInformation("[MoMo] Escrow funded for contract={ContractId}, newBalance={Balance}", contract.Id, contract.Escrow.Balance);
        }

        private async Task HandleSupervisorPaymentAsync(MomoWebhookDto payload, Guid projectId, CancellationToken ct)
        {
            if (projectId == Guid.Empty)
            {
                _logger.LogError("[MoMo] Supervisor payment missing projectId in extraData");
                return;
            }

            // Update project payment status only (don't assign supervisor yet - that happens when contract is completed)
            try
            {
                var project = await _db.Projects
                    .FirstOrDefaultAsync(p => p.Id == projectId, ct);
                    
                if (project != null)
                {
                    project.SupervisorPackagePaymentStatus = PaymentStatus.Succeeded;
                    await _db.SaveChangesAsync(ct);
                    _logger.LogInformation("[MoMo] Updated project {ProjectId} payment status to Succeeded", projectId);
                    // Supervisor will be assigned when contract is completed (signed by both parties)
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[MoMo] Failed to update payment status for project={ProjectId}", projectId);
            }
        }

        private void ValidateWebhookSignature(MomoWebhookDto payload)
        {
            var rawSignature = $"accessKey={_momo.AccessKey}&amount={payload.Amount}&extraData={payload.ExtraData}&message={payload.Message}&orderId={payload.OrderId}&orderInfo={payload.OrderInfo}&orderType=momo_wallet&partnerCode={_momo.PartnerCode}&payType={payload.PayType}&requestId={payload.RequestId}&responseTime={payload.ResponseTime}&resultCode={payload.ResultCode}&transId={payload.TransId}";
            var expectedSignature = SignHmacSha256(rawSignature, _momo.SecretKey);
            
            if (!string.Equals(expectedSignature, payload.Signature, StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogWarning("[MoMo] Invalid signature for orderId={OrderId}", payload.OrderId);
                throw new UnauthorizedAccessException("Invalid webhook signature");
            }
        }

        private static Dictionary<string, string>? ParseExtraData(string extraData)
        {
            if (string.IsNullOrWhiteSpace(extraData)) return null;
            
            try
            {
                var bytes = Convert.FromBase64String(extraData);
                return System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string>>(bytes);
            }
            catch
            {
                return null;
            }
        }

        private static Guid ExtractUserId(string extraData)
        {
            var dict = ParseExtraData(extraData);
            return dict != null && dict.TryGetValue("userId", out var s) && Guid.TryParse(s, out var g) ? g : Guid.Empty;
        }

        private static Guid ExtractContractId(string extraData)
        {
            var dict = ParseExtraData(extraData);
            return dict != null && dict.TryGetValue("contractId", out var s) && Guid.TryParse(s, out var g) ? g : Guid.Empty;
        }

        private static (string purpose, Guid projectId) ExtractPurposeAndProjectId(string extraData)
        {
            var dict = ParseExtraData(extraData);
            if (dict == null) return (string.Empty, Guid.Empty);
            
            var purpose = dict.TryGetValue("purpose", out var p) ? p : string.Empty;
            var projectId = dict.TryGetValue("projectId", out var s) && Guid.TryParse(s, out var g) ? g : Guid.Empty;
            return (purpose, projectId);
        }

        public async Task<decimal> GetWalletBalanceAsync(Guid userId, CancellationToken ct = default)
        {
            var wallet = await _db.Wallets.FirstOrDefaultAsync(w => w.UserId == userId, ct);
            return wallet?.Available ?? 0m;
        }

        public async Task<bool> IsSupervisorPaymentPaidAsync(Guid userId, Guid projectId, CancellationToken ct = default)
        {
            if (projectId == Guid.Empty) return false;

            var project = await _db.Projects
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == projectId, ct);

            if (project == null || project.HomeownerId != userId)
                return false;

            return project.SupervisorPackagePaymentStatus == Domain.Enums.PaymentStatus.Succeeded;
        }

        public async Task<bool> IsCommissionPaidAsync(Guid userId, Guid contractId, CancellationToken ct = default)
        {
            // Logic: Nhà thầu luôn phải trả phí hoa hồng 1% dù dự án có đăng ký giám sát viên hay không
            // Chỉ kiểm tra xem commission đã được thanh toán chưa
            
            var paidCommission = await _db.PaymentTransactions
                .AsNoTracking()
                .AnyAsync(p => p.ContractId == contractId
                               && p.Description != null
                               && p.Description.ToLower() == "phí môi giới"
                               && p.Status == Domain.Enums.PaymentStatus.Succeeded, ct);
            return paidCommission;
        }

        private static string SignHmacSha256(string raw, string secret)
        {
            using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secret));
            var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(raw));
            return BitConverter.ToString(hash).Replace("-", string.Empty).ToLowerInvariant();
        }

        private static string RemoveDiacritics(string text)
        {
            if (string.IsNullOrEmpty(text)) return text;
            var normalized = text.Normalize(NormalizationForm.FormD);
            var sb = new StringBuilder();
            foreach (var c in normalized)
            {
                var unicodeCategory = System.Globalization.CharUnicodeInfo.GetUnicodeCategory(c);
                if (unicodeCategory != System.Globalization.UnicodeCategory.NonSpacingMark)
                {
                    sb.Append(c);
                }
            }
            return sb.ToString().Normalize(NormalizationForm.FormC);
        }
    }
}



