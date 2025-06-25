using backend.Data;
using backend.DTOs;
using backend.Models;
using backend.Models.Enums;
using backend.Models.Policies;
using backend.Models.Mongo;
using Microsoft.EntityFrameworkCore;

namespace backend.Services
{
    /// <summary>
    /// Implements <see cref="ILoanService"/> by coordinating EF Core, stock, notification,
    /// history and audit-log services.
    /// </summary>
    public class LoanService : ILoanService
    {
        private readonly BiblioMateDbContext    _context;
        private readonly IStockService          _stockService;
        private readonly INotificationService   _notificationService;
        private readonly IHistoryService        _historyService;
        private readonly IUserActivityLogService _activityLogService;

        /// <summary>
        /// Initializes a new instance of <see cref="LoanService"/>.
        /// </summary>
        /// <param name="context">EF Core DB context.</param>
        /// <param name="stockService">Service to adjust stock levels.</param>
        /// <param name="notificationService">Service to send user notifications.</param>
        /// <param name="historyService">Service to record domain history events.</param>
        /// <param name="activityLogService">Service to record user activity logs.</param>
        public LoanService(
            BiblioMateDbContext     context,
            IStockService           stockService,
            INotificationService    notificationService,
            IHistoryService         historyService,
            IUserActivityLogService activityLogService)
        {
            _context              = context;
            _stockService         = stockService;
            _notificationService  = notificationService;
            _historyService       = historyService;
            _activityLogService   = activityLogService;
        }

        /// <inheritdoc/>
        public async Task<Result<LoanCreatedResult, string>> CreateAsync(LoanCreateDto dto)
        {
            var user = await _context.Users.FindAsync(dto.UserId);
            if (user is null)
                return Result<LoanCreatedResult, string>.Fail("User not found.");

            var activeCount = await _context.Loans
                .CountAsync(l => l.UserId == dto.UserId && l.ReturnDate == null);
            if (activeCount >= LoanPolicy.MaxActiveLoansPerUser)
                return Result<LoanCreatedResult, string>
                    .Fail($"The maximum number of active loans ({LoanPolicy.MaxActiveLoansPerUser}) is already reached.");

            var stock = await _context.Stocks
                .FirstOrDefaultAsync(s => s.BookId == dto.BookId);
            if (stock is null || stock.Quantity <= 0)
                return Result<LoanCreatedResult, string>.Fail("The requested book is currently unavailable.");

            var loan = new Loan
            {
                UserId   = dto.UserId,
                BookId   = dto.BookId,
                LoanDate = DateTime.UtcNow,
                DueDate  = DateTime.UtcNow.AddDays(LoanPolicy.DefaultLoanDurationDays)
            };

            _context.Loans.Add(loan);
            _stockService.Decrease(stock);
            await _context.SaveChangesAsync();

            // Record domain history event
            await _historyService.LogEventAsync(dto.UserId, "Loan", loan.LoanId);

            // Record user-action audit log
            await _activityLogService.LogAsync(new UserActivityLogDocument
            {
                UserId  = dto.UserId,
                Action  = "CreateLoan",
                Details = $"LoanId={loan.LoanId}, BookId={dto.BookId}"
            });

            return Result<LoanCreatedResult, string>.Ok(new LoanCreatedResult(loan.DueDate));
        }

        /// <inheritdoc/>
        public async Task<Result<LoanReturnedResult, string>> ReturnAsync(int loanId)
        {
            var loan = await _context.Loans
                .Include(l => l.Stock)
                    .ThenInclude(s => s.Book)
                .FirstOrDefaultAsync(l => l.LoanId == loanId);

            if (loan is null)
                return Result<LoanReturnedResult, string>.Fail("Loan not found.");

            if (loan.ReturnDate is not null)
                return Result<LoanReturnedResult, string>.Fail("Book already returned.");

            // Mark returned
            loan.ReturnDate = DateTime.UtcNow;
            var stock = loan.Stock!;
            stock.IsAvailable = true;

            // Find next pending reservation
            var nextReservation = await _context.Reservations
                .Where(r => r.BookId == stock.BookId && r.Status == ReservationStatus.Pending)
                .OrderBy(r => r.CreatedAt)
                .FirstOrDefaultAsync();

            var notified = false;
            if (nextReservation is not null)
            {
                nextReservation.AssignedStockId = stock.StockId;
                nextReservation.Status          = ReservationStatus.Available;
                stock.IsAvailable                = false;

                // Notify the waiting user
                await _notificationService.NotifyUser(
                    nextReservation.UserId,
                    $"📚 The book '{stock.Book.Title}' is now available for you. You have 48h to pick it up."
                );
                notified = true;
            }

            await _context.SaveChangesAsync();

            // Record domain history
            await _historyService.LogEventAsync(loan.UserId, "Return", loan.LoanId);

            // Record user-action audit log
            await _activityLogService.LogAsync(new UserActivityLogDocument
            {
                UserId  = loan.UserId,
                Action  = "ReturnLoan",
                Details = $"LoanId={loan.LoanId}"
            });

            return Result<LoanReturnedResult, string>.Ok(new LoanReturnedResult(notified));
        }
    }
}
