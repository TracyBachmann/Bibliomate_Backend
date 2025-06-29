using backend.Controllers;
using backend.Models.Mongo;
using backend.Services;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;

namespace Tests.Controllers
{
    public class AuditControllerTests
    {
        private readonly AuditController _controller;
        private readonly IUserActivityLogService _mockActivityLog;

        public AuditControllerTests()
        {
            _mockActivityLog = Substitute.For<IUserActivityLogService>();
            _controller = new AuditController(_mockActivityLog);
        }

        [Fact]
        public async Task GetUserActivityLogs_ReturnsNotFound_WhenNoLogs()
        {
            const int userId = 123;
            _mockActivityLog.GetByUserAsync(userId)
                            .Returns(Task.FromResult(new List<UserActivityLogDocument>()));

            var actionResult = await _controller.GetUserActivityLogs(userId);

            var notFound = Assert.IsType<NotFoundObjectResult>(actionResult.Result);
            Assert.Equal($"No activity logs found for user {userId}.", notFound.Value);
        }

        [Fact]
        public async Task GetUserActivityLogs_ReturnsOk_WithLogs()
        {
            const int userId = 456;
            var docs = new List<UserActivityLogDocument>
            {
                new UserActivityLogDocument
                {
                    UserId    = userId,
                    Timestamp = DateTime.UtcNow.AddMinutes(-1),
                    Action    = "Login",
                    Details   = "User logged in"
                },
                new UserActivityLogDocument
                {
                    UserId    = userId,
                    Timestamp = DateTime.UtcNow,
                    Action    = "Search",
                    Details   = "Searched for 'C# tips'"
                }
            };
            _mockActivityLog.GetByUserAsync(userId)
                            .Returns(Task.FromResult(docs));
            
            var actionResult = await _controller.GetUserActivityLogs(userId);

            var ok = Assert.IsType<OkObjectResult>(actionResult.Result);
            var returned = Assert.IsType<List<UserActivityLogDocument>>(ok.Value);
            Assert.Equal(2, returned.Count);
            Assert.Same(docs[0], returned[0]);
            Assert.Same(docs[1], returned[1]);
        }
    }
}