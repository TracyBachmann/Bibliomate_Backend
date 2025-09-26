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
    /// Unit tests for <see cref="RecommendationsController"/>.
    /// Verifies authorization rules and successful retrieval of recommendations
    /// depending on user identity and roles.
    /// </summary>
    public class RecommendationsControllerTest
    {
        private readonly Mock<IRecommendationService> _serviceMock;
        private readonly RecommendationsController    _controller;

        /// <summary>
        /// Initializes the test class by setting up a mocked <see cref="IRecommendationService"/>
        /// and an instance of <see cref="RecommendationsController"/>.
        /// </summary>
        public RecommendationsControllerTest()
        {
            _serviceMock = new Mock<IRecommendationService>();
            _controller  = new RecommendationsController(_serviceMock.Object);
        }

        /// <summary>
        /// Helper method to configure the controller's <see cref="ClaimsPrincipal"/>
        /// with the provided user ID and role.
        /// </summary>
        /// <param name="userId">The user ID to set in the claims.</param>
        /// <param name="role">The role to assign (e.g., <see cref="UserRoles.User"/> or <see cref="UserRoles.Admin"/>).</param>
        private void SetUser(int userId, string role)
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
                new Claim(ClaimTypes.Role, role)
            };
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
        /// Verifies that a normal user requesting their own recommendations
        /// receives a 200 OK response containing the expected list of recommendations.
        /// </summary>
        [Fact]
        public async Task GetRecommendations_UserRequestsOwn_ReturnsOk()
        {
            // Arrange
            const int userId = 5;
            SetUser(userId, UserRoles.User);

            var recs = new List<RecommendationReadDto>
            {
                new RecommendationReadDto { BookId = 1, Title = "Book A" },
                new RecommendationReadDto { BookId = 2, Title = "Book B" }
            };
            _serviceMock
                .Setup(s => s.GetRecommendationsForUserAsync(userId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(recs);

            // Act
            var action = await _controller.GetRecommendations(userId, CancellationToken.None);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(action.Result);
            Assert.Equal(recs, okResult.Value);
        }

        /// <summary>
        /// Verifies that an administrator requesting recommendations
        /// for another user receives a 200 OK response with the expected list.
        /// </summary>
        [Fact]
        public async Task GetRecommendations_AdminRequestsOtherUser_ReturnsOk()
        {
            // Arrange
            const int targetUserId = 7;
            SetUser(userId: 1, role: UserRoles.Admin);

            var recs = new List<RecommendationReadDto>
            {
                new RecommendationReadDto { BookId = 3, Title = "Book X" }
            };
            _serviceMock
                .Setup(s => s.GetRecommendationsForUserAsync(targetUserId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(recs);

            // Act
            var action = await _controller.GetRecommendations(targetUserId, CancellationToken.None);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(action.Result);
            Assert.Equal(recs, okResult.Value);
        }

        /// <summary>
        /// Verifies that a normal user attempting to request recommendations
        /// for another user (not themselves) receives a 403 Forbidden response.
        /// </summary>
        [Fact]
        public async Task GetRecommendations_UserRequestsOtherUser_ReturnsForbid()
        {
            // Arrange
            const int currentUserId = 8;
            const int otherUserId   = 9;
            SetUser(currentUserId, UserRoles.User);

            // Act
            var action = await _controller.GetRecommendations(otherUserId, CancellationToken.None);

            // Assert
            Assert.IsType<ForbidResult>(action.Result);
        }
    }
}
