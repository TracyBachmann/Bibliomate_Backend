using System.Text;
using BackendBiblioMate.Data;
using BackendBiblioMate.Models;
using BackendBiblioMate.Services.Users;
using BackendBiblioMate.Services.Infrastructure.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Xunit.Abstractions;

namespace UnitTestsBiblioMate.Services.Users
{
    /// <summary>
    /// Unit tests for <see cref="HistoryService"/>.
    /// Validates logging of history events and paginated retrieval of user history.
    /// </summary>
    public class HistoryServiceTest
    {
        private readonly HistoryService _service;
        private readonly BiblioMateDbContext _db;
        private readonly ITestOutputHelper _output;

        /// <summary>
        /// Initializes an in-memory database and the HistoryService.
        /// </summary>
        public HistoryServiceTest(ITestOutputHelper output)
        {
            _output = output;

            // In-memory EF Core database
            var options = new DbContextOptionsBuilder<BiblioMateDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            // EncryptionService for DbContext constructor
            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["Encryption:Key"] = Convert.ToBase64String(
                        Encoding.UTF8.GetBytes("12345678901234567890123456789012"))
                })
                .Build();
            var encryptionService = new EncryptionService(config);

            _db = new BiblioMateDbContext(options, encryptionService);
            _service = new HistoryService(_db);
        }

        /// <summary>
        /// Ensures LogEventAsync persists a history record with the correct data.
        /// </summary>
        [Fact]
        public async Task LogEventAsync_ShouldPersistHistoryRecord()
        {
            // Act
            await _service.LogEventAsync(userId: 42, eventType: "Loan", loanId: 7, reservationId: null);

            // Assert
            var stored = await _db.Histories.SingleAsync();
            Assert.Equal(42, stored.UserId);
            Assert.Equal("Loan", stored.EventType);
            Assert.Equal(7, stored.LoanId);
            Assert.Null(stored.ReservationId);
            _output.WriteLine($"Logged event at {stored.EventDate:O}");
        }

        /// <summary>
        /// Ensures GetHistoryForUserAsync returns an empty list when no history exists.
        /// </summary>
        [Fact]
        public async Task GetHistoryForUserAsync_ShouldReturnEmptyList_WhenNoRecords()
        {
            // Act
            var page = await _service.GetHistoryForUserAsync(userId: 1);

            // Assert
            Assert.Empty(page);
        }

        /// <summary>
        /// Ensures GetHistoryForUserAsync returns history in descending order and paginates correctly.
        /// </summary>
        [Fact]
        public async Task GetHistoryForUserAsync_ShouldReturnOrderedPage()
        {
            // Arrange: three records for user 1 and one for user 2
            var now = DateTime.UtcNow;
            _db.Histories.AddRange(new[]
            {
                new History { UserId = 1, EventType = "A", EventDate = now.AddMinutes(-5) },
                new History { UserId = 1, EventType = "B", EventDate = now.AddMinutes(-2) },
                new History { UserId = 1, EventType = "C", EventDate = now.AddMinutes(-1) },
                new History { UserId = 2, EventType = "X", EventDate = now }
            });
            await _db.SaveChangesAsync();

            // Act
            var page1 = await _service.GetHistoryForUserAsync(userId: 1, page: 1, pageSize: 2);
            var page2 = await _service.GetHistoryForUserAsync(userId: 1, page: 2, pageSize: 2);

            // Assert page1: most recent two ("C", "B")
            Assert.Equal(2, page1.Count);
            Assert.Equal("C", page1[0].EventType);
            Assert.Equal("B", page1[1].EventType);

            // Assert page2: the next one ("A")
            Assert.Single(page2);
            Assert.Equal("A", page2[0].EventType);
        }
    }
}
