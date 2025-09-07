using System.Dynamic;
using System.Security.Claims;
using AutoMapper;
using BackendBiblioMate.DTOs;
using BackendBiblioMate.Models.Enums;
using BackendBiblioMate.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace BackendBiblioMate.Controllers
{
    /// <summary>
    /// Controller responsible for managing loans.
    /// Provides endpoints to create, return, retrieve, update, and delete loans.
    /// </summary>
    [ApiController]
    [Route("api/[controller]"), Route("api/v{version:apiVersion}/[controller]")]
    [ApiVersion("1.0")]
    [Produces("application/json")]
    public class LoansController : ControllerBase
    {
        private readonly ILoanService _loanService;
        private readonly IMapper _mapper;

        /// <summary>
        /// Initializes a new instance of the <see cref="LoansController"/> class.
        /// </summary>
        /// <param name="loanService">The loan service to handle business logic.</param>
        /// <param name="mapper">The AutoMapper instance for DTO conversions.</param>
        public LoansController(ILoanService loanService, IMapper mapper)
        {
            _loanService = loanService ?? throw new ArgumentNullException(nameof(loanService));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        /// <summary>
        /// Creates a new loan.
        /// </summary>
        /// <param name="dto">The loan creation data transfer object.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>
        /// 200 OK with { message, dueDate } on success;
        /// 400 BadRequest with { error } on failure.
        /// </returns>
        [HttpPost]
        [Authorize]
        [MapToApiVersion("1.0")]
        [SwaggerOperation(
            Summary = "Creates a new loan (v1)",
            Description = "Creates a new loan. Requires Librarian or Admin role.",
            Tags = new[] { "Loans" }
        )]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreateLoan(
            [FromBody] LoanCreateDto dto,
            CancellationToken cancellationToken = default)
        {
            if (dto == null || !ModelState.IsValid)
                return BadRequest(new { error = "Invalid payload." });

            // Qui emprunte ?
            var isStaff = User.IsInRole(UserRoles.Librarian) || User.IsInRole(UserRoles.Admin);
            int? userIdFromToken = TryGetUserIdFromClaims(User);

            if (!isStaff && userIdFromToken is null)
                return BadRequest(new { error = "Authenticated user id not found in token." });

            var effectiveUserId = isStaff && dto.UserId > 0
                ? dto.UserId                       // staff : peut prêter pour quelqu’un d’autre
                : userIdFromToken!.Value;          // user standard : pour lui-même

            var result = await _loanService.CreateAsync(
                new LoanCreateDto { UserId = effectiveUserId, BookId = dto.BookId },
                cancellationToken);

            if (result.IsError)
                return BadRequest(new { error = result.Error });

            return Ok(new { message = "Loan created successfully.", dueDate = result.Value!.DueDate });
        }

        private static int? TryGetUserIdFromClaims(ClaimsPrincipal user)
        {
            var claim = user.FindFirst(ClaimTypes.NameIdentifier)
                        ?? user.FindFirst("nameid")
                        ?? user.FindFirst("sub")
                        ?? user.FindFirst("uid");

            return int.TryParse(claim?.Value, out var id) ? id : null;
        }

        /// <summary>
        /// Returns a book for an existing loan.
        /// </summary>
        /// <param name="id">The identifier of the loan to return.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>
        /// 200 OK with { message, reservationNotified, fine } on success;
        /// 400 BadRequest with { error } on failure.
        /// </returns>
        [HttpPut("{id}/return")]
        [Authorize(Roles = UserRoles.Librarian + "," + UserRoles.Admin)]
        [MapToApiVersion("1.0")]
        [SwaggerOperation(
            Summary = "Returns a book for an existing loan (v1)",
            Description = "Marks a loan as returned. Requires Librarian or Admin role.",
            Tags = new[] { "Loans" }
        )]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ReturnLoan(
            [FromRoute] int id,
            CancellationToken cancellationToken = default)
        {
            if (id <= 0)
            {
                dynamic errorResponse = new ExpandoObject();
                errorResponse.error = "Invalid loan ID.";
                return BadRequest(errorResponse);
            }

            var result = await _loanService.ReturnAsync(id, cancellationToken);
            if (result.IsError)
            {
                dynamic errorResponse = new ExpandoObject();
                errorResponse.error = result.Error;
                return BadRequest(errorResponse);
            }

            dynamic successResponse = new ExpandoObject();
            successResponse.message = "Book returned successfully.";
            successResponse.reservationNotified = result.Value!.ReservationNotified;
            successResponse.fine = result.Value.Fine;
            return Ok(successResponse);
        }

        /// <summary>
        /// Retrieves all loans.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>
        /// 200 OK with list of LoanReadDto on success;
        /// 400 BadRequest with { error } on failure.
        /// </returns>
        [HttpGet]
        [Authorize]
        [MapToApiVersion("1.0")]
        [SwaggerOperation(
            Summary = "Retrieves all loans (v1)",
            Description = "Returns all loans. Requires authentication.",
            Tags = new[] { "Loans" }
        )]
        [ProducesResponseType(typeof(IEnumerable<LoanReadDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetAll(
            CancellationToken cancellationToken = default)
        {
            var result = await _loanService.GetAllAsync(cancellationToken);
            if (result.IsError)
            {
                dynamic errorResponse = new ExpandoObject();
                errorResponse.error = result.Error;
                return BadRequest(errorResponse);
            }

            var dtos = _mapper.Map<IEnumerable<LoanReadDto>>(result.Value);
            return Ok(dtos);
        }

        /// <summary>
        /// Retrieves a loan by its ID.
        /// </summary>
        /// <param name="id">The identifier of the loan to retrieve.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>
        /// 200 OK with LoanReadDto on success;
        /// 400 BadRequest with { error } on failure.
        /// </returns>
        [HttpGet("{id}")]
        [Authorize]
        [MapToApiVersion("1.0")]
        [SwaggerOperation(
            Summary = "Retrieves a loan by ID (v1)",
            Description = "Returns the loan with the specified ID. Requires authentication.",
            Tags = new[] { "Loans" }
        )]
        [ProducesResponseType(typeof(LoanReadDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetById(
            [FromRoute] int id,
            CancellationToken cancellationToken = default)
        {
            if (id <= 0)
            {
                dynamic errorResponse = new ExpandoObject();
                errorResponse.error = "Invalid loan ID.";
                return BadRequest(errorResponse);
            }

            var result = await _loanService.GetByIdAsync(id, cancellationToken);
            if (result.IsError)
            {
                dynamic errorResponse = new ExpandoObject();
                errorResponse.error = result.Error;
                return BadRequest(errorResponse);
            }

            var dto = _mapper.Map<LoanReadDto>(result.Value);
            return Ok(dto);
        }

        /// <summary>
        /// Updates an existing loan.
        /// </summary>
        /// <param name="id">The identifier of the loan to update.</param>
        /// <param name="dto">The loan update DTO.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>
        /// 200 OK with updated LoanReadDto on success;
        /// 400 BadRequest with { error } on failure.
        /// </returns>
        [HttpPut("{id}")]
        [Authorize(Roles = UserRoles.Librarian + "," + UserRoles.Admin)]
        [MapToApiVersion("1.0")]
        [SwaggerOperation(
            Summary = "Updates an existing loan (v1)",
            Description = "Updates a loan's properties. Requires Librarian or Admin role.",
            Tags = new[] { "Loans" }
        )]
        [ProducesResponseType(typeof(LoanReadDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UpdateLoan(
            [FromRoute] int id,
            [FromBody] LoanUpdateDto dto,
            CancellationToken cancellationToken = default)
        {
            if (id <= 0)
            {
                dynamic errorResponse = new ExpandoObject();
                errorResponse.error = "Invalid loan ID.";
                return BadRequest(errorResponse);
            }
            if (dto == null || !ModelState.IsValid)
            {
                dynamic errorResponse = new ExpandoObject();
                errorResponse.error = "Invalid payload.";
                return BadRequest(errorResponse);
            }

            var result = await _loanService.UpdateAsync(id, dto, cancellationToken);
            if (result.IsError)
            {
                dynamic errorResponse = new ExpandoObject();
                errorResponse.error = result.Error;
                return BadRequest(errorResponse);
            }

            var updatedDto = _mapper.Map<LoanReadDto>(result.Value);
            return Ok(updatedDto);
        }

        /// <summary>
        /// Deletes an existing loan.
        /// </summary>
        /// <param name="id">The identifier of the loan to delete.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>
        /// 200 OK with { message } on success;
        /// 400 BadRequest with { error } on failure.
        /// </returns>
        [HttpDelete("{id}")]
        [Authorize(Roles = UserRoles.Librarian + "," + UserRoles.Admin)]
        [MapToApiVersion("1.0")]
        [SwaggerOperation(
            Summary = "Deletes an existing loan (v1)",
            Description = "Deletes the loan with the specified ID. Requires Librarian or Admin role.",
            Tags = new[] { "Loans" }
        )]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> DeleteLoan(
            [FromRoute] int id,
            CancellationToken cancellationToken = default)
        {
            if (id <= 0)
            {
                dynamic errorResponse = new ExpandoObject();
                errorResponse.error = "Invalid loan ID.";
                return BadRequest(errorResponse);
            }

            var result = await _loanService.DeleteAsync(id, cancellationToken);
            if (result.IsError)
            {
                dynamic errorResponse = new ExpandoObject();
                errorResponse.error = result.Error;
                return BadRequest(errorResponse);
            }

            dynamic successResponse = new ExpandoObject();
            successResponse.message = "Loan deleted successfully.";
            return Ok(successResponse);
        }
    }
}