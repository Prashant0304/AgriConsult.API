using AgriConsult.API.Services.Interfaces;

namespace AgriConsult.API.Services.Implementations
{
    public class SmsNotificationService:INotificationService
    {
        public string ChannelName => "SMS";
        public async Task SendAsync(string recipent, string message)
        {
            Console.WriteLine($"[SMS] To :{recipent} | Message : {message}");
            await Task.CompletedTask;
        }

    }
}
