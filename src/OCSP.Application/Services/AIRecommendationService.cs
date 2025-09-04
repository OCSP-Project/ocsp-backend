using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json; // cần package Newtonsoft.Json
using OCSP.Application.DTOs.Contractor;
using OCSP.Application.Services.Interfaces;
using OCSP.Domain.Entities;

namespace OCSP.Application.Services
{
    public class AIRecommendationService : IAIRecommendationService
    {
        private readonly ILogger<AIRecommendationService> _logger;
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;

        private readonly string _geminiApiKey;
        private readonly string _geminiApiUrl;

        public AIRecommendationService(
            ILogger<AIRecommendationService> logger,
            HttpClient httpClient,
            IConfiguration configuration)
        {
            _logger = logger;
            _httpClient = httpClient;
            _configuration = configuration;

            _geminiApiKey = configuration["Gemini:ApiKey"] ?? "";
            _geminiApiUrl = configuration["Gemini:ApiUrl"]
                            ?? "https://generativelanguage.googleapis.com/v1/models/gemini-pro:generateContent";
        }

        public async Task<decimal> CalculateMatchScoreAsync(Contractor contractor, AIRecommendationRequestDto request)
        {
            try
            {
                decimal score = 0;

                // Location (30%)
                if (!string.IsNullOrWhiteSpace(request.PreferredLocation) &&
                    ((contractor.City ?? "").Contains(request.PreferredLocation, StringComparison.OrdinalIgnoreCase) ||
                     (contractor.Province ?? "").Contains(request.PreferredLocation, StringComparison.OrdinalIgnoreCase)))
                {
                    score += 30;
                }

                // Budget (25%)
                if (request.Budget.HasValue)
                {
                    if (contractor.MinProjectBudget.HasValue && contractor.MaxProjectBudget.HasValue)
                    {
                        if (request.Budget >= contractor.MinProjectBudget && request.Budget <= contractor.MaxProjectBudget)
                            score += 25;
                        else if (request.Budget >= contractor.MinProjectBudget * 0.8m &&
                                 request.Budget <= contractor.MaxProjectBudget * 1.2m)
                            score += 15;
                    }
                    else
                    {
                        score += 10;
                    }
                }

                // Specialty (20%)
                if (request.RequiredSpecialties?.Any() == true)
                {
                    var spec = contractor.Specialties.Select(s => s.SpecialtyName.ToLowerInvariant()).ToList();
                    var matched = request.RequiredSpecialties.Count(rs =>
                        spec.Any(cs => cs.Contains(rs.ToLowerInvariant())));
                    if (matched > 0)
                        score += (decimal)matched / request.RequiredSpecialties.Count * 20m;
                }

                // Exp + Rating (15%)
                var expScore = Math.Min(contractor.YearsOfExperience / 10m, 1m) * 7.5m;
                var ratingScore = contractor.AverageRating / 5m * 7.5m;
                score += expScore + ratingScore;

                // Verified/Premium/Completed (10%)
                if (contractor.IsVerified) score += 5;
                if (contractor.IsPremium) score += 3;
                if (contractor.CompletedProjects > 10) score += 2;

                // AI semantic (30% trộn)
                var aiScore = await GetAISemanticMatchScore(contractor, request.ProjectDescription);
                score = (score * 0.7m) + (aiScore * 0.3m);

                return Math.Round(Math.Clamp(score, 0, 100), 2);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating match score for contractor {ContractorId}", contractor.Id);
                return 0;
            }
        }

        public async Task<string> GenerateMatchReasonAsync(Contractor contractor, AIRecommendationRequestDto request)
        {
            try
            {
                var prompt = $@"
Tạo lý do khuyến nghị nhà thầu này cho dự án xây dựng. Viết bằng tiếng Việt, ngắn gọn (2–3 câu):

Nhà thầu:
- Tên: {contractor.CompanyName}
- Kinh nghiệm: {contractor.YearsOfExperience} năm
- Đánh giá: {contractor.AverageRating}/5 ({contractor.TotalReviews} đánh giá)
- Dự án hoàn thành: {contractor.CompletedProjects}
- Chuyên môn: {string.Join(", ", contractor.Specialties.Select(s => s.SpecialtyName))}
- Khu vực: {contractor.City}, {contractor.Province}
- Khoảng ngân sách: {contractor.MinProjectBudget:C0} - {contractor.MaxProjectBudget:C0}

Yêu cầu dự án:
- Mô tả: {request.ProjectDescription}
- Ngân sách: {request.Budget:C0}
- Địa điểm: {request.PreferredLocation}
- Chuyên môn cần: {string.Join(", ", request.RequiredSpecialties ?? new List<string>())}
";
                var ai = await CallGeminiAPI(prompt);
                return string.IsNullOrWhiteSpace(ai) ? GenerateFallbackReason(contractor, request) : ai;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating match reason");
                return GenerateFallbackReason(contractor, request);
            }
        }

