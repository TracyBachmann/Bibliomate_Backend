using BackendBiblioMate.Data;
using BackendBiblioMate.DTOs;
using BackendBiblioMate.Interfaces;
using BackendBiblioMate.Models;
using BackendBiblioMate.Models.Enums;
using BackendBiblioMate.Models.Mongo;
using BackendBiblioMate.Models.Policies;
using BackendBiblioMate.Services.Loans;
using BackendBiblioMate.Services.Infrastructure.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using NSubstitute;
using System.Text;

namespace UnitTestsBiblioMate.Services.Loans
{
    /// <summary>
    /// Unit tests for <see cref="LoanService"/>, covering creation,
    /// return, retrieval, update, and deletion scenarios.
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
            // 1) In-memory EF Core context with encryption stubbed
            var options = new DbContextOptionsBuilder<BiblioMateDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            
            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["Encryption:Key"] = Convert.ToBase64String(
                        Encoding.UTF8.GetBytes("12345678901234567890123456789012"))
                })
                .Build();
            
            var encryptionService = new EncryptionService(config);
            _context = new BiblioMateDbContext(options, encryptionService);

            // 2) Substitute dependencies
            _stockService         = Substitute.For<IStockService>();
            _notificationService  = Substitute.For<INotificationService>();
            _historyService       = Substitute.For<IHistoryService>();
            _activityLogService   = Substitute.For<IUserActivityLogService>();

            // 3) Create service under test
            _service = new LoanService(
                _context,
                _stockService,
                _notificationService,
                _historyService,
                _activityLogService
            );
        }

        /// <summary>
        /// Creating a loan for a non-existent user should fail.
        /// </summary>
        [Fact]
        public async Task CreateAsync_ShouldFail_WhenUserNotFound()
        {
            var dto = new LoanCreateDto { UserId = 999, BookId = 1 };

            var result = await _service.CreateAsync(dto);

            Assert.True(result.IsError);
            Assert.Equal("User not found.", result.Error);
        }

        /// <summary>
        /// Creating a loan when user has max active loans should fail.
        /// </summary>
        [Fact]
        public async Task CreateAsync_ShouldFail_WhenMaxActiveLoansReached()
        {
            // Arrange: user with exactly MaxActiveLoansPerUser active loans
            var user = new User { UserId = 1 };
            _context.Users.Add(user);
            for (int i = 0; i < LoanPolicy.MaxActiveLoansPerUser; i++)
                _context.Loans.Add(new Loan { UserId = 1, ReturnDate = null });
            await _context.SaveChangesAsync();

            var dto = new LoanCreateDto { UserId = 1, BookId = 1 };

            var result = await _service.CreateAsync(dto);

            Assert.True(result.IsError);
            Assert.Contains(
                $"Maximum active loans ({LoanPolicy.MaxActiveLoansPerUser}) reached.",
                result.Error);
        }

        /// <summary>
        /// Creating a loan when no stock exists should fail with "Book unavailable.".
        /// </summary>
        [Fact]
        public async Task CreateAsync_ShouldFail_WhenStockUnavailable()
        {
            // Arrange: existing user but no stock for BookId
            _context.Users.Add(new User { UserId = 2 });
            await _context.SaveChangesAsync();

            var dto = new LoanCreateDto { UserId = 2, BookId = 123 };

            var result = await _service.CreateAsync(dto);

            Assert.True(result.IsError);
            Assert.Equal("Book unavailable.", result.Error);
        }

        /// <summary>
        /// Valid loan creation should persist the loan, decrement stock,
        /// log history and user activity.
        /// </summary>
        [Fact]
        public async Task CreateAsync_ShouldCreateLoan_WhenDataIsValid()
        {
            // Arrange: user and stock record in context
            _context.Users.Add(new User { UserId = 3 });
            var stock = new Stock { StockId = 5, BookId = 42, Quantity = 1 };
            _context.Stocks.Add(stock);
            await _context.SaveChangesAsync();

            // Ensure stockService.Decrease is called on the same stock instance
            var dto = new LoanCreateDto { UserId = 3, BookId = 42 };

            // Act
            var result = await _service.CreateAsync(dto);
            var createdLoan = _context.Loans.FirstOrDefault(l => l.UserId == 3 && l.BookId == 42);

            // Assert
            Assert.False(result.IsError);
            Assert.NotNull(createdLoan);
            Assert.Equal(createdLoan.DueDate, result.Value!.DueDate);
            _stockService.Received(1).Decrease(stock);
            await _historyService.Received(1).LogEventAsync(3, "Loan", createdLoan.LoanId);
            await _activityLogService.Received(1).LogAsync(
                Arg.Is<UserActivityLogDocument>(d =>
                    d.UserId == 3 && d.Action == "CreateLoan"));
        }

        /// <summary>
        /// Returning a non-existent loan should fail.
        /// </summary>
        [Fact]
        public async Task ReturnAsync_ShouldFail_WhenLoanNotFound()
        {
            var result = await _service.ReturnAsync(999);

            Assert.True(result.IsError);
            Assert.Equal("Loan not found.", result.Error);
        }

        /// <summary>
        /// Returning an already returned loan should fail.
        /// </summary>
        [Fact]
        public async Task ReturnAsync_ShouldFail_WhenAlreadyReturned()
        {
            _context.Loans.Add(new Loan { LoanId = 10, ReturnDate = DateTime.UtcNow });
            await _context.SaveChangesAsync();

            var result = await _service.ReturnAsync(10);

            Assert.True(result.IsError);
            Assert.Equal("Loan not found.", result.Error);
        }

        /// <summary>
        /// Returning a loan with no matching reservations should succeed
        /// without sending notifications.
        /// </summary>
        [Fact]
        public async Task ReturnAsync_ShouldReturnLoanWithoutNotification_WhenNoReservation()
        {
            // Arrange: book & stock & loan, but no reservation
            var book  = new Book { BookId = 7,  Title = "Test" };
            var stock = new Stock { StockId = 8,  BookId = 7, Book = book, Quantity = 0 };
            var loan  = new Loan  { LoanId = 20, UserId = 4, Stock = stock, StockId = 8 };

            _context.Stocks.Add(stock);
            _context.Loans.Add(loan);
            await _context.SaveChangesAsync();

            var result = await _service.ReturnAsync(20);

            Assert.False(result.IsError);
            Assert.False(result.Value!.ReservationNotified);
            Assert.Null(_context.Reservations.FirstOrDefault(r => r.AssignedStockId == 8));
            await _historyService.Received(1).LogEventAsync(4, "Return", 20);
            await _activityLogService.Received(1).LogAsync(
                Arg.Is<UserActivityLogDocument>(d => d.Action == "ReturnLoan"));
        }

        /// <summary>
        /// Returning a loan when a pending reservation exists should
        /// update the reservation and notify the reserved user.
        /// </summary>
        [Fact]
        public async Task ReturnAsync_ShouldReturnLoanAndNotify_WhenReservationExists()
        {
            // Arrange: book, stock, loan, pending reservation
            var book        = new Book       { BookId = 9,  Title = "NotifyBook" };
            var stock       = new Stock      { StockId = 11, BookId = 9,  Book = book, Quantity = 0 };
            var loan        = new Loan       { LoanId  = 30, UserId = 5,  Stock = stock, StockId = 11 };
            var reservation = new Reservation
            {
                ReservationId    = 100,
                BookId           = 9,
                UserId           = 6,
                Status           = ReservationStatus.Pending,
                CreatedAt        = DateTime.UtcNow.AddHours(-1)
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
        /// Retrieving all loans should return a successful result
        /// with the correct count.
        /// </summary>
        [Fact]
        public async Task GetAllAsync_ShouldReturnAllLoans()
        {
            _context.Loans.AddRange(
                new Loan { LoanId = 1 },
                new Loan { LoanId = 2 }
            );
            await _context.SaveChangesAsync();

            var result = await _service.GetAllAsync();

            Assert.False(result.IsError);
            Assert.Equal(2, result.Value!.Count());
        }

        /// <summary>
        /// Retrieving a non-existent loan by ID should fail.
        /// </summary>
        [Fact]
        public async Task GetByIdAsync_ShouldFail_WhenNotFound()
        {
            var result = await _service.GetByIdAsync(999);

            Assert.True(result.IsError);
            Assert.Equal("Loan not found.", result.Error);
        }

        /// <summary>
        /// Retrieving an existing loan by ID should succeed.
        /// </summary>
        [Fact]
        public async Task GetByIdAsync_ShouldReturnLoan_WhenFound()
        {
            _context.Loans.Add(new Loan { LoanId = 50 });
            await _context.SaveChangesAsync();

            var result = await _service.GetByIdAsync(50);

            Assert.False(result.IsError);
            Assert.Equal(50, result.Value!.LoanId);
        }

        /// <summary>
        /// Updating a non-existent loan should fail.
        /// </summary>
        [Fact]
        public async Task UpdateAsync_ShouldFail_WhenNotFound()
        {
            var dto = new LoanUpdateDto { DueDate = DateTime.UtcNow };
            var result = await _service.UpdateAsync(999, dto);

            Assert.True(result.IsError);
            Assert.Equal("Loan not found.", result.Error);
        }

        /// <summary>
        /// Updating the due date of an existing loan should succeed
        /// and log the update.
        /// </summary>
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
            await _historyService.Received(1).LogEventAsync(7, "Update", 60);
        }

        /// <summary>
        /// Deleting a non-existent loan should fail.
        /// </summary>
        [Fact]
        public async Task DeleteAsync_ShouldFail_WhenNotFound()
        {
            var result = await _service.DeleteAsync(999);

            Assert.True(result.IsError);
            Assert.Equal("Loan not found.", result.Error);
        }

        /// <summary>
        /// Deleting an existing loan should succeed and log the deletion.
        /// </summary>
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
            await _historyService.Received(1).LogEventAsync(8, "Delete", 70);
        }
        
        [Fact]
        public async Task ReturnAsync_LateLoan_SetsFineCorrectly()
        {
            // Arrange
            var stock = new Stock { StockId = 1, BookId = 2, Quantity = 0, Book = new Book { Title = "Test" } };
            var loan = new Loan
            {
                LoanId = 1,
                UserId = 3,
                Stock = stock,
                StockId = 1,
                DueDate = DateTime.UtcNow.AddDays(-5),
                ReturnDate = null,
                Fine = 0m
            };
            _context.Stocks.Add(stock);
            _context.Loans.Add(loan);
            await _context.SaveChangesAsync();

            // Act
            var result = await _service.ReturnAsync(1);

            // Assert
            Assert.False(result.IsError);
            Assert.Equal(5 * LoanPolicy.LateFeePerDay, result.Value!.Fine);
            _stockService.Received(1).Increase(stock);
        }
    }
}