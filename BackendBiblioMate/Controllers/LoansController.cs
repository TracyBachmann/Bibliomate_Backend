using BackendBiblioMate.DTOs;
using BackendBiblioMate.Models.Enums;
using BackendBiblioMate.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BackendBiblioMate.Controllers
{
    /// <summary>
    /// Controller for managing loans.
    /// Provides endpoints to create, return, retrieve, update, and delete loans.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class LoansController : ControllerBase
    {
        private readonly ILoanService _loanService;

        /// <summary>
        /// Initializes a new instance of <see cref="LoansController"/>.
        /// </summary>
        /// <param name="loanService">Injected business service for loans.</param>
        public LoansController(ILoanService loanService)
        {
            _loanService = loanService;
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
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreateLoan(
            [FromBody] LoanCreateDto dto,
            CancellationToken cancellationToken = default)
        {
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
        /// <c>200 OK</c> with { message, reservationNotified } on success;  
        /// <c>400 BadRequest</c> with { error } on failure.
        /// </returns>
        [HttpPut("{id}/return"), Authorize(Roles = UserRoles.Librarian + "," + UserRoles.Admin)]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ReturnLoan(
            [FromRoute] int id,
            CancellationToken cancellationToken = default)
        {
            var result = await _loanService.ReturnAsync(id, cancellationToken);
            if (result.IsError)
                return BadRequest(new { error = result.Error });

            return Ok(new
            {
                message             = "Book returned successfully.",
                reservationNotified = result.Value!.ReservationNotified
            });
        }

        /// <summary>
        /// Retrieves all loans in the system.
        /// </summary>
        /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
        /// <returns>
        /// <c>200 OK</c> with a list of loans;  
        /// <c>400 BadRequest</c> with { error } on failure.
        /// </returns>
        [HttpGet, Authorize]
        [ProducesResponseType(typeof(IEnumerable<LoanReadDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetAll(
            CancellationToken cancellationToken = default)
        {
            var result = await _loanService.GetAllAsync(cancellationToken);
            if (result.IsError)
                return BadRequest(new { error = result.Error });

            return Ok(result.Value);
        }

        /// <summary>
        /// Retrieves a specific loan by its identifier.
        /// </summary>
        /// <param name="id">Identifier of the loan to retrieve.</param>
        /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
        /// <returns>
        /// <c>200 OK</c> with the loan;  
        /// <c>400 BadRequest</c> with { error } on failure.
        /// </returns>
        [HttpGet("{id}"), Authorize]
        [ProducesResponseType(typeof(LoanReadDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetById(
            [FromRoute] int id,
            CancellationToken cancellationToken = default)
        {
            var result = await _loanService.GetByIdAsync(id, cancellationToken);
            if (result.IsError)
                return BadRequest(new { error = result.Error });

            return Ok(result.Value);
        }

        /// <summary>
        /// Updates fields of an existing loan (e.g., due date).
        /// </summary>
        /// <param name="id">Identifier of the loan to update.</param>
        /// <param name="dto">Data for updating the loan.</param>
        /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
        /// <returns>
        /// <c>200 OK</c> with the updated loan;  
        /// <c>400 BadRequest</c> with { error } on failure.
        /// </returns>
        [HttpPut("{id}"), Authorize(Roles = UserRoles.Librarian + "," + UserRoles.Admin)]
        [ProducesResponseType(typeof(LoanReadDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UpdateLoan(
            [FromRoute] int id,
            [FromBody] LoanUpdateDto dto,
            CancellationToken cancellationToken = default)
        {
            var result = await _loanService.UpdateAsync(id, dto, cancellationToken);
            if (result.IsError)
                return BadRequest(new { error = result.Error });

            return Ok(result.Value);
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
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> DeleteLoan(
            [FromRoute] int id,
            CancellationToken cancellationToken = default)
        {
            var result = await _loanService.DeleteAsync(id, cancellationToken);
            if (result.IsError)
                return BadRequest(new { error = result.Error });

            return Ok(new { message = "Loan deleted successfully." });
        }
    }
}