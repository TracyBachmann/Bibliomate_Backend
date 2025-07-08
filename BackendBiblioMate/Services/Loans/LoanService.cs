using Microsoft.EntityFrameworkCore;
using BackendBiblioMate.Data;
using BackendBiblioMate.DTOs;
using BackendBiblioMate.Interfaces;
using BackendBiblioMate.Models;
using BackendBiblioMate.Models.Enums;
using BackendBiblioMate.Models.Mongo;
using BackendBiblioMate.Models.Policies;

namespace BackendBiblioMate.Services.Loans
{
    /// <summary>
    /// Service for managing book loans.
    /// </summary>
    public class LoanService : ILoanService
    {
        private readonly BiblioMateDbContext _context;
        private readonly IStockService _stockService;
        private readonly INotificationService _notificationService;
        private readonly IHistoryService _historyService;
        private readonly IUserActivityLogService _activityLogService;

        public LoanService(
            BiblioMateDbContext context,
            IStockService stockService,
            INotificationService notificationService,
            IHistoryService historyService,
            IUserActivityLogService activityLogService)
        {
            _context             = context               ?? throw new ArgumentNullException(nameof(context));
            _stockService        = stockService          ?? throw new ArgumentNullException(nameof(stockService));
            _notificationService = notificationService   ?? throw new ArgumentNullException(nameof(notificationService));
            _historyService      = historyService        ?? throw new ArgumentNullException(nameof(historyService));
            _activityLogService  = activityLogService    ?? throw new ArgumentNullException(nameof(activityLogService));
        }

        public async Task<Result<LoanCreatedResult, string>> CreateAsync(
            LoanCreateDto dto,
            CancellationToken cancellationToken = default)
        {
            var user = await _context.Users.FindAsync(new object[]{ dto.UserId }, cancellationToken);
            if (user is null)
                return Result<LoanCreatedResult, string>.Fail("User not found.");

            var activeCount = await _context.Loans
                .CountAsync(l => l.UserId == dto.UserId && l.ReturnDate == null, cancellationToken);
            if (activeCount >= LoanPolicy.MaxActiveLoansPerUser)
                return Result<LoanCreatedResult, string>.Fail(
                    $"Maximum active loans ({LoanPolicy.MaxActiveLoansPerUser}) reached.");

            var stock = await _context.Stocks
                .FirstOrDefaultAsync(s => s.BookId == dto.BookId, cancellationToken);
            if (stock is null || stock.Quantity <= 0)
                return Result<LoanCreatedResult, string>.Fail("Book unavailable.");

            var now = DateTime.UtcNow;
            var loan = new Loan
            {
                UserId   = dto.UserId,
                BookId   = dto.BookId,
                StockId  = stock.StockId,
                LoanDate = now,
                DueDate  = now.AddDays(LoanPolicy.DefaultLoanDurationDays)
            };

            _context.Loans.Add(loan);
            _stockService.Decrease(stock);
            await _context.SaveChangesAsync(cancellationToken);

            await _historyService.LogEventAsync(
                dto.UserId,
                eventType: "Loan",
                loanId:    loan.LoanId,
                cancellationToken: cancellationToken);

            await _activityLogService.LogAsync(
                new UserActivityLogDocument
                {
                    UserId  = dto.UserId,
                    Action  = "CreateLoan",
                    Details = $"LoanId={loan.LoanId}, BookId={dto.BookId}"
                },
                cancellationToken);

            return Result<LoanCreatedResult, string>.Ok(
                new LoanCreatedResult { DueDate = loan.DueDate });
        }

