using System.Security.Claims;
using BackendBiblioMate.Controllers;
using BackendBiblioMate.DTOs;
using BackendBiblioMate.Interfaces;
using BackendBiblioMate.Models.Enums;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace UnitTestsBiblioMate.Controllers
{
    /// <summary>
    /// Unit tests for <see cref="HistoriesController"/>.
    /// Validates authorization rules and response behavior for the
    /// <see cref="HistoriesController.GetUserHistory"/> endpoint.
    /// </summary>
    public class HistoriesControllerTest
    {
        private readonly Mock<IHistoryService> _historyServiceMock;
        private readonly HistoriesController _controller;

        /// <summary>
        /// Initializes the test environment with:
        /// <list type="bullet">
        ///   <item><description>A mocked <see cref="IHistoryService"/>.</description></item>
        ///   <item><description>An instance of <see cref="HistoriesController"/> using the mock.</description></item>
        /// </list>
        /// </summary>
        public HistoriesControllerTest()
        {
            _historyServiceMock = new Mock<IHistoryService>();
            _controller         = new HistoriesController(_historyServiceMock.Object);
        }

        /// <summary>
        /// Ensures that when a user requests their own history,
        /// the controller returns HTTP 200 OK with the correct data.
        /// </summary>
        [Fact]
        public async Task GetUserHistory_Owner_ShouldReturnOk()
        {
            // Arrange
            const int userId   = 7;
            const int page     = 1;
            const int pageSize = 20;
            var history        = new List<HistoryReadDto>();

            _historyServiceMock
                .Setup(s => s.GetHistoryForUserAsync(userId, page, pageSize, It.IsAny<CancellationToken>()))
                .ReturnsAsync(history);

            // Simulate authenticated user (owner)
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity(
                        new[] { new Claim(ClaimTypes.NameIdentifier, userId.ToString()) }, "TestAuth"))
                }
            };

            // Act
            var result = await _controller.GetUserHistory(userId, page, pageSize, CancellationToken.None);

            // Assert
            var ok = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Same(history, ok.Value);
        }

        /// <summary>
        /// Ensures that when a staff member (Librarian/Admin)
        /// requests another user's history, the controller returns HTTP 200 OK.
        /// </summary>
        [Fact]
        public async Task GetUserHistory_Staff_ShouldReturnOk()
        {
            // Arrange
            const int requestedUserId = 8;
            const int currentUserId   = 1;
            const int page            = 2;
            const int pageSize        = 10;
            var history               = new List<HistoryReadDto>();

            _historyServiceMock
                .Setup(s => s.GetHistoryForUserAsync(requestedUserId, page, pageSize, It.IsAny<CancellationToken>()))
                .ReturnsAsync(history);

            // Simulate user with Librarian role
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity(new[]
                    {
                        new Claim(ClaimTypes.NameIdentifier, currentUserId.ToString()),
                        new Claim(ClaimTypes.Role, UserRoles.Librarian)
                    }, "TestAuth"))
                }
            };

            // Act
            var result = await _controller.GetUserHistory(requestedUserId, page, pageSize, CancellationToken.None);

            // Assert
            var ok = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Same(history, ok.Value);
        }

        /// <summary>
        /// Ensures that when an unauthorized user (not owner and not staff)
        /// requests another user's history, the controller returns HTTP 403 Forbidden.
        /// </summary>
        [Fact]
        public async Task GetUserHistory_Unauthorized_ShouldReturnForbid()
        {
            // Arrange
            const int requestedUserId = 5;
            const int currentUserId   = 6;

            // Simulate non-staff user trying to access another user's history
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity(
                        new[] { new Claim(ClaimTypes.NameIdentifier, currentUserId.ToString()) }, "TestAuth"))
                }
            };

            // Act
            var result = await _controller.GetUserHistory(requestedUserId, 1, 20, CancellationToken.None);

            // Assert
            Assert.IsType<ForbidResult>(result.Result);
        }
    }
}
