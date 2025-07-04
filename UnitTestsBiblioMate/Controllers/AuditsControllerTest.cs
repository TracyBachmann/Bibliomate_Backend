using BackendBiblioMate.Controllers;
using BackendBiblioMate.Interfaces;
using BackendBiblioMate.Models.Mongo;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;

namespace UnitTestsBiblioMate.Controllers
{
    /// <summary>
    /// Unit tests for <see cref="AuditsController"/>.
    /// Verifies the behavior of the GetUserActivityLogs endpoint.
    /// </summary>
    public class AuditsControllerTest
    {
        private readonly AuditsController _controller;
        private readonly IUserActivityLogService _mockActivityLog;

        /// <summary>
        /// Initializes the test with a mocked <see cref="IUserActivityLogService"/>
        /// and an instance of <see cref="AuditsController"/>.
        /// </summary>
        public AuditsControllerTest()
        {
            _mockActivityLog = Substitute.For<IUserActivityLogService>();
            _controller       = new AuditsController(_mockActivityLog);
        }

        /// <summary>
        /// Ensures that when the service returns an empty list,
        /// the controller responds with 404 NotFound and the correct message.
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
            Assert.Equal($"No activity logs found for user {userId}.", notFound.Value);
        }

        /// <summary>
        /// Ensures that when the service returns logs,
        /// the controller responds with 200 OK and the same list of documents.
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
            var ok       = Assert.IsType<OkObjectResult>(actionResult.Result);
            var returned = Assert.IsType<List<UserActivityLogDocument>>(ok.Value);
            Assert.Equal(2, returned.Count);
            Assert.Same(docs[0], returned[0]);
            Assert.Same(docs[1], returned[1]);
        }
    }
}