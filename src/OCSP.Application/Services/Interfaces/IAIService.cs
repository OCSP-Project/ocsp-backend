public interface IAIService
{
    Task<AIResponse> GetChatResponseAsync(ChatRequest request);
    Task<List<ContractorRecommendation>> GetContractorRecommendationsAsync(RecommendationRequest request);
    Task<bool> IsServiceHealthyAsync();
}

public class ChatRequest
{
    public string Message { get; set; }
    public string UserId { get; set; }
    public UserContext UserContext { get; set; }
    public List<ChatMessage> ChatHistory { get; set; } = new();
}

public class UserContext
{
    public string UserId { get; set; }
    public string UserRole { get; set; }
    public string CurrentProjectId { get; set; }
    public string Location { get; set; } = "Da Nang";
    public Dictionary<string, object> Preferences { get; set; } = new();
}

public class AIResponse
{
    public string Message { get; set; }
    public string UserId { get; set; }
    public double ConfidenceScore { get; set; }
    public List<RelevantDocument> RelevantDocs { get; set; } = new();
    public DateTime Timestamp { get; set; }
    public int ProcessingTimeMs { get; set; }
}