        public Task<List<string>> GetMatchingFactorsAsync(Contractor contractor, AIRecommendationRequestDto request)
        {
            var factors = new List<string>();

            if (!string.IsNullOrWhiteSpace(request.PreferredLocation) &&
                (contractor.City ?? "").Contains(request.PreferredLocation, StringComparison.OrdinalIgnoreCase))
                factors.Add($"Hoạt động tại {contractor.City}");

            if (request.Budget.HasValue && contractor.MinProjectBudget.HasValue && contractor.MaxProjectBudget.HasValue &&
                request.Budget >= contractor.MinProjectBudget && request.Budget <= contractor.MaxProjectBudget)
                factors.Add("Ngân sách phù hợp");

            if (contractor.YearsOfExperience >= 5)
                factors.Add($"{contractor.YearsOfExperience} năm kinh nghiệm");

            if (contractor.AverageRating >= 4.0m)
                factors.Add($"Đánh giá {contractor.AverageRating}/5");

            if (contractor.CompletedProjects >= 10)
                factors.Add($"{contractor.CompletedProjects} dự án hoàn thành");

            if (contractor.IsVerified) factors.Add("Đã xác thực");
            if (contractor.IsPremium) factors.Add("Tài khoản Premium");

            if (request.RequiredSpecialties?.Any() == true)
            {
                var contractorSpec = contractor.Specialties.Select(s => s.SpecialtyName).ToList();
                var matched = request.RequiredSpecialties
                    .Where(rs => contractorSpec.Any(cs => cs.Contains(rs, StringComparison.OrdinalIgnoreCase)))
                    .ToList();
                if (matched.Any())
                    factors.Add($"Chuyên về {string.Join(", ", matched)}");
            }

            return Task.FromResult(factors.Take(5).ToList());
        }

        public async Task<List<string>> ExtractProjectRequirementsAsync(string projectDescription)
        {
            try
            {
                var prompt = $@"
Phân tích mô tả dự án xây dựng và trích xuất yêu cầu chính. Trả về dạng danh sách ngắn gọn, mỗi dòng một yêu cầu:
- Loại công trình
- Chuyên môn cần thiết
- Quy mô dự án
- Yêu cầu đặc biệt

Mô tả: {projectDescription}
";
                var ai = await CallGeminiAPI(prompt);
                if (string.IsNullOrWhiteSpace(ai)) return new();

                return ai.Split('\n', StringSplitOptions.RemoveEmptyEntries)
                    .Select(x => x.Trim().TrimStart('-', '*', '•', '1', '2', '3', '4', '5', '.', ' '))
                    .Where(x => !string.IsNullOrWhiteSpace(x))
                    .Take(10)
                    .ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error extracting project requirements");
                return new();
            }
        }

        public async Task<List<Contractor>> RankContractorsByCompatibilityAsync(List<Contractor> contractors, AIRecommendationRequestDto request)
        {
            var tuples = new List<(Contractor c, decimal score)>(contractors.Count);
            foreach (var c in contractors)
            {
                var score = await CalculateMatchScoreAsync(c, request);
                tuples.Add((c, score));
            }
            return tuples.OrderByDescending(t => t.score).Select(t => t.c).ToList();
        }

        private async Task<decimal> GetAISemanticMatchScore(Contractor contractor, string projectDescription)
        {
            try
            {
                var info = $"Nhà thầu: {contractor.CompanyName}; " +
                           $"Chuyên môn: {string.Join(", ", contractor.Specialties.Select(s => s.SpecialtyName))}; " +
                           $"Mô tả: {contractor.Description}.";

                var prompt = $@"
Đánh giá mức độ phù hợp giữa nhà thầu và dự án (0-100). Chỉ trả về con số:
{info}
Dự án: {projectDescription}
";
                var text = await CallGeminiAPI(prompt);
                if (!string.IsNullOrWhiteSpace(text) && decimal.TryParse(text.Trim(), out var num))
                    return Math.Clamp(num, 0, 100);

                return 50;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting AI semantic score");
                return 50;
            }
        }

        private async Task<string?> CallGeminiAPI(string prompt)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(_geminiApiKey))
                {
                    _logger.LogWarning("Gemini API key not configured");
                    return null;
                }

                var requestBody = new
                {
                    contents = new[]
                    {
                        new
                        {
                            parts = new[] { new { text = prompt } }
                        }
                    },
                    generationConfig = new
                    {
                        temperature = 0.7,
                        topK = 40,
                        topP = 0.95,
                        maxOutputTokens = 512
                    }
                };

                _httpClient.DefaultRequestHeaders.Clear();
                _httpClient.DefaultRequestHeaders.Add("x-goog-api-key", _geminiApiKey);

                var resp = await _httpClient.PostAsJsonAsync(_geminiApiUrl, requestBody);
                if (!resp.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Gemini call failed: {StatusCode}", resp.StatusCode);
                    return null;
                }

                var json = await resp.Content.ReadAsStringAsync();
                dynamic? obj = JsonConvert.DeserializeObject(json);
                return obj?.candidates?[0]?.content?.parts?[0]?.text?.ToString();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "CallGeminiAPI error");
                return null;
            }
        }

        private string GenerateFallbackReason(Contractor contractor, AIRecommendationRequestDto request)
        {
            var reasons = new List<string>();
            if (contractor.IsVerified) reasons.Add("đã được xác thực");
            if (contractor.AverageRating >= 4.5m) reasons.Add($"đánh giá xuất sắc ({contractor.AverageRating}/5)");
            else if (contractor.AverageRating >= 4.0m) reasons.Add($"đánh giá tốt ({contractor.AverageRating}/5)");
            if (contractor.YearsOfExperience >= 10) reasons.Add($"{contractor.YearsOfExperience} năm kinh nghiệm");
            if (contractor.CompletedProjects >= 10) reasons.Add($"{contractor.CompletedProjects} dự án hoàn thành");
            if (!string.IsNullOrWhiteSpace(request.PreferredLocation) &&
                (contractor.City ?? "").Contains(request.PreferredLocation, StringComparison.OrdinalIgnoreCase))
                reasons.Add($"hoạt động tại {contractor.City}");

            var text = reasons.Any()
                ? $"{contractor.CompanyName} là lựa chọn phù hợp vì " + string.Join(", ", reasons.Take(3)) + "."
                : $"{contractor.CompanyName} có thể đáp ứng yêu cầu dự án của bạn.";
            return text;
        }
    }
}
