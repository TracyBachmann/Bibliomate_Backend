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
    private readonly ILogger<LoanService> _logger; // 👈

    public LoanService(
        BiblioMateDbContext context,
        IStockService stockService,
        INotificationService notificationService,
        IHistoryService historyService,
        IUserActivityLogService activityLogService,
        ILogger<LoanService> logger) // 👈
    {
        _context = context;
        _stockService = stockService;
        _notificationService = notificationService;
        _historyService = historyService;
        _activityLogService = activityLogService;
        _logger = logger;
    }

    public async Task<Result<LoanCreatedResult, string>> CreateAsync(
    LoanCreateDto dto, CancellationToken cancellationToken = default)
{
    try
    {
        if (!dto.UserId.HasValue || dto.UserId.Value <= 0 || dto.BookId <= 0)
            return Result<LoanCreatedResult, string>.Fail("Utilisateur ou livre invalide.");

        _logger.LogInformation("LoanService.CreateAsync userId={UserId}, bookId={BookId}",
                               dto.UserId, dto.BookId);

        // --- Utilisateur ---
        var user = await _context.Users.FindAsync(new object[] { dto.UserId.Value }, cancellationToken);
        if (user is null)
            return Result<LoanCreatedResult, string>.Fail("Utilisateur introuvable.");

        // --- GARDE : même livre déjà emprunté par cet utilisateur ---
        var alreadyForSameBook = await _context.Loans.AnyAsync(l =>
            l.UserId == dto.UserId.Value &&
            l.BookId == dto.BookId &&
            l.ReturnDate == null, cancellationToken);

        if (alreadyForSameBook)
            return Result<LoanCreatedResult, string>.Fail("Vous avez déjà un emprunt en cours pour ce livre.");

        // --- Politique : nombre max de prêts actifs ---
        var activeCount = await _context.Loans
            .CountAsync(l => l.UserId == dto.UserId.Value && l.ReturnDate == null, cancellationToken);

        _logger.LogInformation("User active loans = {ActiveCount}", activeCount);

        if (activeCount >= LoanPolicy.MaxActiveLoansPerUser)
            return Result<LoanCreatedResult, string>.Fail(
                $"Nombre maximal d’emprunts actifs atteint ({LoanPolicy.MaxActiveLoansPerUser}).");

        // --- Stock / disponibilité ---
        var stock = await _context.Stocks
            .FirstOrDefaultAsync(s => s.BookId == dto.BookId, cancellationToken);

        if (stock is null)
            return Result<LoanCreatedResult, string>.Fail("Livre indisponible.");

        var activeForBook = await _context.Loans
            .CountAsync(l => l.BookId == dto.BookId && l.ReturnDate == null, cancellationToken);

        var remaining = stock.Quantity - activeForBook;
        _logger.LogInformation("Stock qty={Qty}, activeForBook={ActiveForBook}, remaining={Remaining}",
                               stock.Quantity, activeForBook, remaining);

        if (remaining <= 0)
            return Result<LoanCreatedResult, string>.Fail("Livre indisponible.");

        // --- Création ---
        var now = DateTime.UtcNow;
        var loan = new Loan
        {
            UserId     = dto.UserId.Value,
            BookId     = dto.BookId,
            StockId    = stock.StockId,
            LoanDate   = now,
            DueDate    = now.AddDays(LoanPolicy.DefaultLoanDurationDays),
            ReturnDate = null,
            Fine       = 0m
        };

        _context.Loans.Add(loan);
        _stockService.Decrease(stock); // si ta logique "disponible" utilise Stock

        await _context.SaveChangesAsync(cancellationToken);

        // --- Logs ---
        await _historyService.LogEventAsync(
            dto.UserId.Value, "Loan", loanId: loan.LoanId, cancellationToken: cancellationToken);

        await _activityLogService.LogAsync(
            new UserActivityLogDocument
            {
                UserId  = dto.UserId.Value,
                Action  = "CreateLoan",
                Details = $"LoanId={loan.LoanId}, BookId={dto.BookId}"
            },
            cancellationToken);

        _logger.LogInformation("Loan created. loanId={LoanId}, dueDate={DueDate}", loan.LoanId, loan.DueDate);

        return Result<LoanCreatedResult, string>.Ok(new LoanCreatedResult { DueDate = loan.DueDate });
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "LoanService.CreateAsync crash userId={UserId}, bookId={BookId}", dto.UserId, dto.BookId);
        return Result<LoanCreatedResult, string>.Fail("Erreur interne. Veuillez réessayer plus tard.");
    }
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