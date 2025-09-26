using BackendBiblioMate.Controllers;
using BackendBiblioMate.DTOs;
using BackendBiblioMate.Interfaces;
using BackendBiblioMate.Models.Enums;
using BackendBiblioMate.Models.Mongo;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace UnitTestsBiblioMate.Controllers
{
    /// <summary>
    /// Unit tests for <see cref="NotificationsLogsController"/>.
    /// Verifies behavior of all endpoints with a mocked <see cref="IMongoLogService"/>.
    /// </summary>
    public class NotificationsLogsControllerTest
    {
        private readonly Mock<IMongoLogService> _mongoLogMock;
        private readonly NotificationsLogsController _controller;

        /// <summary>
        /// Initializes the test class with a mocked Mongo logging service.
        /// </summary>
        public NotificationsLogsControllerTest()
        {
            _mongoLogMock = new Mock<IMongoLogService>();
            _controller   = new NotificationsLogsController(_mongoLogMock.Object);
        }

        /// <summary>
        /// Ensures that <see cref="NotificationsLogsController.GetAll"/> returns
        /// all documents with 200 OK.
        /// </summary>
        [Fact]
        public async Task GetAll_ShouldReturnOkWithLogs()
        {
            // Arrange
            var logs = new List<NotificationLogDocument>
            {
                new NotificationLogDocument { Id = "1", UserId = 1, Type = NotificationType.Custom, Message = "Msg1" },
                new NotificationLogDocument { Id = "2", UserId = 2, Type = NotificationType.Info,   Message = "Msg2" }
            };
            _mongoLogMock
                .Setup(s => s.GetAllAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(logs);

            // Act
            var action = await _controller.GetAll(CancellationToken.None);

            // Assert
            var ok = Assert.IsType<OkObjectResult>(action.Result);
            Assert.Equal(logs, ok.Value);
        }

        /// <summary>
        /// Ensures that <see cref="NotificationsLogsController.GetById"/>
        /// returns the document when it exists.
        /// </summary>
        [Fact]
        public async Task GetById_ShouldReturnOkWhenFound()
        {
            // Arrange
            var log = new NotificationLogDocument
            {
                Id      = "abc123",
                UserId  = 5,
                Type    = NotificationType.Custom,
                Message = "Hello",
                SentAt  = DateTime.UtcNow
            };
            _mongoLogMock
                .Setup(s => s.GetByIdAsync("abc123", It.IsAny<CancellationToken>()))
                .ReturnsAsync(log);

            // Act
            var action = await _controller.GetById("abc123", CancellationToken.None);

            // Assert
            var ok = Assert.IsType<OkObjectResult>(action);
            Assert.Equal(log, ok.Value);
        }

        /// <summary>
        /// Ensures that <see cref="NotificationsLogsController.GetById"/>
        /// returns 404 NotFound when the document does not exist.
        /// </summary>
        [Fact]
        public async Task GetById_ShouldReturnNotFoundWhenMissing()
        {
            // Arrange
            _mongoLogMock
                .Setup(s => s.GetByIdAsync("missing", It.IsAny<CancellationToken>()))
                .ReturnsAsync((NotificationLogDocument)null!);

            // Act
            var action = await _controller.GetById("missing", CancellationToken.None);

            // Assert
            var nf = Assert.IsType<NotFoundObjectResult>(action);
            Assert.Contains("not found", nf.Value!.ToString(), StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Ensures that <see cref="NotificationsLogsController.Create"/> adds
        /// a new document and returns 201 Created.
        /// </summary>
        [Fact]
        public async Task Create_ShouldReturnCreatedWithDocument()
        {
            // Arrange
            var dto = new NotificationLogCreateDto
            {
                UserId  = 7,
                Type    = NotificationType.Warning,
                Message = "Test message",
                SentAt  = DateTime.UtcNow
            };
            NotificationLogDocument createdDoc = null!;
            _mongoLogMock
                .Setup(s => s.AddAsync(It.IsAny<NotificationLogDocument>(), It.IsAny<CancellationToken>()))
                .Callback<NotificationLogDocument, CancellationToken>((doc, ct) =>
                {
                    // Simulate Mongo ID generation
                    doc.Id = "new-id";
                    createdDoc = doc;
                })
                .Returns(Task.CompletedTask);

            // Act
            var action = await _controller.Create(dto, CancellationToken.None);

            // Assert
            var created = Assert.IsType<CreatedAtActionResult>(action);
            Assert.Equal(nameof(NotificationsLogsController.GetById), created.ActionName);
            Assert.Equal("new-id", created.RouteValues!["id"]);
            var returned = Assert.IsType<NotificationLogDocument>(created.Value);
            Assert.Equal(createdDoc, returned);
        }
    }
}
