using backend.DTOs;
using backend.Data;
using backend.Models;
using backend.Models.Enums;
using backend.Models.Mongo;
using backend.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.Text;

namespace Tests.Services
{
    public class ReservationServiceTest
    {
        private readonly ReservationService _service;
        private readonly BiblioMateDbContext _db;

        public ReservationServiceTest()
        {
            // In-memory EF context
            var options = new DbContextOptionsBuilder<BiblioMateDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            
            // Dummy encryption for DbContext
            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["Encryption:Key"] = Convert.ToBase64String(Encoding.UTF8.GetBytes("12345678901234567890123456789012"))
                })
                .Build();
            var encryptionService = new EncryptionService(config);

            _db = new BiblioMateDbContext(options, encryptionService);
            
            // Stub history and audit services
            var history = new DummyHistoryService();
            var audit   = new DummyActivityLogService();

            _service = new ReservationService(_db, history, audit);
        }

        [Fact]
        public async Task GetAllAsync_ShouldReturnAll()
        {
            // Arrange
            var user = new User { Name = "Alice" };
            var book = new Book { Title = "Book A" };
            _db.Users.Add(user);
            _db.Books.Add(book);
            await _db.SaveChangesAsync();

            _db.Reservations.Add(new Reservation { UserId = user.UserId, BookId = book.BookId, Status = ReservationStatus.Pending, ReservationDate = DateTime.UtcNow, CreatedAt = DateTime.UtcNow });
            _db.Reservations.Add(new Reservation { UserId = user.UserId, BookId = book.BookId, Status = ReservationStatus.Completed, ReservationDate = DateTime.UtcNow, CreatedAt = DateTime.UtcNow });
            await _db.SaveChangesAsync();

            // Act
            var list = (await _service.GetAllAsync()).ToList();

            // Assert
            Assert.Equal(2, list.Count);
        }

        [Fact]
        public async Task GetByUserAsync_ShouldFilterByUserAndStatus()
        {
            var user1 = new User { Name = "Bob" };
            var user2 = new User { Name = "Carol" };
            var book  = new Book { Title = "Book B" };
            _db.Users.AddRange(user1, user2);
            _db.Books.Add(book);
            await _db.SaveChangesAsync();

            // user1: one pending, one available, one completed
            _db.Reservations.AddRange(
                new Reservation { UserId = user1.UserId, BookId = book.BookId, Status = ReservationStatus.Pending, ReservationDate = DateTime.UtcNow, CreatedAt = DateTime.UtcNow },
                new Reservation { UserId = user1.UserId, BookId = book.BookId, Status = ReservationStatus.Available, ReservationDate = DateTime.UtcNow, CreatedAt = DateTime.UtcNow },
                new Reservation { UserId = user1.UserId, BookId = book.BookId, Status = ReservationStatus.Completed, ReservationDate = DateTime.UtcNow, CreatedAt = DateTime.UtcNow }
            );
            // user2: one pending
            _db.Reservations.Add(new Reservation { UserId = user2.UserId, BookId = book.BookId, Status = ReservationStatus.Pending, ReservationDate = DateTime.UtcNow, CreatedAt = DateTime.UtcNow });
            await _db.SaveChangesAsync();

            // Act
            var result1 = (await _service.GetByUserAsync(user1.UserId)).ToList();
            var result2 = (await _service.GetByUserAsync(user2.UserId)).ToList();

            // Assert
            Assert.Equal(2, result1.Count); // Pending + Available
            Assert.Single(result2);
        }

        [Fact]
        public async Task GetPendingForBookAsync_ShouldReturnOnlyPending()
        {
            var user = new User { Name = "Dan" };
            var book1 = new Book { Title = "Book C" };
            var book2 = new Book { Title = "Book D" };
            _db.Users.Add(user);
            _db.Books.AddRange(book1, book2);
            await _db.SaveChangesAsync();

            // For book1
            _db.Reservations.Add(new Reservation { UserId = user.UserId, BookId = book1.BookId, Status = ReservationStatus.Pending, ReservationDate = DateTime.UtcNow, CreatedAt = DateTime.UtcNow });
            _db.Reservations.Add(new Reservation { UserId = user.UserId, BookId = book1.BookId, Status = ReservationStatus.Available, ReservationDate = DateTime.UtcNow, CreatedAt = DateTime.UtcNow });
            // For book2
            _db.Reservations.Add(new Reservation { UserId = user.UserId, BookId = book2.BookId, Status = ReservationStatus.Pending, ReservationDate = DateTime.UtcNow, CreatedAt = DateTime.UtcNow });
            await _db.SaveChangesAsync();

            // Act
            var list1 = (await _service.GetPendingForBookAsync(book1.BookId)).ToList();
            var list2 = (await _service.GetPendingForBookAsync(book2.BookId)).ToList();

            // Assert
            Assert.Single(list1);
            Assert.Single(list2);
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturnNull_WhenNotFound()
        {
            var result = await _service.GetByIdAsync(999);
            Assert.Null(result);
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturnDto_WhenExists()
        {
            var user = new User { Name = "Eve" };
            var book = new Book { Title = "Book E" };
            _db.Users.Add(user);
            _db.Books.Add(book);
            await _db.SaveChangesAsync();

            var res = new Reservation { UserId = user.UserId, BookId = book.BookId, Status = ReservationStatus.Pending, ReservationDate = DateTime.UtcNow, CreatedAt = DateTime.UtcNow };
            _db.Reservations.Add(res);
            await _db.SaveChangesAsync();

            var dto = await _service.GetByIdAsync(res.ReservationId);
            Assert.NotNull(dto);
            Assert.Equal(user.UserId, dto.UserId);
            Assert.Equal(book.BookId, dto.BookId);
        }

        [Fact]
        public async Task CreateAsync_ShouldCreateAndReturnDto()
        {
            var user = new User { Name = "Frank" };
            var book = new Book { Title = "Book F" };
            var stock = new Stock { BookId = 0, Quantity = 1, IsAvailable = true };
            _db.Users.Add(user);
            _db.Books.Add(book);
            _db.Stocks.Add(new Stock { BookId = book.BookId, Quantity = 1, IsAvailable = true });
            await _db.SaveChangesAsync();

            var dto = new ReservationCreateDto { UserId = user.UserId, BookId = book.BookId };
            var result = await _service.CreateAsync(dto, user.UserId);

            Assert.NotNull(result);
            Assert.Equal(user.UserId, result.UserId);
            Assert.Equal(book.BookId, result.BookId);
            Assert.Equal(ReservationStatus.Pending, result.Status);
        }

        [Fact]
        public async Task CreateAsync_ShouldThrowUnauthorized_WhenUserMismatch()
        {
            var dto = new ReservationCreateDto { UserId = 1, BookId = 1 };
            await Assert.ThrowsAsync<UnauthorizedAccessException>(
                () => _service.CreateAsync(dto, currentUserId: 2));
        }

        [Fact]
        public async Task CreateAsync_ShouldThrowInvalidOperation_WhenDuplicate()
        {
            var user = new User { Name = "Gina" };
            var book = new Book { Title = "Book G" };
            _db.Users.Add(user);
            _db.Books.Add(book);
            _db.Reservations.Add(new Reservation { UserId = user.UserId, BookId = book.BookId, Status = ReservationStatus.Pending, ReservationDate = DateTime.UtcNow, CreatedAt = DateTime.UtcNow });
            _db.Stocks.Add(new Stock { BookId = book.BookId, Quantity = 1, IsAvailable = true });
            await _db.SaveChangesAsync();

            var dto = new ReservationCreateDto { UserId = user.UserId, BookId = book.BookId };
            await Assert.ThrowsAsync<InvalidOperationException>(
                () => _service.CreateAsync(dto, user.UserId));
        }

        [Fact]
        public async Task CreateAsync_ShouldThrowInvalidOperation_WhenNoStock()
        {
            var user = new User { Name = "Hank" };
            var book = new Book { Title = "Book H" };
            _db.Users.Add(user);
            _db.Books.Add(book);
            // no stock entry
            await _db.SaveChangesAsync();

            var dto = new ReservationCreateDto { UserId = user.UserId, BookId = book.BookId };
            await Assert.ThrowsAsync<InvalidOperationException>(
                () => _service.CreateAsync(dto, user.UserId));
        }

        [Fact]
        public async Task UpdateAsync_ShouldReturnTrue_WhenExists()
        {
            var res = new Reservation { UserId = 1, BookId = 1, Status = ReservationStatus.Pending, ReservationDate = DateTime.UtcNow, CreatedAt = DateTime.UtcNow };
            _db.Reservations.Add(res);
            await _db.SaveChangesAsync();

            var dto = new ReservationUpdateDto { ReservationId = res.ReservationId, UserId = res.UserId, BookId = res.BookId, ReservationDate = res.ReservationDate, Status = ReservationStatus.Completed };
            var ok = await _service.UpdateAsync(dto);

            Assert.True(ok);
            var updated = await _db.Reservations.FindAsync(res.ReservationId);

            Assert.NotNull(updated);

            Assert.Equal(ReservationStatus.Completed, updated.Status);
        }

        [Fact]
        public async Task UpdateAsync_ShouldReturnFalse_WhenNotExists()
        {
            var dto = new ReservationUpdateDto { ReservationId = 999, UserId = 1, BookId = 1, ReservationDate = DateTime.UtcNow, Status = ReservationStatus.Pending };
            var ok = await _service.UpdateAsync(dto);
            Assert.False(ok);
        }

        [Fact]
        public async Task DeleteAsync_ShouldReturnTrue_WhenExists()
        {
            var res = new Reservation { UserId = 1, BookId = 1, Status = ReservationStatus.Pending, ReservationDate = DateTime.UtcNow, CreatedAt = DateTime.UtcNow };
            _db.Reservations.Add(res);
            await _db.SaveChangesAsync();

            var ok = await _service.DeleteAsync(res.ReservationId);
            Assert.True(ok);
            Assert.False(await _db.Reservations.AnyAsync(r => r.ReservationId == res.ReservationId));
        }

        [Fact]
        public async Task DeleteAsync_ShouldReturnFalse_WhenNotExists()
        {
            var ok = await _service.DeleteAsync(999);
            Assert.False(ok);
        }

        // Dummy stub implementations
        private class DummyHistoryService : IHistoryService
        {
            public Task LogEventAsync(int userId, string eventType, int? loanId = null, int? reservationId = null)
                => Task.CompletedTask;

            public Task<List<HistoryReadDto>> GetHistoryForUserAsync(int userId, int page = 1, int pageSize = 20)
                => Task.FromResult(new List<HistoryReadDto>());
        }

        private class DummyActivityLogService : IUserActivityLogService
        {
            public Task LogAsync(UserActivityLogDocument doc)
                => Task.CompletedTask;

            public Task<List<UserActivityLogDocument>> GetByUserAsync(int userId)
                => Task.FromResult(new List<UserActivityLogDocument>());
        }
    }
}
