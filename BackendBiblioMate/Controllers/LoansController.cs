using System.Dynamic;
using System.Security.Claims;
using BackendBiblioMate.DTOs;
using BackendBiblioMate.Interfaces;
using BackendBiblioMate.Models.Enums;
using BackendBiblioMate.Models.Policies;
using BackendBiblioMate.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;

namespace BackendBiblioMate.Controllers
{
    [ApiController]
    [Route("api/[controller]"), Route("api/v{version:apiVersion}/[controller]")]
    [ApiVersion("1.0")]
    [Produces("application/json")]
    public class LoansController : ControllerBase
    {
        private readonly ILoanService _loanService;
        private readonly ILogger<LoansController> _logger;
        private readonly IWebHostEnvironment _env;
        private readonly BiblioMateDbContext _db; // utilisé pour les lectures (mapping)

        public LoansController(
            ILoanService loanService,
            ILogger<LoansController> logger,
            IWebHostEnvironment env,
            BiblioMateDbContext db)
        {
            _loanService = loanService ?? throw new ArgumentNullException(nameof(loanService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _env = env ?? throw new ArgumentNullException(nameof(env));
            _db = db ?? throw new ArgumentNullException(nameof(db));
        }

        // ---------- Helpers ----------
        private static int? TryGetUserIdFromClaims(ClaimsPrincipal user)
        {
            var claim = user.FindFirst(ClaimTypes.NameIdentifier)
                        ?? user.FindFirst("nameid")
                        ?? user.FindFirst("sub")
                        ?? user.FindFirst("uid");
            return int.TryParse(claim?.Value, out var id) ? id : null;
        }

        private async Task<LoanReadDto> ToDtoAsync(Models.Loan l, CancellationToken ct)
        {
            // User
            string userName;
            if (l.User != null)
                userName = $"{l.User.FirstName} {l.User.LastName}".Trim();
            else
            {
                var u = await _db.Users.AsNoTracking()
                    .Where(x => x.UserId == l.UserId)
                    .Select(x => new { x.FirstName, x.LastName })
                    .FirstOrDefaultAsync(ct);
                userName = u != null ? $"{u.FirstName} {u.LastName}".Trim() : string.Empty;
            }

            // Book
            string bookTitle;
            if (l.Book != null)
                bookTitle = l.Book.Title;
            else
            {
                var b = await _db.Books.AsNoTracking()
                    .Where(x => x.BookId == l.BookId)
                    .Select(x => new { x.Title })
                    .FirstOrDefaultAsync(ct);
                bookTitle = b?.Title ?? string.Empty;
            }

            return new LoanReadDto
            {
                LoanId = l.LoanId,
                UserId = l.UserId,
                UserName = userName,
                BookId = l.BookId,
                BookTitle = bookTitle,
                LoanDate = l.LoanDate,
                DueDate = l.DueDate,
                ReturnDate = l.ReturnDate,
                Fine = l.Fine
            };
        }

        // ---------- Endpoints ----------

        /// <summary>Crée un prêt.</summary>
        [HttpPost]
        [Authorize]
        [MapToApiVersion("1.0")]
        [SwaggerOperation(Summary = "Creates a new loan (v1)", Tags = new[] { "Loans" })]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreateLoan([FromBody] LoanCreateDto dto, CancellationToken ct = default)
        {
            if (dto == null || dto.BookId <= 0)
                return BadRequest(new { error = "Invalid payload." });

            try
            {
                // Qui emprunte ?
                var isStaff = User.IsInRole(UserRoles.Librarian) || User.IsInRole(UserRoles.Admin);
                int? userIdFromToken = TryGetUserIdFromClaims(User);
                if (!isStaff && userIdFromToken is null)
                    return Unauthorized(new { error = "Authenticated user id not found in token." });

                var effectiveUserId = isStaff && dto.UserId > 0
                    ? dto.UserId
                    : userIdFromToken!.Value;

                var result = await _loanService.CreateAsync(
                    new LoanCreateDto { UserId = effectiveUserId, BookId = dto.BookId }, ct);

                if (result.IsError)
                    return BadRequest(new { error = result.Error });

                return Ok(new { message = "Loan created successfully.", dueDate = result.Value!.DueDate });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "CreateLoan failed. Payload={@dto}", dto);
                return StatusCode(500, new
                {
                    error = "InternalError",
                    message = "An unexpected error occurred. Please try again later.",
                    details = _env.IsDevelopment() ? ex.ToString() : null
                });
            }
        }

        /// <summary>
        /// Endpoint de DIAGNOSTIC : état du stock & prêts actifs pour un BookId.
        /// </summary>
        [HttpGet("debug/{bookId:int}")]
        [Authorize]
        [MapToApiVersion("1.0")]
        public async Task<IActionResult> DebugBook(int bookId, CancellationToken ct = default)
        {
            try
            {
                var uid = TryGetUserIdFromClaims(User);
                var stock = await _db.Stocks.AsNoTracking().FirstOrDefaultAsync(s => s.BookId == bookId, ct);
                var activeOnBook = await _db.Loans.AsNoTracking()
                    .CountAsync(l => l.BookId == bookId && l.ReturnDate == null, ct);
                var activeForUser = uid.HasValue
                    ? await _db.Loans.AsNoTracking().CountAsync(l => l.UserId == uid && l.ReturnDate == null, ct)
                    : 0;

                return Ok(new
                {
                    bookId,
                    stockQty = stock?.Quantity ?? 0,
                    activeLoansOnBook = activeOnBook,
                    activeLoansForCurrentUser = activeForUser,
                    policyMaxPerUser = LoanPolicy.MaxActiveLoansPerUser
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Loans debug failed for bookId={BookId}", bookId);
                return StatusCode(500, new
                {
                    error = "InternalError",
                    message = "Debug failed.",
                    details = _env.IsDevelopment() ? ex.ToString() : null
                });
            }
        }

        /// <summary>Marque un prêt comme rendu.</summary>
        [HttpPut("{id}/return")]
        [Authorize(Roles = UserRoles.Librarian + "," + UserRoles.Admin)]
        [MapToApiVersion("1.0")]
        [SwaggerOperation(Summary = "Returns a book for an existing loan (v1)", Tags = new[] { "Loans" })]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ReturnLoan([FromRoute] int id, CancellationToken cancellationToken = default)
        {
            if (id <= 0) return BadRequest(new { error = "Invalid loan ID." });

            var result = await _loanService.ReturnAsync(id, cancellationToken);
            if (result.IsError) return BadRequest(new { error = result.Error });

            return Ok(new
            {
                message = "Book returned successfully.",
                reservationNotified = result.Value!.ReservationNotified,
                fine = result.Value.Fine
            });
        }

        /// <summary>Liste tous les prêts.</summary>
        [HttpGet]
        [Authorize]
        [MapToApiVersion("1.0")]
        [SwaggerOperation(Summary = "Retrieves all loans (v1)", Tags = new[] { "Loans" })]
        [ProducesResponseType(typeof(IEnumerable<LoanReadDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll(CancellationToken ct = default)
        {
            var result = await _loanService.GetAllAsync(ct);
            if (result.IsError) return BadRequest(new { error = result.Error });

            // Mapping manuel
            var dtos = new List<LoanReadDto>();
            foreach (var l in result.Value)
                dtos.Add(await ToDtoAsync(l, ct));

            return Ok(dtos);
        }

        /// <summary>Récupère un prêt par ID.</summary>
        [HttpGet("{id}")]
        [Authorize]
        [MapToApiVersion("1.0")]
        [ProducesResponseType(typeof(LoanReadDto), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetById([FromRoute] int id, CancellationToken ct = default)
        {
            if (id <= 0) return BadRequest(new { error = "Invalid loan ID." });

            var result = await _loanService.GetByIdAsync(id, ct);
            if (result.IsError) return BadRequest(new { error = result.Error });

            var dto = await ToDtoAsync(result.Value, ct);
            return Ok(dto);
        }

        /// <summary>Met à jour un prêt.</summary>
        [HttpPut("{id}")]
        [Authorize(Roles = UserRoles.Librarian + "," + UserRoles.Admin)]
        [MapToApiVersion("1.0")]
        [ProducesResponseType(typeof(LoanReadDto), StatusCodes.Status200OK)]
        public async Task<IActionResult> UpdateLoan([FromRoute] int id, [FromBody] LoanUpdateDto dto, CancellationToken ct = default)
        {
            if (id <= 0) return BadRequest(new { error = "Invalid loan ID." });
            if (dto == null || !ModelState.IsValid) return BadRequest(new { error = "Invalid payload." });

            var result = await _loanService.UpdateAsync(id, dto, ct);
            if (result.IsError) return BadRequest(new { error = result.Error });

            var updatedDto = await ToDtoAsync(result.Value, ct);
            return Ok(updatedDto);
        }

        /// <summary>Supprime un prêt.</summary>
        [HttpDelete("{id}")]
        [Authorize(Roles = UserRoles.Librarian + "," + UserRoles.Admin)]
        [MapToApiVersion("1.0")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        public async Task<IActionResult> DeleteLoan([FromRoute] int id, CancellationToken ct = default)
        {
            if (id <= 0) return BadRequest(new { error = "Invalid loan ID." });

            var result = await _loanService.DeleteAsync(id, ct);
            if (result.IsError) return BadRequest(new { error = result.Error });

            return Ok(new { message = "Loan deleted successfully." });
        }
        
        /// <summary>
        /// Indique si l'utilisateur courant a déjà un prêt actif pour ce livre.
        /// </summary>
        [HttpGet("active/me/{bookId:int}")]
        [Authorize]
        [MapToApiVersion("1.0")]
        [SwaggerOperation(
            Summary = "Checks if current user has an active loan for the book (v1)",
            Tags = new[] { "Loans" }
        )]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        public async Task<IActionResult> HasActiveForMe(
            [FromRoute] int bookId,
            CancellationToken ct = default)
        {
            var uid = TryGetUserIdFromClaims(User);
            if (uid is null) return Unauthorized(new { error = "Authenticated user id not found in token." });

            var loan = await _db.Loans
                .AsNoTracking()
                .Where(l => l.UserId == uid.Value && l.BookId == bookId && l.ReturnDate == null)
                .OrderByDescending(l => l.LoanDate)
                .FirstOrDefaultAsync(ct);

            if (loan is null)
                return Ok(new { hasActive = false });

            return Ok(new { hasActive = true, dueDate = loan.DueDate });
        }
    }
}
