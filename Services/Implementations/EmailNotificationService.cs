using AgriConsult.API.Services.Interfaces;

namespace AgriConsult.API.Services.Implementations
{
    public class EmailNotificationService : INotificationService
    {
        public string ChannelName => "Email";

        public async Task SendAsync(string recipent, string message)
        {
            Console.WriteLine($"[Email] To :{recipent} | Message : {message}");
            await Task.CompletedTask;
        }
    }
}
