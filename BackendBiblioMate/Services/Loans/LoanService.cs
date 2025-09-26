using Microsoft.EntityFrameworkCore;
using BackendBiblioMate.Data;
using BackendBiblioMate.DTOs;
using BackendBiblioMate.Interfaces;
using BackendBiblioMate.Models;
using BackendBiblioMate.Models.Enums;
using BackendBiblioMate.Models.Policies;

namespace BackendBiblioMate.Services.Loans
{
    /// <summary>
    /// Provides business logic for managing book loans,
    /// including creation, return handling, updating, deletion,
    /// and reservation processing.
    /// </summary>
    public class LoanService : ILoanService
    {
        private readonly BiblioMateDbContext _context;
        private readonly IStockService _stockService;
        private readonly INotificationService _notificationService;
        private readonly IHistoryService _historyService;
        private readonly IUserActivityLogService _activityLogService;
        private readonly ILogger<LoanService> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="LoanService"/> class.
        /// </summary>
        /// <param name="context">Database context for persistence.</param>
        /// <param name="stockService">Service for adjusting stock availability.</param>
        /// <param name="notificationService">Service for sending user notifications.</param>
        /// <param name="historyService">Service for recording historical events.</param>
        /// <param name="activityLogService">Service for recording user activities.</param>
        /// <param name="logger">Logger for diagnostics and error tracking.</param>
        public LoanService(
            BiblioMateDbContext context,
            IStockService stockService,
            INotificationService notificationService,
            IHistoryService historyService,
            IUserActivityLogService activityLogService,
            ILogger<LoanService> logger)
        {
            _context = context;
            _stockService = stockService;
            _notificationService = notificationService;
            _historyService = historyService;
            _activityLogService = activityLogService;
            _logger = logger;
        }

        /// <summary>
        /// Creates a new loan for a book if the user and stock meet policy requirements.
        /// </summary>
        /// <param name="dto">DTO containing the user and book identifiers.</param>
        /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
        /// <returns>
        /// A <see cref="Result{TSuccess,TError}"/> with loan due date if successful,
        /// or an error message if creation fails.
        /// </returns>
        public async Task<Result<LoanCreatedResult, string>> CreateAsync(
            LoanCreateDto dto,
            CancellationToken cancellationToken = default)
        {
            try
            {
                // Validate input
                if (!dto.UserId.HasValue || dto.UserId.Value <= 0 || dto.BookId <= 0)
                    return Result<LoanCreatedResult, string>.Fail("Utilisateur ou livre invalide.");

                // Check user existence
                var user = await _context.Users.FindAsync(new object[] { dto.UserId.Value }, cancellationToken);
                if (user is null)
                    return Result<LoanCreatedResult, string>.Fail("Utilisateur introuvable.");

                // Prevent multiple active loans for the same book
                var alreadyForSameBook = await _context.Loans.AnyAsync(l =>
                    l.UserId == dto.UserId.Value &&
                    l.BookId == dto.BookId &&
                    l.ReturnDate == null, cancellationToken);

                if (alreadyForSameBook)
                    return Result<LoanCreatedResult, string>.Fail("Vous avez déjà un emprunt en cours pour ce livre.");

                // Enforce max active loans per user
                var activeCount = await _context.Loans
                    .CountAsync(l => l.UserId == dto.UserId.Value && l.ReturnDate == null, cancellationToken);

                if (activeCount >= LoanPolicy.MaxActiveLoansPerUser)
                    return Result<LoanCreatedResult, string>.Fail($"Nombre maximal atteint ({LoanPolicy.MaxActiveLoansPerUser}).");

                // Verify stock availability
                var stock = await _context.Stocks.FirstOrDefaultAsync(s => s.BookId == dto.BookId, cancellationToken);
                if (stock is null)
                    return Result<LoanCreatedResult, string>.Fail("Livre indisponible.");

                var activeForBook = await _context.Loans
                    .CountAsync(l => l.BookId == dto.BookId && l.ReturnDate == null, cancellationToken);

                if (stock.Quantity - activeForBook <= 0)
                    return Result<LoanCreatedResult, string>.Fail("Livre indisponible.");

                // Create loan
                var now = DateTime.UtcNow;
                var loan = new Loan
                {
                    UserId = dto.UserId.Value,
                    BookId = dto.BookId,
                    StockId = stock.StockId,
                    LoanDate = now,
                    DueDate = now.AddDays(LoanPolicy.DefaultLoanDurationDays),
                    Fine = 0m
                };

                _context.Loans.Add(loan);
                _stockService.Decrease(stock);
                await _context.SaveChangesAsync(cancellationToken);

                return Result<LoanCreatedResult, string>.Ok(new LoanCreatedResult { DueDate = loan.DueDate });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "LoanService.CreateAsync failed");
                return Result<LoanCreatedResult, string>.Fail("Erreur interne. Veuillez réessayer plus tard.");
            }
        }

