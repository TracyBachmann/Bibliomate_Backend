using BackendBiblioMate.Data;
using BackendBiblioMate.Hubs;
using BackendBiblioMate.Interfaces;
using BackendBiblioMate.Models;
using BackendBiblioMate.Models.Enums;
using BackendBiblioMate.Services.Infrastructure.Security;
using BackendBiblioMate.Services.Notifications;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using NSubstitute;

namespace UnitTestsBiblioMate.Services.Notifications
{
    /// <summary>
    /// Tests for <see cref="NotificationService"/>.
    /// Verifies SignalR + email behavior when user exists / does not exist.
    /// </summary>
    public class NotificationServiceTests : IDisposable
    {
        private readonly BiblioMateDbContext _db;
        private readonly IClientProxy        _clientProxy;
        private readonly IEmailService       _emailService;
        private readonly NotificationService _service;

        public NotificationServiceTests()
        {
            // In-memory EF + encryption key
            var options = new DbContextOptionsBuilder<BiblioMateDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            var cfg = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["Encryption:Key"] = Convert.ToBase64String(
                        System.Text.Encoding.UTF8.GetBytes("12345678901234567890123456789012"))
                })
                .Build();

            var encryption = new EncryptionService(cfg);
            _db = new BiblioMateDbContext(options, encryption);

            // SignalR hub context substitution
            var hubContext = Substitute.For<IHubContext<NotificationHub>>();
            var clients    = Substitute.For<IHubClients>();
            _clientProxy   = Substitute.For<IClientProxy>();

            // Return the same proxy for any user id
            clients.User(Arg.Any<string>()).Returns(_clientProxy);
            hubContext.Clients.Returns(clients);

            // Email service substitution
            _emailService = Substitute.For<IEmailService>();

            _service = new NotificationService(hubContext, _emailService, _db);
        }

        [Fact]
        public async Task NotifyUser_UserExists_SendsHubAndEmail()
        {
            // Arrange
            _db.Users.Add(new User
            {
                UserId          = 1,
                FirstName       = "Test",
                LastName        = "User",
                Email           = "test@example.com",
                Password        = "hashed",
                Address1        = "1 Test Street",
                Phone           = "0600000000",
                Role            = UserRoles.User,
                IsEmailConfirmed = true,
                IsApproved       = true,
                SecurityStamp    = Guid.NewGuid().ToString()
            });
            await _db.SaveChangesAsync();

            const string message = "Hello, world!";

            // Act
            await _service.NotifyUser(1, message);

            // Assert: SignalR called once with method and message
            await _clientProxy.Received(1).SendCoreAsync(
                "ReceiveNotification",
                Arg.Is<object?[]>(args => args != null && args.Length >= 1 && (args[0] as string) == message),
                Arg.Any<CancellationToken>());

            // Assert: email sent once to user's address with same message
            await _emailService.Received(1).SendEmailAsync(
                "test@example.com",
                "BiblioMate Notification",
                message);
        }

        [Fact]
        public async Task NotifyUser_UserDoesNotExist_DoesNothing()
        {
            // Act
            await _service.NotifyUser(42, "nobody");

            // Assert: no SignalR calls
            await _clientProxy.DidNotReceiveWithAnyArgs()
                .SendCoreAsync(default!, default!, default);

            // Assert: no email calls
            await _emailService.DidNotReceiveWithAnyArgs()
                .SendEmailAsync(default!, default!, default!);
        }

        public void Dispose() => _db.Dispose();
    }
}
