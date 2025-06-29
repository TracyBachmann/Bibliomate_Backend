using System.Text;
using backend.Data;
using backend.Models;
using backend.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Xunit.Abstractions;

namespace Tests.Services
{
    public class HistoryServiceTest
    {
        private readonly HistoryService _service;
        private readonly BiblioMateDbContext _db;
        private readonly ITestOutputHelper _output;

        public HistoryServiceTest(ITestOutputHelper output)
        {
            _output = output;

            // 1) In-memory EF context
            var options = new DbContextOptionsBuilder<BiblioMateDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            // 2) EncryptionService required by your DbContext constructor
            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["Encryption:Key"] = Convert.ToBase64String(Encoding.UTF8.GetBytes(
                        "12345678901234567890123456789012"))
                })
                .Build();
            var encryptionService = new EncryptionService(config);

            _db = new BiblioMateDbContext(options, encryptionService);
            _service = new HistoryService(_db);
        }

        [Fact]
        public async Task LogEventAsync_ShouldPersistHistoryRecord()
        {
            // Arrange
            int userId = 42;
            string eventType = "Loan";
            int? loanId = 7;
            int? reservationId = null;

            // Act
            await _service.LogEventAsync(userId, eventType, loanId, reservationId);

            // Assert
            var stored = await _db.Histories.SingleAsync();
            Assert.Equal(userId, stored.UserId);
            Assert.Equal(eventType, stored.EventType);
            Assert.Equal(loanId, stored.LoanId);
            Assert.Equal(reservationId, stored.ReservationId);
            _output.WriteLine($"Logged event at {stored.EventDate:O}");
        }

        [Fact]
        public async Task GetHistoryForUserAsync_ShouldReturnEmptyList_WhenNoRecords()
        {
            // Act
            var page = await _service.GetHistoryForUserAsync(userId: 1);
            // Assert
            Assert.Empty(page);
        }

        [Fact]
        public async Task GetHistoryForUserAsync_ShouldReturnOrderedPage()
        {
            // Arrange: insert 3 records with different timestamps
            var now = DateTime.UtcNow;
            _db.Histories.AddRange(new[]
            {
                new History { UserId = 1, EventType = "A", EventDate = now.AddMinutes(-5) },
                new History { UserId = 1, EventType = "B", EventDate = now.AddMinutes(-2) },
                new History { UserId = 1, EventType = "C", EventDate = now.AddMinutes(-1) },
            });
            // also a record for another user
            _db.Histories.Add(new History { UserId = 2, EventType = "X", EventDate = now });
            await _db.SaveChangesAsync();

            // Act
            var page1 = await _service.GetHistoryForUserAsync(userId: 1, page: 1, pageSize: 2);
            var page2 = await _service.GetHistoryForUserAsync(userId: 1, page: 2, pageSize: 2);

            // Assert page1: most recent two ("C","B")
            Assert.Equal(2, page1.Count);
            Assert.Equal("C", page1[0].EventType);
            Assert.Equal("B", page1[1].EventType);

            // Assert page2: next one ("A")
            Assert.Single(page2);
            Assert.Equal("A", page2[0].EventType);
        }
    }
}
