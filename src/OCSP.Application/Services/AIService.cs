public class AIService : IAIService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AIService> _logger;
    
    public AIService(HttpClient httpClient, IConfiguration configuration, ILogger<AIService> logger)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        _logger = logger;
        
        var aiServiceUrl = _configuration["AIService:BaseUrl"] ?? "http://localhost:8000";
        _httpClient.BaseAddress = new Uri(aiServiceUrl);
        _httpClient.DefaultRequestHeaders.Add("X-API-Key", _configuration["AIService:ApiKey"]);
    }
    
    public async Task<AIResponse> GetChatResponseAsync(ChatRequest request)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("/api/v1/chat/message", request);
            response.EnsureSuccessStatusCode();
            
            var result = await response.Content.ReadFromJsonAsync<AIResponse>();
            return result;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Failed to get AI response for user {UserId}", request.UserId);
            
            // Fallback response
            return new AIResponse
            {
                Message = "Xin lỗi, hiện tại dịch vụ AI tạm thời không khả dụng. Vui lòng thử lại sau.",
                UserId = request.UserId,
                ConfidenceScore = 0.0,
                Timestamp = DateTime.UtcNow
            };
        }
    }
    
    public async Task<bool> IsServiceHealthyAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync("/health");
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }
}