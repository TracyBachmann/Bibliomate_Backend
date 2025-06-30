using backend.DTOs;
using backend.Models;

namespace backend.Services
{
    /// <summary>
    /// Defines business operations related to book loans.
    /// </summary>
    public interface ILoanService
    {
        Task<Result<LoanCreatedResult, string>> CreateAsync(LoanCreateDto dto);
        Task<Result<LoanReturnedResult, string>> ReturnAsync(int loanId);
        Task<Result<IEnumerable<Loan>, string>> GetAllAsync();
        Task<Result<Loan, string>> GetByIdAsync(int loanId);
        Task<Result<Loan, string>> UpdateAsync(int loanId, LoanUpdateDto dto);
        Task<Result<bool, string>> DeleteAsync(int loanId);
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
