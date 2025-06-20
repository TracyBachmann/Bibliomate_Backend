using Microsoft.AspNetCore.SignalR;
using backend.Hubs;

namespace backend.Services
{
    public class NotificationService
    {
        private readonly IHubContext<NotificationHub> _hubContext;

        public NotificationService(IHubContext<NotificationHub> hubContext)
        {
            _hubContext = hubContext;
        }

        public async Task CreateAndSendAsync(string message, string type = "info")
        {
            var payload = new
            {
                type,
                message,
                timestamp = DateTime.UtcNow
            };

            await _hubContext.Clients.All.SendAsync("ReceiveNotification", payload);
        }
    }
}