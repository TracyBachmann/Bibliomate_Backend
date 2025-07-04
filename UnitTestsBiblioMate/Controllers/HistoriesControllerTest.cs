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
    /// Verifies behavior of GetUserHistory endpoint with authorization and pagination.
    /// </summary>
    public class HistoriesControllerTest
    {
        private readonly Mock<IHistoryService> _historyServiceMock;
        private readonly HistoriesController _controller;

        public HistoriesControllerTest()
        {
            _historyServiceMock = new Mock<IHistoryService>();
            _controller = new HistoriesController(_historyServiceMock.Object);
        }

        /// <summary>
        /// Owner requests their own history ⇒ 200 OK.
        /// </summary>
        [Fact]
        public async Task GetUserHistory_Owner_ShouldReturnOk()
        {
            // Arrange
            const int userId = 7;
            const int page = 1;
            const int pageSize = 20;
            var history = new List<HistoryReadDto>();  // on ne se soucie pas des champs
            _historyServiceMock
                .Setup(s => s.GetHistoryForUserAsync(userId, page, pageSize, It.IsAny<CancellationToken>()))
                .ReturnsAsync(history);

            // Simuler l'utilisateur authentifié (propriétaire)
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
        /// Staff (Librarian/Admin) requests someone else's history ⇒ 200 OK.
        /// </summary>
        [Fact]
        public async Task GetUserHistory_Staff_ShouldReturnOk()
        {
            // Arrange
            const int requestedUserId = 8;
            const int currentUserId = 1;
            const int page = 2;
            const int pageSize = 10;
            var history = new List<HistoryReadDto>();
            _historyServiceMock
                .Setup(s => s.GetHistoryForUserAsync(requestedUserId, page, pageSize, It.IsAny<CancellationToken>()))
                .ReturnsAsync(history);

            // Simuler un utilisateur en rôle Librarian
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
        /// Un autorisé demande l'historique d'un autre ⇒ 403 Forbidden.
        /// </summary>
        [Fact]
        public async Task GetUserHistory_Unauthorized_ShouldReturnForbid()
        {
            // Arrange
            const int requestedUserId = 5;
            const int currentUserId = 6;

            // Simuler un utilisateur sans rôle staff
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