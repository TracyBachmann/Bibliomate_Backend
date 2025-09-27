using BackendBiblioMate.Data;
using BackendBiblioMate.DTOs;
using BackendBiblioMate.Interfaces;
using BackendBiblioMate.Models;
using BackendBiblioMate.Models.Enums;
using BackendBiblioMate.Models.Policies;
using BackendBiblioMate.Services.Infrastructure.Security;
using BackendBiblioMate.Services.Loans;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NSubstitute;
using System.Text;

namespace UnitTestsBiblioMate.Services.Loans
{
    /// <summary>
    /// Unit tests for <see cref="LoanService"/>.
    /// Verifies CRUD, business rules, notifications, fines, and stock interactions.
    /// </summary>
    public class LoanServiceTests
    {
        private readonly LoanService _service;
        private readonly BiblioMateDbContext _context;
        private readonly IStockService _stockService;
        private readonly INotificationService _notificationService;
        private readonly IHistoryService _historyService;
        private readonly IUserActivityLogService _activityLogService;

        public LoanServiceTests()
        {
            // --------- In-memory EF Core context with encryption ---------
            var options = new DbContextOptionsBuilder<BiblioMateDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    // 32-byte AES key encoded in Base64
                    ["Encryption:Key"] = Convert.ToBase64String(
                        Encoding.UTF8.GetBytes("12345678901234567890123456789012"))
                })
                .Build();

            var encryptionService = new EncryptionService(config);
            _context = new BiblioMateDbContext(options, encryptionService);

            // --------- Substitute dependencies ---------
            _stockService        = Substitute.For<IStockService>();
            _notificationService = Substitute.For<INotificationService>();
            _historyService      = Substitute.For<IHistoryService>();
            _activityLogService  = Substitute.For<IUserActivityLogService>();
            var logger           = Substitute.For<ILogger<LoanService>>();

