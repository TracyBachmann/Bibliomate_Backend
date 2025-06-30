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
    /// Service for managing loans.
    /// </summary>
    public class LoanService : ILoanService
    {
        private readonly BiblioMateDbContext     _context;
        private readonly IStockService           _stockService;
        private readonly INotificationService    _notificationService;
        private readonly IHistoryService         _historyService;
        private readonly IUserActivityLogService _activityLogService;

        /// <summary>
        /// Initializes a new instance of <see cref="LoanService"/>.
        /// </summary>
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

        /// <summary>
        /// Creates a new loan for a user and a book.
        /// </summary>
        /// <param name="dto">Loan creation data (UserId, BookId).</param>
        /// <returns>Operation result with due date or error message.</returns>
        public async Task<Result<LoanCreatedResult, string>> CreateAsync(LoanCreateDto dto)
        {
            var user = await _context.Users.FindAsync(dto.UserId);
            if (user is null)
                return Result<LoanCreatedResult, string>.Fail("User not found.");

            var activeCount = await _context.Loans
                .CountAsync(l => l.UserId == dto.UserId && l.ReturnDate == null);
            if (activeCount >= LoanPolicy.MaxActiveLoansPerUser)
                return Result<LoanCreatedResult, string>.Fail(
                    $"Maximum active loans ({LoanPolicy.MaxActiveLoansPerUser}) reached.");

            var stock = await _context.Stocks
                .FirstOrDefaultAsync(s => s.BookId == dto.BookId);
            if (stock is null || stock.Quantity <= 0)
                return Result<LoanCreatedResult, string>.Fail("Book unavailable.");

            var loan = new Loan
            {
                UserId   = dto.UserId,
                BookId   = dto.BookId,
                StockId  = stock.StockId,
                LoanDate = DateTime.UtcNow,
                DueDate  = DateTime.UtcNow.AddDays(LoanPolicy.DefaultLoanDurationDays)
            };

            _context.Loans.Add(loan);
            _stockService.Decrease(stock);
            await _context.SaveChangesAsync();

            await _historyService.LogEventAsync(dto.UserId, "Loan", loan.LoanId);
            await _activityLogService.LogAsync(new UserActivityLogDocument
            {
                UserId  = dto.UserId,
                Action  = "CreateLoan",
                Details = $"LoanId={loan.LoanId}, BookId={dto.BookId}"
            });

            return Result<LoanCreatedResult, string>.Ok(new LoanCreatedResult(loan.DueDate));
        }

        /// <summary>
        /// Returns a loan and notifies the next reservation if available.
        /// </summary>
        /// <param name="loanId">ID of the loan to return.</param>
        /// <returns>Operation result with notification status or error message.</returns>
        public async Task<Result<LoanReturnedResult, string>> ReturnAsync(int loanId)
        {
            var loan = await _context.Loans
                .Include(l => l.Stock).ThenInclude(s => s.Book)
                .FirstOrDefaultAsync(l => l.LoanId == loanId);

            if (loan is null)
                return Result<LoanReturnedResult, string>.Fail("Loan not found.");
            if (loan.ReturnDate is not null)
                return Result<LoanReturnedResult, string>.Fail("Already returned.");

            loan.ReturnDate = DateTime.UtcNow;
            var stock = loan.Stock!;
            stock.IsAvailable = true;

            var nextRes = await _context.Reservations
                .Where(r => r.BookId == stock.BookId && r.Status == ReservationStatus.Pending)
                .OrderBy(r => r.CreatedAt)
                .FirstOrDefaultAsync();

            var notified = false;
            if (nextRes is not null)
            {
                nextRes.AssignedStockId = stock.StockId;
                nextRes.Status          = ReservationStatus.Available;
                stock.IsAvailable       = false;

                await _notificationService.NotifyUser(
                    nextRes.UserId,
                    $"The book '{stock.Book.Title}' is now available."
                );
                notified = true;
            }

            await _context.SaveChangesAsync();
            await _historyService.LogEventAsync(loan.UserId, "Return", loan.LoanId);
            await _activityLogService.LogAsync(new UserActivityLogDocument
            {
                UserId  = loan.UserId,
                Action  = "ReturnLoan",
                Details = $"LoanId={loan.LoanId}"
            });

            return Result<LoanReturnedResult, string>.Ok(new LoanReturnedResult(notified));
        }

        /// <summary>
        /// Retrieves all loans.
        /// </summary>
        /// <returns>Operation result with list of loans or error message.</returns>
        public async Task<Result<IEnumerable<Loan>, string>> GetAllAsync()
        {
            var loans = await _context.Loans.ToListAsync();
            return Result<IEnumerable<Loan>, string>.Ok(loans);
        }

        /// <summary>
        /// Retrieves a loan by its ID.
        /// </summary>
        /// <param name="loanId">ID of the loan.</param>
        /// <returns>Operation result with loan or error message.</returns>
        public async Task<Result<Loan, string>> GetByIdAsync(int loanId)
        {
            var loan = await _context.Loans.FindAsync(loanId);
            if (loan is null)
                return Result<Loan, string>.Fail("Loan not found.");

            return Result<Loan, string>.Ok(loan);
        }

        /// <summary>
        /// Updates the due date of a loan.
        /// </summary>
        /// <param name="loanId">ID of the loan.</param>
        /// <param name="dto">Data for updating due date.</param>
        /// <returns>Operation result with updated loan or error message.</returns>
        public async Task<Result<Loan, string>> UpdateAsync(int loanId, LoanUpdateDto dto)
        {
            var loan = await _context.Loans.FindAsync(loanId);
            if (loan is null)
                return Result<Loan, string>.Fail("Loan not found.");

            loan.DueDate = dto.DueDate;
            await _context.SaveChangesAsync();

            await _historyService.LogEventAsync(loan.UserId, "Update", loan.LoanId);
            await _activityLogService.LogAsync(new UserActivityLogDocument
            {
                UserId  = loan.UserId,
                Action  = "UpdateLoan",
                Details = $"LoanId={loan.LoanId}, DueDate={dto.DueDate}"
            });

            return Result<Loan, string>.Ok(loan);
        }

        /// <summary>
        /// Deletes a loan by its ID.
        /// </summary>
        /// <param name="loanId">ID of the loan.</param>
        /// <returns>Operation result with success status or error message.</returns>
        public async Task<Result<bool, string>> DeleteAsync(int loanId)
        {
            var loan = await _context.Loans.FindAsync(loanId);
            if (loan is null)
                return Result<bool, string>.Fail("Loan not found.");

            _context.Loans.Remove(loan);
            await _context.SaveChangesAsync();

            await _historyService.LogEventAsync(loan.UserId, "Delete", loan.LoanId);
            await _activityLogService.LogAsync(new UserActivityLogDocument
            {
                UserId  = loan.UserId,
                Action  = "DeleteLoan",
                Details = $"LoanId={loan.LoanId}"
            });

            return Result<bool, string>.Ok(true);
        }
    }
}
