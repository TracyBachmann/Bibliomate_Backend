using System.Security.Claims;

namespace BackendBiblioMate.Helpers
{
    /// <summary>
    /// Helper methods to extract information from a <see cref="ClaimsPrincipal"/> (e.g. JWT token).
    /// </summary>
    public static class TokenHelper
    {
        private const string UserIdClaimType = ClaimTypes.NameIdentifier;

        /// <summary>
        /// Attempts to parse the user identifier from the claims principal.
        /// </summary>
        /// <param name="user">The <see cref="ClaimsPrincipal"/> containing the claims.</param>
        /// <param name="userId">
        /// When this method returns, contains the parsed user identifier if the operation succeeded;
        /// otherwise, 0.</param>
        /// <returns>
        /// <c>true</c> if the claim was present and valid; otherwise <c>false</c>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="user"/> is <c>null</c>.
        /// </exception>
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
        /// Retrieves the user identifier from the claims principal.
        /// </summary>
        /// <param name="user">The <see cref="ClaimsPrincipal"/> containing the claims.</param>
        /// <returns>The user identifier parsed from the <c>NameIdentifier</c> claim.</returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="user"/> is <c>null</c>.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// Thrown if the <c>NameIdentifier</c> claim is missing.
        /// </exception>
        /// <exception cref="FormatException">
        /// Thrown if the claim value cannot be parsed to an integer.
        /// </exception>
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