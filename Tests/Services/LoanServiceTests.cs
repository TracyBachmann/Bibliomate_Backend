using backend.Data;
using backend.DTOs;
using backend.Models;
using backend.Models.Enums;
using backend.Models.Policies;
using backend.Models.Mongo;
using backend.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using NSubstitute;
using System.Text;

namespace Tests.Services
{
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
            // In-memory EF context setup
            var options = new DbContextOptionsBuilder<BiblioMateDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            // Simple config for context (no encryption needed here)
            var config = new ConfigurationBuilder()
    .AddInMemoryCollection(new Dictionary<string, string?>
    {
        ["Encryption:Key"] = Convert.ToBase64String(Encoding.UTF8.GetBytes("12345678901234567890123456789012"))
    })
    .Build();
            var encryptionService = new EncryptionService(config);
            _context = new BiblioMateDbContext(options, encryptionService);

            // Substitutes for dependencies
            _stockService = Substitute.For<IStockService>();
            _notificationService = Substitute.For<INotificationService>();
            _historyService = Substitute.For<IHistoryService>();
            _activityLogService = Substitute.For<IUserActivityLogService>();

            _service = new LoanService(
                _context,
                _stockService,
                _notificationService,
                _historyService,
                _activityLogService
            );
        }

        [Fact]
        public async Task CreateAsync_ShouldFail_WhenUserNotFound()
        {
            // Arrange
            var dto = new LoanCreateDto { UserId = 999, BookId = 1 };

            // Act
            var result = await _service.CreateAsync(dto);

            // Assert
            Assert.True(result.IsError);
            Assert.Equal("User not found.", result.Error);
        }

        [Fact]
        public async Task CreateAsync_ShouldFail_WhenMaxActiveLoansReached()
        {
            // Arrange user and existing loans
            var user = new User { UserId = 1 };
            _context.Users.Add(user);
            for (int i = 0; i < LoanPolicy.MaxActiveLoansPerUser; i++)
                _context.Loans.Add(new Loan { UserId = 1, ReturnDate = null });
            await _context.SaveChangesAsync();

            var dto = new LoanCreateDto { UserId = 1, BookId = 1 };

            // Act
            var result = await _service.CreateAsync(dto);

            // Assert
            Assert.True(result.IsError);
            Assert.Contains($"Maximum active loans ({LoanPolicy.MaxActiveLoansPerUser}) reached.", result.Error);
        }

        [Fact]
        public async Task CreateAsync_ShouldFail_WhenStockUnavailable()
        {
            // Arrange user but missing stock
            var user = new User { UserId = 2 };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var dto = new LoanCreateDto { UserId = 2, BookId = 123 };

            // Act
            var result = await _service.CreateAsync(dto);

            // Assert
            Assert.True(result.IsError);
            Assert.Equal("Book unavailable.", result.Error);
        }

        [Fact]
        public async Task CreateAsync_ShouldCreateLoan_WhenDataIsValid()
        {
            // Arrange user and stock
            var user = new User { UserId = 3 };
            _context.Users.Add(user);

            var stock = new Stock { StockId = 5, BookId = 42, Quantity = 1 };
            _context.Stocks.Add(stock);
            await _context.SaveChangesAsync();

            var dto = new LoanCreateDto { UserId = 3, BookId = 42 };

            // Act
            var result = await _service.CreateAsync(dto);
            var createdLoan = _context.Loans.FirstOrDefault(l => l.UserId == 3 && l.BookId == 42);

            // Assert
            Assert.False(result.IsError);
            Assert.NotNull(createdLoan);
            Assert.Equal(createdLoan!.DueDate, result.Value!.DueDate);
            _stockService.Received(1).Decrease(stock);
            await _historyService.Received(1).LogEventAsync(3, "Loan", createdLoan.LoanId);
            await _activityLogService.Received(1).LogAsync(Arg.Is<UserActivityLogDocument>(d => d.UserId == 3 && d.Action == "CreateLoan"));
        }

        [Fact]
        public async Task ReturnAsync_ShouldFail_WhenLoanNotFound()
        {
            // Act
            var result = await _service.ReturnAsync(999);

            // Assert
            Assert.True(result.IsError);
            Assert.Equal("Loan not found.", result.Error);
        }

        [Fact]
        public async Task ReturnAsync_ShouldFail_WhenAlreadyReturned()
        {
            // Arrange
            var loan = new Loan { LoanId = 10, ReturnDate = DateTime.UtcNow };
            _context.Loans.Add(loan);
            await _context.SaveChangesAsync();

            // Act
            var result = await _service.ReturnAsync(10);

            // Assert
            Assert.True(result.IsError);
            Assert.Equal("Loan not found.", result.Error);
        }

        [Fact]
        public async Task ReturnAsync_ShouldReturnLoanWithoutNotification_WhenNoReservation()
        {
            // Arrange loan with stock and book
            var book = new Book { BookId = 7, Title = "Test" };
            var stock = new Stock { StockId = 8, BookId = 7, Book = book, IsAvailable = false };
            var loan = new Loan { LoanId = 20, UserId = 4, Stock = stock, StockId = 8 };
            _context.Stocks.Add(stock);
            _context.Loans.Add(loan);
            await _context.SaveChangesAsync();

            // Act
            var result = await _service.ReturnAsync(20);

            // Assert
            Assert.False(result.IsError);
            Assert.False(result.Value!.ReservationNotified);
            Assert.Null(_context.Reservations.FirstOrDefault(r => r.AssignedStockId == 8));
            await _historyService.Received(1).LogEventAsync(4, "Return", 20);
            await _activityLogService.Received(1).LogAsync(Arg.Is<UserActivityLogDocument>(d => d.Action == "ReturnLoan"));
        }

        [Fact]
        public async Task ReturnAsync_ShouldReturnLoanAndNotify_WhenReservationExists()
        {
            // Arrange loan with stock and book
            var book = new Book { BookId = 9, Title = "NotifyBook" };
            var stock = new Stock { StockId = 11, BookId = 9, Book = book, IsAvailable = false };
            var loan = new Loan { LoanId = 30, UserId = 5, Stock = stock, StockId = 11 };
            var reservation = new Reservation
            {
                ReservationId = 100,
                BookId = 9,
                UserId = 6,
                Status = ReservationStatus.Pending,
                CreatedAt = DateTime.UtcNow.AddHours(-1)
            };
            _context.Stocks.Add(stock);
            _context.Loans.Add(loan);
            _context.Reservations.Add(reservation);
            await _context.SaveChangesAsync();

            // Act
            var result = await _service.ReturnAsync(30);

            // Assert
            Assert.False(result.IsError);
            Assert.True(result.Value!.ReservationNotified);
            await _notificationService.Received(1).NotifyUser(
                6,
                Arg.Is<string>(msg => msg.Contains("is now available"))
            );
            var updatedRes = _context.Reservations.First(r => r.ReservationId == 100);
            Assert.Equal(ReservationStatus.Available, updatedRes.Status);
        }

        [Fact]
        public async Task GetAllAsync_ShouldReturnAllLoans()
        {
            // Arrange
            _context.Loans.AddRange(
                new Loan { LoanId = 1 },
                new Loan { LoanId = 2 }
            );
            await _context.SaveChangesAsync();

            // Act
            var result = await _service.GetAllAsync();

            // Assert
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
            var loan = new Loan { LoanId = 50 };
            _context.Loans.Add(loan);
            await _context.SaveChangesAsync();

            var result = await _service.GetByIdAsync(50);
            Assert.False(result.IsError);
            Assert.Equal(50, result.Value!.LoanId);
        }

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
            // Arrange
            var loan = new Loan { LoanId = 60, UserId = 7, DueDate = DateTime.UtcNow };
            _context.Loans.Add(loan);
            await _context.SaveChangesAsync();

            var newDate = DateTime.UtcNow.AddDays(5);
            var dto = new LoanUpdateDto { DueDate = newDate };

            // Act
            var result = await _service.UpdateAsync(60, dto);
            
            // Assert
            Assert.False(result.IsError);
            Assert.Equal(newDate, result.Value!.DueDate);
            await _historyService.Received(1).LogEventAsync(7, "Update", 60);
        }

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
            // Arrange
            var loan = new Loan { LoanId = 70, UserId = 8 };
            _context.Loans.Add(loan);
            await _context.SaveChangesAsync();

            // Act
            var result = await _service.DeleteAsync(70);
            var exists = _context.Loans.Any(l => l.LoanId == 70);

            // Assert
            Assert.False(result.IsError);
            Assert.True(result.Value);
            Assert.False(exists);
            await _historyService.Received(1).LogEventAsync(8, "Delete", 70);
        }
    }
}

