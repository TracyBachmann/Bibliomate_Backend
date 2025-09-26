using System.Security.Claims;
using BackendBiblioMate.Controllers;
using BackendBiblioMate.Data;
using BackendBiblioMate.DTOs;
using BackendBiblioMate.Interfaces;
using BackendBiblioMate.Models;
using BackendBiblioMate.Models.Enums;
using BackendBiblioMate.Services.Infrastructure.Security;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Moq;

namespace UnitTestsBiblioMate.Controllers
{
    /// <summary>
    /// Unit tests for <see cref="NotificationsController"/>.
    /// Uses an in-memory EF Core context and mocks for real-time dispatch and logging services.
    /// </summary>
    public class NotificationsControllerTest : IDisposable
    {
        private readonly BiblioMateDbContext _context;
        private readonly Mock<INotificationService> _notifyMock;
        private readonly Mock<INotificationLogService> _logMock;
        private readonly NotificationsController _controller;

        private const int NormalUserId    = 1;
        private const int LibrarianUserId = 2;

        public NotificationsControllerTest()
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

            _context = new BiblioMateDbContext(options, encryptionService);

            _context.Users.AddRange(
                new User { UserId = NormalUserId,    FirstName = "User1", LastName = string.Empty },
                new User { UserId = LibrarianUserId, FirstName = "Lib1",  LastName = string.Empty }
            );

            _context.Notifications.AddRange(
                new Notification
                {
                    NotificationId = 10,
                    UserId         = NormalUserId,
                    Title          = "N1",
                    Message        = "Msg1",
                    User           = _context.Users.Find(NormalUserId)!
                },
                new Notification
                {
                    NotificationId = 20,
                    UserId         = LibrarianUserId,
                    Title          = "N2",
                    Message        = "Msg2",
                    User           = _context.Users.Find(LibrarianUserId)!
                }
            );
            _context.SaveChanges();

            _notifyMock = new Mock<INotificationService>();
            _logMock    = new Mock<INotificationLogService>();

            _controller = new NotificationsController(
                _context,
                _notifyMock.Object,
                _logMock.Object
            );
        }

        public void Dispose() => _context.Dispose();

