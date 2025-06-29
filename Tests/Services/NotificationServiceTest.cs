using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using NSubstitute;
using backend.Data;
using backend.Hubs;
using backend.Models;
using backend.Services;

namespace Tests.Services
{
    public class NotificationServiceTests : IDisposable
    {
        private readonly BiblioMateDbContext _db;
        private readonly IClientProxy        _clientProxy;
        private readonly IEmailService       _emailService;
        private readonly NotificationService _service;

        public NotificationServiceTests()
        {
            var options = new DbContextOptionsBuilder<BiblioMateDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            var encryptionConfig = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["Encryption:Key"] = Convert.ToBase64String(
                        System.Text.Encoding.UTF8.GetBytes("12345678901234567890123456789012"))
                })
                .Build();
            var encryptionService = new EncryptionService(encryptionConfig);

            _db = new BiblioMateDbContext(options, encryptionService);

            var hubContext  = Substitute.For<IHubContext<NotificationHub>>();
            var clients     = Substitute.For<IHubClients>();
            _clientProxy    = Substitute.For<IClientProxy>();
            clients.User("1").Returns(_clientProxy);
            hubContext.Clients.Returns(clients);

            _emailService = Substitute.For<IEmailService>();

            _service = new NotificationService(hubContext, _emailService, _db);
        }

        [Fact]
        public async Task NotifyUser_UserExists_SendsHubAndEmail()
        {
            _db.Users.Add(new User { UserId = 1, Email = "test@example.com", Name = "Test" });
            await _db.SaveChangesAsync();

            const string message = "Hello, world!";
            await _service.NotifyUser(1, message);

            await _clientProxy
                .Received(1)
                .SendCoreAsync(
                    "ReceiveNotification",
                    Arg.Any<object[]>(),
                    Arg.Any<CancellationToken>());

            await _emailService
                .Received(1)
                .SendEmailAsync(
                    "test@example.com",
                    "BiblioMate Notification",
                    message);
        }

        [Fact]
        public async Task NotifyUser_UserDoesNotExist_DoesNothing()
        {
            await _service.NotifyUser(42, "nobody");

            await _clientProxy
                .ReceivedWithAnyArgs(0)
                .SendCoreAsync(default!, default!, default);

            await _emailService
                .ReceivedWithAnyArgs(0)
                .SendEmailAsync(default!, default!, default!);
        }

        public void Dispose() => _db.Dispose();
    }
}
