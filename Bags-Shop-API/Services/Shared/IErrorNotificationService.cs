namespace Bags_Shop_API.Services.Shared
{
    public interface IErrorNotificationService
    {
        Task SendErrorNotificationAsync(string errorMessage, string? stackTrace = null);
    }
}