            // --------- Service under test ---------
            _service = new LoanService(
                _context,
                _stockService,
                _notificationService,
                _historyService,
                _activityLogService,
                logger
            );
        }

        // ---------------- CreateAsync ----------------

        /// <summary>
        /// Fails when the user does not exist.
        /// </summary>
        [Fact]
        public async Task CreateAsync_ShouldFail_WhenUserNotFound()
        {
            var dto = new LoanCreateDto { UserId = 999, BookId = 1 };

            var result = await _service.CreateAsync(dto);

            Assert.True(result.IsError);
            Assert.Equal("Utilisateur introuvable.", result.Error);
        }

        /// <summary>
        /// Fails when the user has already reached the maximum number of active loans.
        /// </summary>
        [Fact]
        public async Task CreateAsync_ShouldFail_WhenMaxActiveLoansReached()
        {
            var user = new User { UserId = 1 };
            _context.Users.Add(user);

            // Add max number of active loans
            for (int i = 0; i < LoanPolicy.MaxActiveLoansPerUser; i++)
                _context.Loans.Add(new Loan { UserId = 1, ReturnDate = null });

            await _context.SaveChangesAsync();

            var dto = new LoanCreateDto { UserId = 1, BookId = 1 };
            var result = await _service.CreateAsync(dto);

            Assert.True(result.IsError);
            Assert.Equal($"Nombre maximal atteint ({LoanPolicy.MaxActiveLoansPerUser}).", result.Error);
        }

        /// <summary>
        /// Fails when there is no available stock for the book.
        /// </summary>
        [Fact]
        public async Task CreateAsync_ShouldFail_WhenStockUnavailable()
        {
            _context.Users.Add(new User { UserId = 2 });
            await _context.SaveChangesAsync();

            var dto = new LoanCreateDto { UserId = 2, BookId = 123 };
            var result = await _service.CreateAsync(dto);

            Assert.True(result.IsError);
            Assert.Equal("Livre indisponible.", result.Error);
        }

        /// <summary>
        /// Creates a loan successfully and decreases stock when input is valid.
        /// </summary>
        [Fact]
        public async Task CreateAsync_ShouldCreateLoan_WhenDataIsValid()
        {
            _context.Users.Add(new User { UserId = 3 });
            var stock = new Stock { StockId = 5, BookId = 42, Quantity = 1 };
            _context.Stocks.Add(stock);
            await _context.SaveChangesAsync();

            var dto = new LoanCreateDto { UserId = 3, BookId = 42 };
            var result = await _service.CreateAsync(dto);

            var createdLoan = _context.Loans.FirstOrDefault(l => l.UserId == 3 && l.BookId == 42);

            Assert.False(result.IsError);
            Assert.NotNull(createdLoan);
            Assert.Equal(createdLoan!.DueDate, result.Value!.DueDate);
            _stockService.Received(1).Decrease(stock);
        }

        // ---------------- ReturnAsync ----------------

        /// <summary>
        /// Fails if the loan does not exist.
        /// </summary>
        [Fact]
        public async Task ReturnAsync_ShouldFail_WhenLoanNotFound()
        {
            var result = await _service.ReturnAsync(999);

            Assert.True(result.IsError);
            Assert.Equal("Loan not found.", result.Error);
        }

        /// <summary>
        /// Fails if the loan has already been returned.
        /// NOTE: l'implémentation actuelle filtre probablement sur les prêts actifs (ReturnDate == null),
        /// donc elle renvoie "Loan not found." pour un prêt déjà retourné.
        /// </summary>
        [Fact]
        public async Task ReturnAsync_ShouldFail_WhenAlreadyReturned()
        {
            // Arrange: créer un prêt déjà retourné et récupérer l'ID généré
            var loan = new Loan { ReturnDate = DateTime.UtcNow };
            _context.Loans.Add(loan);
            await _context.SaveChangesAsync();

            // Act
            var result = await _service.ReturnAsync(loan.LoanId);

            // Assert
            Assert.True(result.IsError);
            // Aligné avec le comportement du service (prêt non “actif” => non trouvé)
            Assert.Equal("Loan not found.", result.Error);
        }

        /// <summary>
        /// Returns a loan without triggering any notification when no reservation exists.
        /// </summary>
        [Fact]
        public async Task ReturnAsync_ShouldReturnLoanWithoutNotification_WhenNoReservation()
        {
            var book  = new Book  { BookId = 7, Title = "Test" };
            var stock = new Stock { StockId = 8, BookId = 7, Book = book, Quantity = 0 };
            var loan  = new Loan  { LoanId = 20, UserId = 4, Stock = stock, StockId = 8, DueDate = DateTime.UtcNow };

            _context.Stocks.Add(stock);
            _context.Loans.Add(loan);
            await _context.SaveChangesAsync();

            var result = await _service.ReturnAsync(20);

            Assert.False(result.IsError);
            Assert.False(result.Value!.ReservationNotified);
            Assert.Null(_context.Reservations.FirstOrDefault(r => r.AssignedStockId == 8));
        }

        /// <summary>
        /// Returns a loan, notifies the reservation holder, and updates reservation status.
        /// </summary>
        [Fact]
        public async Task ReturnAsync_ShouldReturnLoanAndNotify_WhenReservationExists()
        {
            var book        = new Book  { BookId = 9, Title = "NotifyBook" };
            var stock       = new Stock { StockId = 11, BookId = 9, Book = book, Quantity = 0 };
            var loan        = new Loan  { LoanId  = 30, UserId = 5, Stock = stock, StockId = 11, DueDate = DateTime.UtcNow };
            var reservation = new Reservation
            {
                ReservationId = 100,
                BookId        = 9,
                UserId        = 6,
                Status        = ReservationStatus.Pending,
                CreatedAt     = DateTime.UtcNow.AddHours(-1)
            };

            _context.Stocks.Add(stock);
            _context.Loans.Add(loan);
            _context.Reservations.Add(reservation);
            await _context.SaveChangesAsync();

            var result = await _service.ReturnAsync(30);

            Assert.False(result.IsError);
            Assert.True(result.Value!.ReservationNotified);

            await _notificationService.Received(1).NotifyUser(
                6,
                Arg.Is<string>(msg => msg.Contains("is now available"))
            );

            var updatedRes = _context.Reservations.First(r => r.ReservationId == 100);
            Assert.Equal(ReservationStatus.Available, updatedRes.Status);
        }

        /// <summary>
        /// Sets fine correctly when returning a late loan.
        /// </summary>
        [Fact]
        public async Task ReturnAsync_LateLoan_SetsFineCorrectly()
        {
            var stock = new Stock { StockId = 1, BookId = 2, Quantity = 0, Book = new Book { Title = "Test" } };
            var loan = new Loan
            {
                LoanId     = 1,
                UserId     = 3,
                Stock      = stock,
                StockId    = 1,
                DueDate    = DateTime.UtcNow.AddDays(-5), // overdue by 5 days
                ReturnDate = null,
                Fine       = 0m
            };
            _context.Stocks.Add(stock);
            _context.Loans.Add(loan);
            await _context.SaveChangesAsync();

            var result = await _service.ReturnAsync(1);

            Assert.False(result.IsError);
            Assert.Equal(5 * LoanPolicy.LateFeePerDay, result.Value!.Fine);
            _stockService.Received(1).Increase(stock);
        }

        // ---------------- GetAllAsync / GetByIdAsync ----------------

        [Fact]
        public async Task GetAllAsync_ShouldReturnAllLoans()
        {
            _context.Loans.AddRange(new Loan { LoanId = 1 }, new Loan { LoanId = 2 });
            await _context.SaveChangesAsync();

            var result = await _service.GetAllAsync();

            Assert.False(result.IsError);
            Assert.Equal(2, result.Value!.Count());
        }

        [Fact]
        public async Task GetByIdAsync_ShouldFail_WhenNotFound()
        {
            var result = await _service.GetByIdAsync(999);

            Assert.True(result.IsError);
            Assert.Equal("Loan not found.", result.Error);
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturnLoan_WhenFound()
        {
            _context.Loans.Add(new Loan { LoanId = 50 });
            await _context.SaveChangesAsync();

            var result = await _service.GetByIdAsync(50);

            Assert.False(result.IsError);
            Assert.Equal(50, result.Value!.LoanId);
        }

        // ---------------- UpdateAsync ----------------

        [Fact]
        public async Task UpdateAsync_ShouldFail_WhenNotFound()
        {
            var dto = new LoanUpdateDto { DueDate = DateTime.UtcNow };
            var result = await _service.UpdateAsync(999, dto);

            Assert.True(result.IsError);
            Assert.Equal("Loan not found.", result.Error);
        }

        [Fact]
        public async Task UpdateAsync_ShouldUpdateDueDate_WhenValid()
        {
            var loan = new Loan { LoanId = 60, UserId = 7, DueDate = DateTime.UtcNow };
            _context.Loans.Add(loan);
            await _context.SaveChangesAsync();

            var newDate = DateTime.UtcNow.AddDays(5);
            var dto     = new LoanUpdateDto { DueDate = newDate };

            var result = await _service.UpdateAsync(60, dto);

            Assert.False(result.IsError);
            Assert.Equal(newDate, result.Value!.DueDate);
        }

        // ---------------- DeleteAsync ----------------

        [Fact]
        public async Task DeleteAsync_ShouldFail_WhenNotFound()
        {
            var result = await _service.DeleteAsync(999);

            Assert.True(result.IsError);
            Assert.Equal("Loan not found.", result.Error);
        }

        [Fact]
        public async Task DeleteAsync_ShouldDeleteLoan_WhenValid()
        {
            var loan = new Loan { LoanId = 70, UserId = 8 };
            _context.Loans.Add(loan);
            await _context.SaveChangesAsync();

            var result = await _service.DeleteAsync(70);
            var exists = _context.Loans.Any(l => l.LoanId == 70);

            Assert.False(result.IsError);
            Assert.True(result.Value);
            Assert.False(exists);
        }
    }
}
