using System.Text;
using BackendBiblioMate.Data;
using BackendBiblioMate.DTOs;
using BackendBiblioMate.Interfaces;
using BackendBiblioMate.Models;
using BackendBiblioMate.Models.Enums;
using BackendBiblioMate.Models.Mongo;
using BackendBiblioMate.Services.Infrastructure.Security;
using BackendBiblioMate.Services.Loans;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace UnitTestsBiblioMate.Services.Loans
{
    /// <summary>
    /// Unit tests for <see cref="ReservationService"/>.
    /// Validates reservation creation, duplicate rules, stock checks,
    /// CRUD operations, and filtering logic.
    /// </summary>
    public class ReservationServiceTest
    {
        private readonly ReservationService _service;
        private readonly BiblioMateDbContext _db;

        public ReservationServiceTest()
        {
            // -------- In-memory EF Core context --------
            var options = new DbContextOptionsBuilder<BiblioMateDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            // 32-byte AES key for EncryptionService
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

            // Service under test (using dummy stubs for unrelated dependencies)
            _service = new ReservationService(
                _db,
                new DummyHistoryService(),
                new DummyActivityLogService()
            );
        }

        // ---------------- GetAllAsync ----------------

        /// <summary>
        /// Returns all reservations in the database regardless of status.
        /// </summary>
        [Fact]
        public async Task GetAllAsync_ShouldReturnAll()
        {
            var user = MakeUser("Alice", "A", "alice@example.com");
            var book = new Book { Title = "Book A" };
            _db.Users.Add(user);
            _db.Books.Add(book);
            await _db.SaveChangesAsync();

            _db.Reservations.AddRange(
                new Reservation { UserId = user.UserId, BookId = book.BookId, Status = ReservationStatus.Pending,   ReservationDate = DateTime.UtcNow, CreatedAt = DateTime.UtcNow },
                new Reservation { UserId = user.UserId, BookId = book.BookId, Status = ReservationStatus.Completed, ReservationDate = DateTime.UtcNow, CreatedAt = DateTime.UtcNow }
            );
            await _db.SaveChangesAsync();

            var list = (await _service.GetAllAsync()).ToList();

            Assert.Equal(2, list.Count);
        }

        // ---------------- GetByUserAsync ----------------

        /// <summary>
        /// Returns only Pending + Available reservations for a specific user.
        /// </summary>
        [Fact]
        public async Task GetByUserAsync_ShouldFilterByUserAndStatus()
        {
            var u1   = MakeUser("Bob", "B", "bob@example.com");
            var u2   = MakeUser("Carol", "C", "carol@example.com");
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

            Assert.Equal(2, r1.Count); // Only Pending + Available
            Assert.Single(r2);
        }

        // ---------------- GetPendingForBookAsync ----------------

        /// <summary>
        /// Returns only pending reservations for a given book.
        /// </summary>
        [Fact]
        public async Task GetPendingForBookAsync_ShouldReturnOnlyPending()
        {
            var user = MakeUser("Dan", "D", "dan@example.com");
            var b1   = new Book { Title = "Book C" };
            var b2   = new Book { Title = "Book D" };
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

        // ---------------- GetByIdAsync ----------------

        [Fact]
        public async Task GetByIdAsync_ShouldReturnNull_WhenNotFound()
        {
            var dto = await _service.GetByIdAsync(999);
            Assert.Null(dto);
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturnDto_WhenExists()
        {
            var u    = MakeUser("Eve", "E", "eve@example.com");
            var book = new Book { Title = "Book E" };
            _db.Users.Add(u);
            _db.Books.Add(book);
            await _db.SaveChangesAsync();

            var res = new Reservation { UserId = u.UserId, BookId = book.BookId, Status = ReservationStatus.Pending, ReservationDate = DateTime.UtcNow, CreatedAt = DateTime.UtcNow };
            _db.Reservations.Add(res);
            await _db.SaveChangesAsync();

            var dto = await _service.GetByIdAsync(res.ReservationId);

            Assert.NotNull(dto);
            Assert.Equal(u.UserId, dto!.UserId);
            Assert.Equal(book.BookId, dto.BookId);
        }

        // ---------------- CreateAsync ----------------

        /// <summary>
        /// Creates a reservation when there are no copies available (Quantity = 0).
        /// </summary>
        [Fact]
        public async Task CreateAsync_ShouldCreateAndReturnDto_WhenNoCopiesAvailable()
        {
            var u    = MakeUser("Frank", "F", "frank@example.com");
            var book = new Book { Title = "Book F" };
            _db.Users.Add(u);
            _db.Books.Add(book);
            await _db.SaveChangesAsync();

            _db.Stocks.Add(new Stock { BookId = book.BookId, Quantity = 0 });
            await _db.SaveChangesAsync();

            var dto = new ReservationCreateDto { UserId = u.UserId, BookId = book.BookId };
            var result = await _service.CreateAsync(dto, currentUserId: u.UserId);

            Assert.NotNull(result);
            Assert.Equal(u.UserId, result.UserId);
            Assert.Equal(book.BookId, result.BookId);
            Assert.Equal(ReservationStatus.Pending, result.Status);
        }

        /// <summary>
        /// Throws UnauthorizedAccessException if UserId does not match currentUserId.
        /// </summary>
        [Fact]
        public async Task CreateAsync_ShouldThrowUnauthorized_WhenUserMismatch()
        {
            var dto = new ReservationCreateDto { UserId = 1, BookId = 1 };
            await Assert.ThrowsAsync<UnauthorizedAccessException>(
                () => _service.CreateAsync(dto, currentUserId: 2));
        }

        /// <summary>
        /// Throws InvalidOperationException if a duplicate active reservation exists for the same user/book.
        /// </summary>
        [Fact]
        public async Task CreateAsync_ShouldThrowInvalidOperation_WhenDuplicateActiveReservationExists()
        {
            var u    = MakeUser("Gina", "G", "gina@example.com");
            var book = new Book { Title = "Book G" };
            _db.Users.Add(u);
            _db.Books.Add(book);
            await _db.SaveChangesAsync();

            _db.Reservations.Add(new Reservation { UserId = u.UserId, BookId = book.BookId, Status = ReservationStatus.Pending, ReservationDate = DateTime.UtcNow, CreatedAt = DateTime.UtcNow });
            await _db.SaveChangesAsync();

            var dto = new ReservationCreateDto { UserId = u.UserId, BookId = book.BookId };
            await Assert.ThrowsAsync<InvalidOperationException>(
                () => _service.CreateAsync(dto, currentUserId: u.UserId));
        }

        /// <summary>
        /// Creates a reservation even when no stock row exists (treated as zero copies available).
        /// </summary>
        [Fact]
        public async Task CreateAsync_ShouldCreate_WhenNoStockRowExists()
        {
            var u    = MakeUser("Hank", "H", "hank@example.com");
            var book = new Book { Title = "Book H" };
            _db.Users.Add(u);
            _db.Books.Add(book);
            await _db.SaveChangesAsync();

            var dto = new ReservationCreateDto { UserId = u.UserId, BookId = book.BookId };
            var result = await _service.CreateAsync(dto, currentUserId: u.UserId);

            Assert.NotNull(result);
            Assert.Equal(ReservationStatus.Pending, result.Status);
        }

        // ---------------- UpdateAsync ----------------

        [Fact]
        public async Task UpdateAsync_ShouldReturnTrue_WhenExists()
        {
            var res = new Reservation { UserId = 1, BookId = 1, Status = ReservationStatus.Pending, ReservationDate = DateTime.UtcNow, CreatedAt = DateTime.UtcNow };
            _db.Reservations.Add(res);
            await _db.SaveChangesAsync();

            var dto = new ReservationUpdateDto { ReservationId = res.ReservationId, UserId = res.UserId, BookId = res.BookId, ReservationDate = res.ReservationDate, Status = ReservationStatus.Completed };
            var ok = await _service.UpdateAsync(dto);

            var updated = await _db.Reservations.FindAsync(res.ReservationId);

            Assert.True(ok);
            Assert.Equal(ReservationStatus.Completed, updated!.Status);
        }

        [Fact]
        public async Task UpdateAsync_ShouldReturnFalse_WhenNotExists()
        {
            var dto = new ReservationUpdateDto { ReservationId = 999, UserId = 1, BookId = 1, ReservationDate = DateTime.UtcNow, Status = ReservationStatus.Pending };
            var ok = await _service.UpdateAsync(dto);

            Assert.False(ok);
        }

        // ---------------- DeleteAsync ----------------

        [Fact]
        public async Task DeleteAsync_ShouldReturnTrue_WhenExists()
        {
            var res = new Reservation { UserId = 1, BookId = 1, Status = ReservationStatus.Pending, ReservationDate = DateTime.UtcNow, CreatedAt = DateTime.UtcNow };
            _db.Reservations.Add(res);
            await _db.SaveChangesAsync();

            var ok = await _service.DeleteAsync(res.ReservationId);
            var exists = await _db.Reservations.AnyAsync(r => r.ReservationId == res.ReservationId);

            Assert.True(ok);
            Assert.False(exists);
        }

        [Fact]
        public async Task DeleteAsync_ShouldReturnFalse_WhenNotExists()
        {
            var ok = await _service.DeleteAsync(999);
            Assert.False(ok);
        }

        // ---------------- Dummy stubs ----------------
        private class DummyHistoryService : IHistoryService
        {
            public Task LogEventAsync(int userId, string eventType, int? loanId = null, int? reservationId = null, CancellationToken cancellationToken = default)
                => Task.CompletedTask;

            public Task<List<HistoryReadDto>> GetHistoryForUserAsync(int userId, int page = 1, int pageSize = 20, CancellationToken cancellationToken = default)
                => Task.FromResult(new List<HistoryReadDto>());
        }

        private class DummyActivityLogService : IUserActivityLogService
        {
            public Task LogAsync(UserActivityLogDocument doc, CancellationToken cancellationToken = default)
                => Task.CompletedTask;

            public Task<List<UserActivityLogDocument>> GetByUserAsync(int userId, CancellationToken cancellationToken = default)
                => Task.FromResult(new List<UserActivityLogDocument>());
        }

        // ---------------- Helpers ----------------
        private static User MakeUser(string first, string last, string email) => new User
        {
            FirstName        = first,
            LastName         = last,
            Email            = email,
            Password         = "hashed",
            Address1         = "1 Test Street",
            Phone            = "0600000000",
            Role             = UserRoles.User,
            IsEmailConfirmed = true,
            IsApproved       = true,
            SecurityStamp    = Guid.NewGuid().ToString()
        };
    }
}
