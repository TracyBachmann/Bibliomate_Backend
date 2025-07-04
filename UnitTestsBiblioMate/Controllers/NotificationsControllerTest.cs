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

        private const int NormalUserId = 1;
        private const int LibrarianUserId = 2;

        /// <summary>
        /// Initializes a new instance of <see cref="NotificationsControllerTest"/>.
        /// Sets up an in-memory database seeded with users and notifications,
        /// and mocks the notification and log services.
        /// </summary>
        public NotificationsControllerTest()
        {
            // Build EF Core in-memory options
            var options = new DbContextOptionsBuilder<BiblioMateDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            // Provide a dummy EncryptionService (required by the DbContext)
            var encryptionConfig = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["Encryption:Key"] = Convert.ToBase64String(
                        System.Text.Encoding.UTF8.GetBytes("12345678901234567890123456789012"))
                })
                .Build();
            var encryptionService = new EncryptionService(encryptionConfig);

            _context = new BiblioMateDbContext(options, encryptionService);

            // Seed users
            _context.Users.AddRange(
                new User { UserId = NormalUserId, Name = "User1" },
                new User { UserId = LibrarianUserId, Name = "Lib1" }
            );

            // Seed notifications
            _context.Notifications.AddRange(
                new Notification
                {
                    NotificationId = 10,
                    UserId = NormalUserId,
                    Title = "N1",
                    Message = "Msg1",
                    User = _context.Users.Find(NormalUserId)!
                },
                new Notification
                {
                    NotificationId = 20,
                    UserId = LibrarianUserId,
                    Title = "N2",
                    Message = "Msg2",
                    User = _context.Users.Find(LibrarianUserId)!
                }
            );
            _context.SaveChanges();

            // Create mocks
            _notifyMock = new Mock<INotificationService>();
            _logMock    = new Mock<INotificationLogService>();

            // Instantiate controller
            _controller = new NotificationsController(
                _context,
                _notifyMock.Object,
                _logMock.Object
            );
        }

        /// <summary>
        /// Tears down in-memory context.
        /// </summary>
        public void Dispose() => _context.Dispose();

        /// <summary>
        /// Configures the controller's HttpContext with a user identity.
        /// </summary>
        /// <param name="userId">The user ID to set on the claims.</param>
        /// <param name="roles">Optional roles to include in the claims.</param>
        private void SetUser(int userId, params string[] roles)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, userId.ToString())
            };
            claims.AddRange(roles.Select(r => new Claim(ClaimTypes.Role, r)));
            var identity = new ClaimsIdentity(claims, "test");
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(identity)
                }
            };
        }

        /// <summary>
        /// When called by a normal user, GetNotifications should return only that user's notifications.
        /// </summary>
        [Fact]
        public async Task GetNotifications_AsNormalUser_ReturnsOnlyOwn()
        {
            SetUser(NormalUserId, UserRoles.User);

            var action = await _controller.GetNotifications(CancellationToken.None);

            var ok = Assert.IsType<OkObjectResult>(action.Result);
            var list = Assert.IsAssignableFrom<IEnumerable<NotificationReadDto>>(ok.Value);
            Assert.All(list, dto => Assert.Equal(NormalUserId, dto.UserId));
        }

        /// <summary>
        /// When called by a librarian, GetNotifications should return all notifications.
        /// </summary>
        [Fact]
        public async Task GetNotifications_AsLibrarian_ReturnsAll()
        {
            SetUser(LibrarianUserId, UserRoles.Librarian);

            var action = await _controller.GetNotifications(CancellationToken.None);

            var ok = Assert.IsType<OkObjectResult>(action.Result);
            var list = Assert.IsAssignableFrom<IEnumerable<NotificationReadDto>>(ok.Value);
            Assert.Equal(2, list.Count());
        }

        /// <summary>
        /// Getting a non-existent notification should return 404 NotFound.
        /// </summary>
        [Fact]
        public async Task GetNotification_NotFound_Returns404()
        {
            SetUser(NormalUserId, UserRoles.User);

            var action = await _controller.GetNotification(999, CancellationToken.None);

            Assert.IsType<NotFoundResult>(action.Result);
        }

        /// <summary>
        /// A normal user requesting another user's notification should get 403 Forbidden.
        /// </summary>
        [Fact]
        public async Task GetNotification_ForbiddenForNormalUser_WhenOtherUser()
        {
            SetUser(NormalUserId, UserRoles.User);

            var action = await _controller.GetNotification(20, CancellationToken.None);

            Assert.IsType<ForbidResult>(action.Result);
        }

        /// <summary>
        /// A librarian requesting any notification should get 200 OK with the DTO.
        /// </summary>
        [Fact]
        public async Task GetNotification_AsLibrarian_Returns200()
        {
            SetUser(LibrarianUserId, UserRoles.Librarian);

            var action = await _controller.GetNotification(10, CancellationToken.None);

            var ok  = Assert.IsType<OkObjectResult>(action.Result);
            var dto = Assert.IsType<NotificationReadDto>(ok.Value);
            Assert.Equal(10, dto.NotificationId);
        }

        /// <summary>
        /// Creating a notification as librarian should persist it, dispatch via SignalR, and log the event.
        /// </summary>
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

        /// <summary>
        /// Updating with mismatched IDs should return 400 BadRequest.
        /// </summary>
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

        /// <summary>
        /// Updating a non-existent notification should return 404 NotFound.
        /// </summary>
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

        /// <summary>
        /// Updating an existing notification should return 204 NoContent and persist changes.
        /// </summary>
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

        /// <summary>
        /// Deleting a non-existent notification should return 404 NotFound.
        /// </summary>
        [Fact]
        public async Task DeleteNotification_NotFound_Returns404()
        {
            SetUser(LibrarianUserId, UserRoles.Admin);

            var action = await _controller.DeleteNotification(999, CancellationToken.None);

            Assert.IsType<NotFoundResult>(action);
        }

        /// <summary>
        /// Deleting an existing notification should return 204 NoContent and remove it from the database.
        /// </summary>
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