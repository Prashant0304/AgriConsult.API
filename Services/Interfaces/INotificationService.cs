namespace AgriConsult.API.Services.Interfaces
{
    public interface INotificationService
    {
        Task SendAsync(string recipient, string message);
        string ChannelName { get; }
    }
}