        /// <summary>
        /// Marks a loan as returned, applies late fines, updates stock,
        /// and notifies the next reservation if applicable.
        /// </summary>
        /// <param name="loanId">Identifier of the loan to return.</param>
        /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
        /// <returns>
        /// A <see cref="Result{TSuccess,TError}"/> with return details if successful,
        /// or an error message if not.
        /// </returns>
        public async Task<Result<LoanReturnedResult, string>> ReturnAsync(
            int loanId,
            CancellationToken cancellationToken = default)
        {
            var loan = await _context.Loans
                .Include(l => l.Stock).ThenInclude(s => s.Book)
                .FirstOrDefaultAsync(l => l.LoanId == loanId, cancellationToken);

            if (loan is null)
                return Result<LoanReturnedResult, string>.Fail("Loan not found.");
            if (loan.ReturnDate is not null)
                return Result<LoanReturnedResult, string>.Fail("Loan already returned.");

            // Mark as returned
            var now = DateTime.UtcNow;
            loan.ReturnDate = now;

            // Calculate fine
            var daysLate = (now.Date - loan.DueDate.Date).Days;
            loan.Fine = daysLate > 0 ? daysLate * LoanPolicy.LateFeePerDay : 0m;

            // Update stock
            _stockService.Increase(loan.Stock);
            await _context.SaveChangesAsync(cancellationToken);

            // Notify next reservation if any
            var notified = await ProcessNextReservationAsync(loan.Stock, cancellationToken);

            return Result<LoanReturnedResult, string>.Ok(new LoanReturnedResult
            {
                ReservationNotified = notified,
                Fine = loan.Fine
            });
        }

        /// <summary>
        /// Retrieves all loans from the system.
        /// </summary>
        public async Task<Result<IEnumerable<Loan>, string>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            var loans = await _context.Loans.ToListAsync(cancellationToken);
            return Result<IEnumerable<Loan>, string>.Ok(loans);
        }

        /// <summary>
        /// Retrieves a loan by its identifier.
        /// </summary>
        public async Task<Result<Loan, string>> GetByIdAsync(int loanId, CancellationToken cancellationToken = default)
        {
            var loan = await _context.Loans.FindAsync(new object[] { loanId }, cancellationToken);
            return loan is null
                ? Result<Loan, string>.Fail("Loan not found.")
                : Result<Loan, string>.Ok(loan);
        }

        /// <summary>
        /// Updates loan details (e.g., due date).
        /// </summary>
        public async Task<Result<Loan, string>> UpdateAsync(int loanId, LoanUpdateDto dto, CancellationToken cancellationToken = default)
        {
            var loan = await _context.Loans.FindAsync(new object[] { loanId }, cancellationToken);
            if (loan is null)
                return Result<Loan, string>.Fail("Loan not found.");

            loan.DueDate = dto.DueDate;
            await _context.SaveChangesAsync(cancellationToken);

            return Result<Loan, string>.Ok(loan);
        }

        /// <summary>
        /// Deletes a loan from the system by its identifier.
        /// </summary>
        public async Task<Result<bool, string>> DeleteAsync(int loanId, CancellationToken cancellationToken = default)
        {
            var loan = await _context.Loans.FindAsync(new object[] { loanId }, cancellationToken);
            if (loan is null)
                return Result<bool, string>.Fail("Loan not found.");

            _context.Loans.Remove(loan);
            await _context.SaveChangesAsync(cancellationToken);

            return Result<bool, string>.Ok(true);
        }

        /// <summary>
        /// Processes the next pending reservation for a book,
        /// marks it as available, and notifies the user.
        /// </summary>
        /// <param name="stock">The stock associated with the returned book.</param>
        /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
        /// <returns><c>true</c> if a reservation was processed and notified; otherwise <c>false</c>.</returns>
        private async Task<bool> ProcessNextReservationAsync(Stock stock, CancellationToken cancellationToken)
        {
            var next = await _context.Reservations
                .Where(r => r.BookId == stock.BookId && r.Status == ReservationStatus.Pending)
                .OrderBy(r => r.CreatedAt)
                .FirstOrDefaultAsync(cancellationToken);

            if (next is null) return false;

            next.AssignedStockId = stock.StockId;
            next.Status = ReservationStatus.Available;
            await _context.SaveChangesAsync(cancellationToken);

            await _notificationService.NotifyUser(next.UserId, $"The book '{stock.Book.Title}' is now available.", cancellationToken);

            _stockService.Decrease(stock);
            await _context.SaveChangesAsync(cancellationToken);

            return true;
        }
    }
}

