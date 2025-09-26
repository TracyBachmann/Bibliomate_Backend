using BackendBiblioMate.Controllers;
using BackendBiblioMate.Interfaces;
using BackendBiblioMate.Models.Mongo;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;

namespace UnitTestsBiblioMate.Controllers
{
    /// <summary>
    /// Unit test suite for <see cref="AuditsController"/>.
    /// Validates the behavior of user activity log retrieval endpoints
    /// under various conditions.
    /// </summary>
    public class AuditsControllerTest
    {
        private readonly AuditsController _controller;
        private readonly IUserActivityLogService _mockActivityLog;

        /// <summary>
        /// Initializes the test environment with:
        /// <list type="bullet">
        ///   <item><description>
        /// A mocked <see cref="IUserActivityLogService"/> using NSubstitute.
        /// </description></item>
        ///   <item><description>
        /// An instance of <see cref="AuditsController"/> configured
        /// to use the mocked dependency.
        /// </description></item>
        /// </list>
        /// </summary>
        public AuditsControllerTest()
        {
            _mockActivityLog = Substitute.For<IUserActivityLogService>();
            _controller      = new AuditsController(_mockActivityLog);
        }

        /// <summary>
        /// Validates that when no activity logs exist for the user:
        /// <list type="bullet">
        ///   <item><description>
        /// The controller returns a <see cref="NotFoundObjectResult"/>.
        /// </description></item>
        ///   <item><description>
        /// The response has HTTP status code 404.
        /// </description></item>
        ///   <item><description>
        /// The response contains an explanatory message.
        /// </description></item>
        /// </list>
        /// </summary>
        [Fact]
        public async Task GetUserActivityLogs_ReturnsNotFound_WhenNoLogs()
        {
            // Arrange
            const int userId = 123;
            _mockActivityLog
                .GetByUserAsync(userId, Arg.Any<CancellationToken>())
                .Returns(Task.FromResult(new List<UserActivityLogDocument>()));

            // Act
            var actionResult = await _controller.GetUserActivityLogs(userId, CancellationToken.None);

            // Assert
            var notFound = Assert.IsType<NotFoundObjectResult>(actionResult.Result);
            Assert.Equal(404, notFound.StatusCode);
            Assert.Contains("No activity logs found", notFound.Value?.ToString());
        }

        /// <summary>
        /// Validates that when activity logs are available for the user:
        /// <list type="bullet">
        ///   <item><description>
        /// The controller returns a <see cref="OkObjectResult"/>.
        /// </description></item>
        ///   <item><description>
        /// The response contains the same number of logs as provided
        /// by the service.
        /// </description></item>
        ///   <item><description>
        /// Each log entry matches the expected user ID and action.
        /// </description></item>
        /// </list>
        /// </summary>
        [Fact]
        public async Task GetUserActivityLogs_ReturnsOk_WithLogs()
        {
            // Arrange
            const int userId = 456;
            var docs = new List<UserActivityLogDocument>
            {
                new()
                {
                    UserId    = userId,
                    Timestamp = DateTime.UtcNow.AddMinutes(-1),
                    Action    = "Login",
                    Details   = "User logged in"
                },
                new()
                {
                    UserId    = userId,
                    Timestamp = DateTime.UtcNow,
                    Action    = "Search",
                    Details   = "Searched for 'C# tips'"
                }
            };

            _mockActivityLog
                .GetByUserAsync(userId, Arg.Any<CancellationToken>())
                .Returns(Task.FromResult(docs));

            // Act
            var actionResult = await _controller.GetUserActivityLogs(userId, CancellationToken.None);

            // Assert
            var ok = Assert.IsType<OkObjectResult>(actionResult.Result);
            var returned = Assert.IsAssignableFrom<List<UserActivityLogDocument>>(ok.Value);

            Assert.Equal(2, returned.Count);

            // Value-based checks (not just reference equality)
            Assert.Equal(userId, returned[0].UserId);
            Assert.Equal("Login", returned[0].Action);

            Assert.Equal(userId, returned[1].UserId);
            Assert.Equal("Search", returned[1].Action);
        }
    }
}