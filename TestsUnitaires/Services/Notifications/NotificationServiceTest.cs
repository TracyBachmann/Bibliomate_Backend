using BackendBiblioMate.Data;
using BackendBiblioMate.Hubs;
using BackendBiblioMate.Interfaces;
using BackendBiblioMate.Models;
using BackendBiblioMate.Services.Notifications;
using BackendBiblioMate.Services.Infrastructure.Security;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using NSubstitute;

namespace TestsUnitaires.Services.Notifications
{
    /// <summary>
    /// Unit tests for <see cref="NotificationService"/>,
    /// verifying that notifications are sent via SignalR and email
    /// only when the user exists in the database.
    /// </summary>
    public class NotificationServiceTests : IDisposable
    {
        private readonly BiblioMateDbContext _db;
        private readonly IClientProxy        _clientProxy;
        private readonly IEmailService       _emailService;
        private readonly NotificationService _service;

        public NotificationServiceTests()
        {
            // Set up in-memory EF Core with encryption service
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

            // Mock SignalR hub context and client proxy
            var hubContext  = Substitute.For<IHubContext<NotificationHub>>();
            var clients     = Substitute.For<IHubClients>();
            _clientProxy    = Substitute.For<IClientProxy>();
            clients.User("1").Returns(_clientProxy);
            hubContext.Clients.Returns(clients);

            // Mock email service
            _emailService = Substitute.For<IEmailService>();

            _service = new NotificationService(hubContext, _emailService, _db);
        }

        /// <summary>
        /// When the user exists, NotifyUser should
        /// - send a SignalR message once via IClientProxy
        /// - send an email once via IEmailService
        /// </summary>
        [Fact]
        public async Task NotifyUser_UserExists_SendsHubAndEmail()
        {
            // Arrange: create a user with ID=1
            _db.Users.Add(new User { UserId = 1, Email = "test@example.com", Name = "Test" });
            await _db.SaveChangesAsync();

            const string message = "Hello, world!";

            // Act
            await _service.NotifyUser(1, message);

            // Assert: SignalR invoked exactly once
            await _clientProxy
                .Received(1)
                .SendCoreAsync(
                    "ReceiveNotification",
                    Arg.Any<object[]>(),
                    Arg.Any<CancellationToken>());

            // Assert: Email sent exactly once to the correct address
            await _emailService
                .Received(1)
                .SendEmailAsync(
                    "test@example.com",
                    "BiblioMate Notification",
                    message);
        }

        /// <summary>
        /// When the user does not exist, NotifyUser should do nothing:
        /// - no SignalR messages
        /// - no emails sent
        /// </summary>
        [Fact]
        public async Task NotifyUser_UserDoesNotExist_DoesNothing()
        {
            // Act
            await _service.NotifyUser(42, "nobody");

            // Assert: no SignalR calls
            await _clientProxy
                .ReceivedWithAnyArgs(0)
                .SendCoreAsync(default!, default!);

            // Assert: no email calls
            await _emailService
                .ReceivedWithAnyArgs(0)
                .SendEmailAsync(default!, default!, default!);
        }

        public void Dispose() => _db.Dispose();
    }
}