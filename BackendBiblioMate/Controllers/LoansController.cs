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
    /// Controller for managing loans.
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
        /// Initializes a new instance of <see cref="LoansController"/>.
        /// </summary>
        /// <param name="loanService">Injected business service for loans.</param>
        /// <param name="mapper">Injected AutoMapper instance.</param>
        public LoansController(ILoanService loanService, IMapper mapper)
        {
            _loanService = loanService ?? throw new ArgumentNullException(nameof(loanService));
            _mapper      = mapper      ?? throw new ArgumentNullException(nameof(mapper));
        }

        /// <summary>
        /// Creates a new loan.
        /// </summary>
        /// <param name="dto">Data required to create the loan.</param>
        /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
        /// <returns>
        /// <c>200 OK</c> with { message, dueDate } on success;  
        /// <c>400 BadRequest</c> with { error } on failure.
        /// </returns>
        [HttpPost, Authorize(Roles = UserRoles.Librarian + "," + UserRoles.Admin)]
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

            var result = await _loanService.CreateAsync(dto, cancellationToken);
            if (result.IsError)
                return BadRequest(new { error = result.Error });

            return Ok(new
            {
                message = "Loan created successfully.",
                dueDate = result.Value!.DueDate
            });
        }

        /// <summary>
        /// Returns a book for an existing loan.
        /// </summary>
        /// <param name="id">Identifier of the loan to return.</param>
        /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
        /// <returns>
        /// <c>200 OK</c> with { message, reservationNotified, fine } on success;  
        /// <c>400 BadRequest</c> with { error } on failure.
        /// </returns>
        [HttpPut("{id}/return"), Authorize(Roles = UserRoles.Librarian + "," + UserRoles.Admin)]
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
                return BadRequest(new { error = "Invalid loan ID." });

            var result = await _loanService.ReturnAsync(id, cancellationToken);
            if (result.IsError)
                return BadRequest(new { error = result.Error });

            return Ok(new
            {
                message = "Book returned successfully.",
                reservationNotified = result.Value!.ReservationNotified,
                fine = result.Value.Fine
            });
        }

        /// <summary>
        /// Retrieves all loans in the system.
        /// </summary>
        /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
        /// <returns>
        /// <c>200 OK</c> with a list of <see cref="LoanReadDto"/>;  
        /// <c>400 BadRequest</c> with { error } on failure.
        /// </returns>
        [HttpGet, Authorize]
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
                return BadRequest(new { error = result.Error });

            var dtos = _mapper.Map<IEnumerable<LoanReadDto>>(result.Value);
            return Ok(dtos);
        }

        /// <summary>
        /// Retrieves a specific loan by its identifier.
        /// </summary>
        /// <param name="id">Identifier of the loan to retrieve.</param>
        /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
        /// <returns>
        /// <c>200 OK</c> with a <see cref="LoanReadDto"/>;  
        /// <c>400 BadRequest</c> with { error } on failure.
        /// </returns>
        [HttpGet("{id}"), Authorize]
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
                return BadRequest(new { error = "Invalid loan ID." });

            var result = await _loanService.GetByIdAsync(id, cancellationToken);
            if (result.IsError)
                return BadRequest(new { error = result.Error });

            var dto = _mapper.Map<LoanReadDto>(result.Value);
            return Ok(dto);
        }

        /// <summary>
        /// Updates fields of an existing loan (e.g., due date).
        /// </summary>
        /// <param name="id">Identifier of the loan to update.</param>
        /// <param name="dto">Data for updating the loan.</param>
        /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
        /// <returns>
        /// <c>200 OK</c> with the updated <see cref="LoanReadDto"/>;  
        /// <c>400 BadRequest</c> with { error } on failure.
        /// </returns>
        [HttpPut("{id}"), Authorize(Roles = UserRoles.Librarian + "," + UserRoles.Admin)]
        [MapToApiVersion("1.0")]
        [SwaggerOperation(
            Summary = "Updates an existing loan (v1)",
            Description = "Updates a loan's fields. Requires Librarian or Admin role.",
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
                return BadRequest(new { error = "Invalid loan ID." });
            if (dto == null || !ModelState.IsValid)
                return BadRequest(new { error = "Invalid payload." });

            var result = await _loanService.UpdateAsync(id, dto, cancellationToken);
            if (result.IsError)
                return BadRequest(new { error = result.Error });

            var updatedDto = _mapper.Map<LoanReadDto>(result.Value);
            return Ok(updatedDto);
        }

        /// <summary>
        /// Deletes an existing loan.
        /// </summary>
        /// <param name="id">Identifier of the loan to delete.</param>
        /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
        /// <returns>
        /// <c>200 OK</c> with { message } on success;  
        /// <c>400 BadRequest</c> with { error } on failure.
        /// </returns>
        [HttpDelete("{id}"), Authorize(Roles = UserRoles.Librarian + "," + UserRoles.Admin)]
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
                return BadRequest(new { error = "Invalid loan ID." });

            var result = await _loanService.DeleteAsync(id, cancellationToken);
            if (result.IsError)
                return BadRequest(new { error = result.Error });

            return Ok(new { message = "Loan deleted successfully." });
        }
    }
}
