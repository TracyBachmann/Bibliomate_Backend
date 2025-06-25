using backend.DTOs;

namespace backend.Services
{
    /// <summary>
    /// Defines business operations related to book loans.
    /// </summary>
    public interface ILoanService
    {
        /// <summary>
        /// Attempts to create a new loan for the specified user and book.
        /// </summary>
        /// <param name="dto">Data needed to create a loan (user ID, book ID).</param>
        /// <returns>
        /// A <see cref="Result{LoanCreatedResult,string}"/> wrapping either:
        /// - <see cref="LoanCreatedResult"/> on success,
        /// - an error message on failure.
        /// </returns>
        Task<Result<LoanCreatedResult, string>> CreateAsync(LoanCreateDto dto);

        /// <summary>
        /// Marks the specified loan as returned, updates inventory, notifies next reservation if any.
        /// </summary>
        /// <param name="loanId">The identifier of the loan to return.</param>
        /// <returns>
        /// A <see cref="Result{LoanReturnedResult,string}"/> wrapping either:
        /// - <see cref="LoanReturnedResult"/> on success,
        /// - an error message on failure.
        /// </returns>
        Task<Result<LoanReturnedResult, string>> ReturnAsync(int loanId);

        // TODO: add methods for GetAllAsync, GetByIdAsync, UpdateAsync, DeleteAsync, etc.
    }

    /// <summary>
    /// Holds the result data for a successfully created loan.
    /// </summary>
    public record LoanCreatedResult(DateTime DueDate);

    /// <summary>
    /// Holds the result data for a successfully returned loan.
    /// </summary>
    public record LoanReturnedResult(bool ReservationNotified);

    /// <summary>
    /// Simple discriminated union type: either a success of type <typeparamref name="TSuccess"/>,
    /// or an error of type <typeparamref name="TError"/>.
    /// </summary>
    /// <typeparam name="TSuccess">Type of the success value.</typeparam>
    /// <typeparam name="TError">Type of the error value (e.g. string).</typeparam>
    public class Result<TSuccess, TError>
    {
        /// <summary>
        /// Gets the success value, or <c>default</c> if this is an error.
        /// </summary>
        public TSuccess? Value { get; }

        /// <summary>
        /// Gets the error value, or <c>default</c> if this is a success.
        /// </summary>
        public TError? Error { get; }

        /// <summary>
        /// Returns <c>true</c> when this instance represents an error.
        /// </summary>
        public bool IsError => Error is not null;

        private Result(TSuccess? value, TError? error)
        {
            Value = value;
            Error = error;
        }

        /// <summary>
        /// Creates a successful result wrapping the given value.
        /// </summary>
        public static Result<TSuccess, TError> Ok(TSuccess value)
            => new(value, default);

        /// <summary>
        /// Creates an error result wrapping the given error.
        /// </summary>
        public static Result<TSuccess, TError> Fail(TError error)
            => new(default, error);
    }
}