        /// <summary>
        /// Marks a loan as returned and calculates any fine for late return.
        /// </summary>
        public async Task<Result<BackendBiblioMate.DTOs.LoanReturnedResult, string>> ReturnAsync(
            int loanId,
            CancellationToken cancellationToken = default)
        {
            var loan = await _context.Loans
                .Include(l => l.Stock).ThenInclude(s => s.Book)
                .FirstOrDefaultAsync(l => l.LoanId == loanId, cancellationToken);

            if (loan is null)
                return Result<BackendBiblioMate.DTOs.LoanReturnedResult, string>.Fail("Loan not found.");
            if (loan.ReturnDate is not null)
                return Result<BackendBiblioMate.DTOs.LoanReturnedResult, string>.Fail("Loan already returned.");

            var now = DateTime.UtcNow;
            loan.ReturnDate = now;

            // Calculate fine
            var daysLate = (now.Date - loan.DueDate.Date).Days;
            loan.Fine = daysLate > 0
                ? daysLate * LoanPolicy.LateFeePerDay
                : 0m;

            _stockService.Increase(loan.Stock);
            await _context.SaveChangesAsync(cancellationToken);

            await _historyService.LogEventAsync(
                loan.UserId,
                eventType: "Return",
                loanId:    loan.LoanId,
                cancellationToken: cancellationToken);

            await _activityLogService.LogAsync(
                new UserActivityLogDocument
                {
                    UserId  = loan.UserId,
                    Action  = "ReturnLoan",
                    Details = $"LoanId={loan.LoanId}, Fine={loan.Fine}"
                },
                cancellationToken);

            var notified = await ProcessNextReservationAsync(loan.Stock, cancellationToken);

            return Result<BackendBiblioMate.DTOs.LoanReturnedResult, string>.Ok(
                new BackendBiblioMate.DTOs.LoanReturnedResult
                {
                    ReservationNotified = notified,
                    Fine = loan.Fine
                });
        }

        public async Task<Result<IEnumerable<Loan>, string>> GetAllAsync(
            CancellationToken cancellationToken = default)
        {
            var loans = await _context.Loans.ToListAsync(cancellationToken);
            return Result<IEnumerable<Loan>, string>.Ok(loans);
        }

        public async Task<Result<Loan, string>> GetByIdAsync(
            int loanId,
            CancellationToken cancellationToken = default)
        {
            var loan = await _context.Loans.FindAsync(new object[]{ loanId }, cancellationToken);
            if (loan is null)
                return Result<Loan, string>.Fail("Loan not found.");
            return Result<Loan, string>.Ok(loan);
        }

        public async Task<Result<Loan, string>> UpdateAsync(
            int loanId,
            LoanUpdateDto dto,
            CancellationToken cancellationToken = default)
        {
            var loan = await _context.Loans.FindAsync(new object[]{ loanId }, cancellationToken);
            if (loan is null)
                return Result<Loan, string>.Fail("Loan not found.");

            loan.DueDate = dto.DueDate;
            await _context.SaveChangesAsync(cancellationToken);

            await _historyService.LogEventAsync(
                loan.UserId,
                eventType: "Update",
                loanId:    loan.LoanId,
                cancellationToken: cancellationToken);

            await _activityLogService.LogAsync(
                new UserActivityLogDocument
                {
                    UserId  = loan.UserId,
                    Action  = "UpdateLoan",
                    Details = $"LoanId={loan.LoanId}, DueDate={dto.DueDate}"
                },
                cancellationToken);

            return Result<Loan, string>.Ok(loan);
        }

        public async Task<Result<bool, string>> DeleteAsync(
            int loanId,
            CancellationToken cancellationToken = default)
        {
            var loan = await _context.Loans.FindAsync(new object[]{ loanId }, cancellationToken);
            if (loan is null)
                return Result<bool, string>.Fail("Loan not found.");

            _context.Loans.Remove(loan);
            await _context.SaveChangesAsync(cancellationToken);

            await _historyService.LogEventAsync(
                loan.UserId,
                eventType: "Delete",
                loanId:    loan.LoanId,
                cancellationToken: cancellationToken);

            await _activityLogService.LogAsync(
                new UserActivityLogDocument
                {
                    UserId  = loan.UserId,
                    Action  = "DeleteLoan",
                    Details = $"LoanId={loan.LoanId}"
                },
                cancellationToken);

            return Result<bool, string>.Ok(true);
        }

        private async Task<bool> ProcessNextReservationAsync(
            Stock stock,
            CancellationToken cancellationToken)
        {
            var next = await _context.Reservations
                .Where(r => r.BookId == stock.BookId && r.Status == ReservationStatus.Pending)
                .OrderBy(r => r.CreatedAt)
                .FirstOrDefaultAsync(cancellationToken);

            if (next is null)
                return false;

            next.AssignedStockId = stock.StockId;
            next.Status = ReservationStatus.Available;
            await _context.SaveChangesAsync(cancellationToken);

            var message = $"The book '{stock.Book.Title}' is now available.";
            await _notificationService.NotifyUser(next.UserId, message, cancellationToken);

            _stockService.Decrease(stock);
            await _context.SaveChangesAsync(cancellationToken);

            return true;
        }
    }
}