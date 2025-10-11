using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
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

        public PaymentService(ApplicationDbContext db, MomoOptions momo, HttpClient http)
        {
            _db = db;
            _momo = momo;
            _http = http;
        }

        public async Task<MomoCreatePaymentResultDto> CreateMomoPaymentAsync(MomoCreatePaymentDto dto, Guid userId, CancellationToken ct = default)
        {
            if (dto.Amount <= 0) throw new ArgumentException("Amount must be > 0");

            var wallet = await _db.Wallets.FirstOrDefaultAsync(w => w.UserId == userId, ct);
            if (wallet == null)
            {
                wallet = new Wallet { UserId = userId, Available = 0m };
                _db.Wallets.Add(wallet);
                await _db.SaveChangesAsync(ct);
            }

            var orderId = $"{_momo.PartnerCode}-{Guid.NewGuid():N}";
            var requestId = Guid.NewGuid().ToString("N");
            var amount = (long)Math.Round(dto.Amount, 0);

            // Carry userId and optional contractId so webhook can correlate and FE can return correctly
            var extra = new Dictionary<string, string>
            {
                ["userId"] = userId.ToString(),
                ["contractId"] = dto.ContractId?.ToString() ?? string.Empty
            };
            var extraData = System.Convert.ToBase64String(System.Text.Json.JsonSerializer.SerializeToUtf8Bytes(extra));
            var requestType = "payWithMethod"; // align with MoMo AIO v2 sample
            var partnerName = "Test";
            var storeId = "MomoTestStore";
            var autoCapture = true;
            var orderGroupId = string.Empty;
            var body = new Dictionary<string, object>
            {
                ["partnerCode"] = _momo.PartnerCode,
                ["accessKey"] = _momo.AccessKey,
                ["requestId"] = requestId,
                ["amount"] = amount,
                ["orderId"] = orderId,
                ["orderInfo"] = dto.Description ?? "OCSP Wallet Topup",
                ["redirectUrl"] = _momo.RedirectUrl,
                ["ipnUrl"] = _momo.IpnUrl,
                ["requestType"] = requestType,
                ["extraData"] = extraData,
                ["lang"] = "vi",
                ["partnerName"] = partnerName,
                ["storeId"] = storeId,
                ["autoCapture"] = autoCapture,
                ["orderGroupId"] = orderGroupId,
            };

            // AIO v2 signature format (no URL-encode in raw string)
            var rawSignature = $"accessKey={_momo.AccessKey}&amount={amount}&extraData={extraData}&ipnUrl={_momo.IpnUrl}&orderId={orderId}&orderInfo={(string)body["orderInfo"]}&partnerCode={_momo.PartnerCode}&redirectUrl={_momo.RedirectUrl}&requestId={requestId}&requestType={requestType}";
            var signature = SignHmacSha256(rawSignature, _momo.SecretKey);
            body["signature"] = signature;

            using var req = new HttpRequestMessage(HttpMethod.Post, _momo.Endpoint)
            {
                Content = JsonContent.Create(body)
            };
            req.Headers.Accept.Clear();
            req.Headers.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

            var resp = await _http.SendAsync(req, ct);
            var respText = await resp.Content.ReadAsStringAsync(ct);
            if (!resp.IsSuccessStatusCode)
            {
                throw new InvalidOperationException($"MoMo create payment failed ({(int)resp.StatusCode}): {respText}");
            }
            var json = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(respText) ?? new();

            var payUrl = json.TryGetValue("payUrl", out var v) ? v?.ToString() ?? string.Empty : string.Empty;
            if (string.IsNullOrEmpty(payUrl)) throw new InvalidOperationException("MoMo didn't return payUrl");

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

            return new MomoCreatePaymentResultDto { PayUrl = payUrl, OrderId = orderId, RequestId = requestId };
        }

        public async Task HandleMomoWebhookAsync(MomoWebhookDto payload, string rawBody, CancellationToken ct = default)
        {
            // Verify signature per MoMo spec
            var rawSignature = $"accessKey={_momo.AccessKey}&amount={payload.Amount}&extraData={payload.ExtraData}&message={payload.Message}&orderId={payload.OrderId}&orderInfo={payload.OrderInfo}&orderType=momo_wallet&partnerCode={_momo.PartnerCode}&payType={payload.PayType}&requestId={payload.RequestId}&responseTime={payload.ResponseTime}&resultCode={payload.ResultCode}&transId={payload.TransId}";
            var expected = SignHmacSha256(rawSignature, _momo.SecretKey);
            if (!string.Equals(expected, payload.Signature, StringComparison.OrdinalIgnoreCase))
                throw new UnauthorizedAccessException("Invalid signature");

            var tx = await _db.WalletTransactions.FirstOrDefaultAsync(x => x.MomoOrderId == payload.OrderId && x.MomoRequestId == payload.RequestId, ct);
            if (tx == null)
            {
                // idempotency: create if not exists
                tx = new WalletTransaction
                {
                    UserId = ExtractUserId(payload.ExtraData),
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

                // Optional: auto-topup contract escrow if contractId present
                var contractId = ExtractContractId(payload.ExtraData);
                if (contractId != Guid.Empty)
                {
                var contract = await _db.Contracts.Include(c => c.Escrow).FirstOrDefaultAsync(c => c.Id == contractId, ct);
                if (contract != null)
                {
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
                        Description = "Topup via MoMo"
                    });
                }
                }
            }

            await _db.SaveChangesAsync(ct);
        }

        private static Guid ExtractUserId(string extra)
        {
            try
            {
                var decoded = Uri.UnescapeDataString(extra ?? string.Empty);
                Dictionary<string, string>? dict = null;
                try
                {
                    var bytes = Convert.FromBase64String(decoded);
                    dict = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string>>(bytes);
                }
                catch
                {
                    // maybe plain JSON
                    dict = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string>>(decoded);
                }
                return dict != null && dict.TryGetValue("userId", out var s) && Guid.TryParse(s, out var g) ? g : Guid.Empty;
            }
            catch { return Guid.Empty; }
        }

        private static Guid ExtractContractId(string extra)
        {
            try
            {
                var decoded = Uri.UnescapeDataString(extra ?? string.Empty);
                Dictionary<string, string>? dict = null;
                try
                {
                    var bytes = Convert.FromBase64String(decoded);
                    dict = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string>>(bytes);
                }
                catch
                {
                    // maybe plain JSON
                    dict = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string>>(decoded);
                }
                return dict != null && dict.TryGetValue("contractId", out var s) && Guid.TryParse(s, out var g) ? g : Guid.Empty;
            }
            catch { return Guid.Empty; }
        }

        private static string SignHmacSha256(string raw, string secret)
        {
            using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secret));
            var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(raw));
            return BitConverter.ToString(hash).Replace("-", string.Empty).ToLowerInvariant();
        }
    }
}