        private void SetUser(int userId, params string[] roles)
        {
            var claims = new List<Claim> { new Claim(ClaimTypes.NameIdentifier, userId.ToString()) };
            claims.AddRange(roles.Select(r => new Claim(ClaimTypes.Role, r)));
            var identity = new ClaimsIdentity(claims, "test");
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(identity) }
            };
        }

        [Fact]
        public async Task GetNotifications_AsNormalUser_ReturnsOnlyOwn()
        {
            SetUser(NormalUserId, UserRoles.User);

            var action = await _controller.GetNotifications(CancellationToken.None);

            var ok = Assert.IsType<OkObjectResult>(action.Result);
            var list = Assert.IsAssignableFrom<IEnumerable<NotificationReadDto>>(ok.Value);
            Assert.All(list, dto => Assert.Equal(NormalUserId, dto.UserId));
        }

        [Fact]
        public async Task GetNotifications_AsLibrarian_ReturnsAll()
        {
            SetUser(LibrarianUserId, UserRoles.Librarian);

            var action = await _controller.GetNotifications(CancellationToken.None);

            var ok = Assert.IsType<OkObjectResult>(action.Result);
            var list = Assert.IsAssignableFrom<IEnumerable<NotificationReadDto>>(ok.Value);
            Assert.Equal(2, list.Count());
        }

        [Fact]
        public async Task GetNotification_NotFound_Returns404()
        {
            SetUser(NormalUserId, UserRoles.User);

            var action = await _controller.GetNotification(999, CancellationToken.None);

            Assert.IsType<NotFoundResult>(action.Result);
        }

        [Fact]
        public async Task GetNotification_ForbiddenForNormalUser_WhenOtherUser()
        {
            SetUser(NormalUserId, UserRoles.User);

            var action = await _controller.GetNotification(20, CancellationToken.None);

            Assert.IsType<ForbidResult>(action.Result);
        }

        [Fact]
        public async Task GetNotification_AsLibrarian_Returns200()
        {
            SetUser(LibrarianUserId, UserRoles.Librarian);

            var action = await _controller.GetNotification(10, CancellationToken.None);

            var ok  = Assert.IsType<OkObjectResult>(action.Result);
            var dto = Assert.IsType<NotificationReadDto>(ok.Value);
            Assert.Equal(10, dto.NotificationId);
        }

        [Fact]
        public async Task CreateNotification_AsLibrarian_CreatesAndDispatches()
        {
            SetUser(LibrarianUserId, UserRoles.Librarian);

            var dto = new NotificationCreateDto
            {
                UserId  = NormalUserId,
                Title   = "New",
                Message = "Hello"
            };

            var action = await _controller.CreateNotification(dto, CancellationToken.None);

            var created = Assert.IsType<CreatedAtActionResult>(action.Result);
            var body    = Assert.IsType<NotificationReadDto>(created.Value);
            Assert.Equal(dto.Title,   body.Title);
            Assert.Equal(dto.Message, body.Message);
            Assert.Equal(dto.UserId,  body.UserId);

            _notifyMock.Verify(s =>
                s.NotifyUser(dto.UserId, dto.Message, It.IsAny<CancellationToken>()),
                Times.Once);

            _logMock.Verify(s =>
                s.LogAsync(dto.UserId, NotificationType.Custom, dto.Message, It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task UpdateNotification_IdMismatch_ReturnsBadRequest()
        {
            SetUser(LibrarianUserId, UserRoles.Admin);

            var dto = new NotificationUpdateDto
            {
                NotificationId = 123,
                UserId         = NormalUserId,
                Title          = "X",
                Message        = "Y"
            };

            var action = await _controller.UpdateNotification(999, dto, CancellationToken.None);

            Assert.IsType<BadRequestResult>(action);
        }

        [Fact]
        public async Task UpdateNotification_NotFound_Returns404()
        {
            SetUser(LibrarianUserId, UserRoles.Admin);

            var dto = new NotificationUpdateDto
            {
                NotificationId = 999,
                UserId         = NormalUserId,
                Title          = "X",
                Message        = "Y"
            };

            var action = await _controller.UpdateNotification(999, dto, CancellationToken.None);

            Assert.IsType<NotFoundResult>(action);
        }

        [Fact]
        public async Task UpdateNotification_WhenExists_ReturnsNoContent()
        {
            SetUser(LibrarianUserId, UserRoles.Librarian);

            var existingId = 10;
            var dto = new NotificationUpdateDto
            {
                NotificationId = existingId,
                UserId         = LibrarianUserId,
                Title          = "Updated",
                Message        = "Msg"
            };

            var action = await _controller.UpdateNotification(existingId, dto, CancellationToken.None);

            Assert.IsType<NoContentResult>(action);

            var updated = await _context.Notifications.FindAsync(existingId);
            Assert.Equal(dto.Title,   updated!.Title);
            Assert.Equal(dto.Message, updated.Message);
            Assert.Equal(dto.UserId,  updated.UserId);
        }

        [Fact]
        public async Task DeleteNotification_NotFound_Returns404()
        {
            SetUser(LibrarianUserId, UserRoles.Admin);

            var action = await _controller.DeleteNotification(999, CancellationToken.None);

            Assert.IsType<NotFoundResult>(action);
        }

        [Fact]
        public async Task DeleteNotification_WhenExists_ReturnsNoContent()
        {
            SetUser(LibrarianUserId, UserRoles.Librarian);

            var toDeleteId = 20;
            var action     = await _controller.DeleteNotification(toDeleteId, CancellationToken.None);

            Assert.IsType<NoContentResult>(action);
            Assert.Null(await _context.Notifications.FindAsync(toDeleteId));
        }
    }
}

