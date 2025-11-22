namespace OCSP.Application.Services.Interfaces
{
    public interface INotificationService
    {
        Task CreateNotificationAsync(Guid userId, string title, string message, CancellationToken ct = default);
    }
}