using System.Security.Claims;

namespace BackendBiblioMate.Helpers
{
    /// <summary>
    /// Provides extension methods for working with <see cref="ClaimsPrincipal"/> instances,
    /// typically representing authenticated users from JWT tokens.
    /// </summary>
    /// <remarks>
    /// This helper centralizes logic for extracting the user identifier from claims,
    /// avoiding duplication and ensuring consistent error handling across the codebase.
    /// </remarks>
    public static class TokenHelper
    {
        /// <summary>
        /// The claim type used to store the unique user identifier.
        /// Defaults to <see cref="ClaimTypes.NameIdentifier"/>.
        /// </summary>
        private const string UserIdClaimType = ClaimTypes.NameIdentifier;

        /// <summary>
        /// Attempts to safely parse the user identifier from the given claims principal.
        /// </summary>
        /// <param name="user">
        /// The <see cref="ClaimsPrincipal"/> instance containing authentication claims
        /// (for example, extracted from a JWT or cookie).
        /// </param>
        /// <param name="userId">
        /// When this method returns, contains the parsed user identifier if successful;
        /// otherwise <c>0</c>.
        /// </param>
        /// <returns>
        /// <c>true</c> if the user ID claim was found and parsed successfully; otherwise <c>false</c>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="user"/> is <c>null</c>.
        /// </exception>
        /// <example>
        /// Example usage in a controller:
        /// <code>
        /// if (User.TryGetUserId(out var userId))
        /// {
        ///     // Use userId for query
        /// }
        /// else
        /// {
        ///     return Unauthorized();
        /// }
        /// </code>
        /// </example>
        public static bool TryGetUserId(this ClaimsPrincipal user, out int userId)
        {
            if (user is null)
                throw new ArgumentNullException(nameof(user));

            userId = 0;

            var claim = user.FindFirst(UserIdClaimType);
            if (claim is null)
                return false;

            return int.TryParse(claim.Value, out userId);
        }

        /// <summary>
        /// Retrieves the user identifier from the given claims principal.
        /// </summary>
        /// <param name="user">
        /// The <see cref="ClaimsPrincipal"/> instance containing authentication claims
        /// (for example, extracted from a JWT or cookie).
        /// </param>
        /// <returns>
        /// The user identifier parsed from the <c>NameIdentifier</c> claim.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="user"/> is <c>null</c>.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// Thrown if the <c>NameIdentifier</c> claim is missing from <paramref name="user"/>.
        /// </exception>
        /// <exception cref="FormatException">
        /// Thrown if the claim value exists but cannot be parsed as a valid integer.
        /// </exception>
        /// <example>
        /// Example usage in a controller:
        /// <code>
        /// var userId = User.GetUserId(); // Will throw if claim is missing or invalid
        /// var reports = await _reportService.GetByUserIdAsync(userId, ct);
        /// </code>
        /// </example>
        public static int GetUserId(this ClaimsPrincipal user)
        {
            if (user is null)
                throw new ArgumentNullException(nameof(user));

            var claim = user.FindFirst(UserIdClaimType)
                        ?? throw new InvalidOperationException(
                            $"Claim '{UserIdClaimType}' not found.");

            if (!int.TryParse(claim.Value, out var userId))
                throw new FormatException(
                    $"Claim '{UserIdClaimType}' has invalid integer value '{claim.Value}'.");

            return userId;
        }
    }
}
