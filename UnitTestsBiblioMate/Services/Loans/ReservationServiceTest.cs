using System.Text;
using BackendBiblioMate.Data;
using BackendBiblioMate.DTOs;
using BackendBiblioMate.Interfaces;
using BackendBiblioMate.Models;
using BackendBiblioMate.Models.Enums;
using BackendBiblioMate.Models.Mongo;
using BackendBiblioMate.Services.Loans;
using BackendBiblioMate.Services.Infrastructure.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace UnitTestsBiblioMate.Services.Loans
{
    /// <summary>
    /// Unit tests for <see cref="ReservationService"/>,
    /// covering retrieval, creation, update, and deletion,
    /// with authorization and business rule validation.
    /// </summary>
    public class ReservationServiceTest
    {
        private readonly ReservationService _service;
        private readonly BiblioMateDbContext _db;

        public ReservationServiceTest()
        {
            var options = new DbContextOptionsBuilder<BiblioMateDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["Encryption:Key"] = Convert.ToBase64String(
                        Encoding.UTF8.GetBytes("12345678901234567890123456789012")
                    )
                })
                .Build();

            var encryptionService = new EncryptionService(config);
            _db = new BiblioMateDbContext(options, encryptionService);

            _service = new ReservationService(
                _db,
                new DummyHistoryService(),
                new DummyActivityLogService()
            );
        }

        /// <summary>
        /// Retrieving all reservations should return every record in the DB.
        /// </summary>
        [Fact]
        public async Task GetAllAsync_ShouldReturnAll()
        {
            var user = new User  { Name = "Alice" };
            var book = new Book  { Title = "Book A" };
            _db.Users.Add(user);
            _db.Books.Add(book);
            await _db.SaveChangesAsync();

            _db.Reservations.AddRange(
                new Reservation
                {
                    UserId          = user.UserId,
                    BookId          = book.BookId,
                    Status          = ReservationStatus.Pending,
                    ReservationDate = DateTime.UtcNow,
                    CreatedAt       = DateTime.UtcNow
                },
                new Reservation
                {
                    UserId          = user.UserId,
                    BookId          = book.BookId,
                    Status          = ReservationStatus.Completed,
                    ReservationDate = DateTime.UtcNow,
                    CreatedAt       = DateTime.UtcNow
                }
            );
            await _db.SaveChangesAsync();

            var list = (await _service.GetAllAsync()).ToList();
            Assert.Equal(2, list.Count);
        }

        /// <summary>
        /// GetByUserAsync should return only Pending or Available for that user.
        /// </summary>
        [Fact]
        public async Task GetByUserAsync_ShouldFilterByUserAndStatus()
        {
            var u1   = new User { Name = "Bob" };
            var u2   = new User { Name = "Carol" };
            var book = new Book { Title = "Book B" };
            _db.Users.AddRange(u1, u2);
            _db.Books.Add(book);
            await _db.SaveChangesAsync();

            _db.Reservations.AddRange(
                new Reservation { UserId = u1.UserId, BookId = book.BookId, Status = ReservationStatus.Pending,   ReservationDate = DateTime.UtcNow, CreatedAt = DateTime.UtcNow },
                new Reservation { UserId = u1.UserId, BookId = book.BookId, Status = ReservationStatus.Available, ReservationDate = DateTime.UtcNow, CreatedAt = DateTime.UtcNow },
                new Reservation { UserId = u1.UserId, BookId = book.BookId, Status = ReservationStatus.Completed, ReservationDate = DateTime.UtcNow, CreatedAt = DateTime.UtcNow },
                new Reservation { UserId = u2.UserId, BookId = book.BookId, Status = ReservationStatus.Pending,   ReservationDate = DateTime.UtcNow, CreatedAt = DateTime.UtcNow }
            );
            await _db.SaveChangesAsync();

            var r1 = (await _service.GetByUserAsync(u1.UserId)).ToList();
            var r2 = (await _service.GetByUserAsync(u2.UserId)).ToList();

            Assert.Equal(2, r1.Count); // Pending + Available
            Assert.Single(r2);
        }

        /// <summary>
        /// GetPendingForBookAsync returns only Pending for that book.
        /// </summary>
        [Fact]
        public async Task GetPendingForBookAsync_ShouldReturnOnlyPending()
        {
            var user  = new User { Name = "Dan" };
            var b1    = new Book { Title = "Book C" };
            var b2    = new Book { Title = "Book D" };
            _db.Users.Add(user);
            _db.Books.AddRange(b1, b2);
            await _db.SaveChangesAsync();

            _db.Reservations.AddRange(
                new Reservation { UserId = user.UserId, BookId = b1.BookId, Status = ReservationStatus.Pending,   ReservationDate = DateTime.UtcNow, CreatedAt = DateTime.UtcNow },
                new Reservation { UserId = user.UserId, BookId = b1.BookId, Status = ReservationStatus.Available, ReservationDate = DateTime.UtcNow, CreatedAt = DateTime.UtcNow },
                new Reservation { UserId = user.UserId, BookId = b2.BookId, Status = ReservationStatus.Pending,   ReservationDate = DateTime.UtcNow, CreatedAt = DateTime.UtcNow }
            );
            await _db.SaveChangesAsync();

            var p1 = (await _service.GetPendingForBookAsync(b1.BookId)).ToList();
            var p2 = (await _service.GetPendingForBookAsync(b2.BookId)).ToList();

            Assert.Single(p1);
            Assert.Single(p2);
        }

        /// <summary>
        /// GetByIdAsync should return null if not found.
        /// </summary>
        [Fact]
        public async Task GetByIdAsync_ShouldReturnNull_WhenNotFound()
        {
            var dto = await _service.GetByIdAsync(999);
            Assert.Null(dto);
        }

        /// <summary>
        /// GetByIdAsync should return correct DTO when reservation exists.
        /// </summary>
        [Fact]
        public async Task GetByIdAsync_ShouldReturnDto_WhenExists()
        {
            var u    = new User { Name = "Eve" };
            var book = new Book { Title = "Book E" };
            _db.Users.Add(u);
            _db.Books.Add(book);
            await _db.SaveChangesAsync();

            var res = new Reservation
            {
                UserId          = u.UserId,
                BookId          = book.BookId,
                Status          = ReservationStatus.Pending,
                ReservationDate = DateTime.UtcNow,
                CreatedAt       = DateTime.UtcNow
            };
            _db.Reservations.Add(res);
            await _db.SaveChangesAsync();

            var dto = await _service.GetByIdAsync(res.ReservationId);
            Assert.NotNull(dto);
            Assert.Equal(u.UserId,   dto.UserId);
            Assert.Equal(book.BookId, dto.BookId);
        }

        /// <summary>
        /// CreateAsync should succeed, set Pending status, and return DTO.
        /// </summary>
        [Fact]
        public async Task CreateAsync_ShouldCreateAndReturnDto()
        {
            var u    = new User { Name = "Frank" };
            var book = new Book { Title = "Book F" };
            _db.Users.Add(u);
            _db.Books.Add(book);

            // Ensure at least one stock exists
            _db.Stocks.Add(new Stock { BookId = book.BookId, Quantity = 1 });
            await _db.SaveChangesAsync();

            var dto = new ReservationCreateDto { UserId = u.UserId, BookId = book.BookId };
            var result = await _service.CreateAsync(dto, currentUserId: u.UserId);

            Assert.NotNull(result);
            Assert.Equal(u.UserId,    result.UserId);
            Assert.Equal(book.BookId, result.BookId);
            Assert.Equal(ReservationStatus.Pending, result.Status);
        }

        /// <summary>
        /// CreateAsync should throw if currentUserId doesn't match DTO.UserId.
        /// </summary>
        [Fact]
        public async Task CreateAsync_ShouldThrowUnauthorized_WhenUserMismatch()
        {
            var dto = new ReservationCreateDto { UserId = 1, BookId = 1 };
            await Assert.ThrowsAsync<UnauthorizedAccessException>(
                () => _service.CreateAsync(dto, currentUserId: 2));
        }

        /// <summary>
        /// CreateAsync should throw if duplicate pending/available exists.
        /// </summary>
        [Fact]
        public async Task CreateAsync_ShouldThrowInvalidOperation_WhenDuplicate()
        {
            var u    = new User { Name = "Gina" };
            var book = new Book { Title = "Book G" };
            _db.Users.Add(u);
            _db.Books.Add(book);
            _db.Reservations.Add(new Reservation
            {
                UserId          = u.UserId,
                BookId          = book.BookId,
                Status          = ReservationStatus.Pending,
                ReservationDate = DateTime.UtcNow,
                CreatedAt       = DateTime.UtcNow
            });
            _db.Stocks.Add(new Stock { BookId = book.BookId, Quantity = 1 });
            await _db.SaveChangesAsync();

            var dto = new ReservationCreateDto { UserId = u.UserId, BookId = book.BookId };
            await Assert.ThrowsAsync<InvalidOperationException>(
                () => _service.CreateAsync(dto, currentUserId: u.UserId));
        }

        /// <summary>
        /// CreateAsync should throw if no stock exists.
        /// </summary>
        [Fact]
        public async Task CreateAsync_ShouldThrowInvalidOperation_WhenNoStock()
        {
            var u    = new User { Name = "Hank" };
            var book = new Book { Title = "Book H" };
            _db.Users.Add(u);
            _db.Books.Add(book);
            await _db.SaveChangesAsync();

            var dto = new ReservationCreateDto { UserId = u.UserId, BookId = book.BookId };
            await Assert.ThrowsAsync<InvalidOperationException>(
                () => _service.CreateAsync(dto, currentUserId: u.UserId));
        }

        /// <summary>
        /// UpdateAsync should return true when reservation exists and update its status.
        /// </summary>
        [Fact]
        public async Task UpdateAsync_ShouldReturnTrue_WhenExists()
        {
            var res = new Reservation
            {
                UserId          = 1,
                BookId          = 1,
                Status          = ReservationStatus.Pending,
                ReservationDate = DateTime.UtcNow,
                CreatedAt       = DateTime.UtcNow
            };
            _db.Reservations.Add(res);
            await _db.SaveChangesAsync();

            var dto = new ReservationUpdateDto
            {
                ReservationId   = res.ReservationId,
                UserId          = res.UserId,
                BookId          = res.BookId,
                ReservationDate = res.ReservationDate,
                Status          = ReservationStatus.Completed
            };

            var ok = await _service.UpdateAsync(dto);
            var updated = await _db.Reservations.FindAsync(res.ReservationId);

            Assert.True(ok);
            Assert.Equal(ReservationStatus.Completed, updated!.Status);
        }

        /// <summary>
        /// UpdateAsync should return false when reservation not found.
        /// </summary>
        [Fact]
        public async Task UpdateAsync_ShouldReturnFalse_WhenNotExists()
        {
            var dto = new ReservationUpdateDto
            {
                ReservationId   = 999,
                UserId          = 1,
                BookId          = 1,
                ReservationDate = DateTime.UtcNow,
                Status          = ReservationStatus.Pending
            };

            var ok = await _service.UpdateAsync(dto);
            Assert.False(ok);
        }

        /// <summary>
        /// DeleteAsync should return true and remove record when found.
        /// </summary>
        [Fact]
        public async Task DeleteAsync_ShouldReturnTrue_WhenExists()
        {
            var res = new Reservation
            {
                UserId          = 1,
                BookId          = 1,
                Status          = ReservationStatus.Pending,
                ReservationDate = DateTime.UtcNow,
                CreatedAt       = DateTime.UtcNow
            };
            _db.Reservations.Add(res);
            await _db.SaveChangesAsync();

            var ok = await _service.DeleteAsync(res.ReservationId);
            var exists = await _db.Reservations.AnyAsync(r => r.ReservationId == res.ReservationId);

            Assert.True(ok);
            Assert.False(exists);
        }

        /// <summary>
        /// DeleteAsync should return false when no matching reservation.
        /// </summary>
        [Fact]
        public async Task DeleteAsync_ShouldReturnFalse_WhenNotExists()
        {
            var ok = await _service.DeleteAsync(999);
            Assert.False(ok);
        }

        // Dummy stub implementations for dependencies
        private class DummyHistoryService : IHistoryService
        {
            public Task LogEventAsync(
                int userId,
                string eventType,
                int? loanId = null,
                int? reservationId = null,
                CancellationToken cancellationToken = default)
                => Task.CompletedTask;

            public Task<List<HistoryReadDto>> GetHistoryForUserAsync(
                int userId,
                int page = 1,
                int pageSize = 20,
                CancellationToken cancellationToken = default)
                => Task.FromResult(new List<HistoryReadDto>());
        }

        private class DummyActivityLogService : IUserActivityLogService
        {
            public Task LogAsync(
                UserActivityLogDocument doc,
                CancellationToken cancellationToken = default)
                => Task.CompletedTask;

            public Task<List<UserActivityLogDocument>> GetByUserAsync(
                int userId,
                CancellationToken cancellationToken = default)
                => Task.FromResult(new List<UserActivityLogDocument>());
        }
    }
}