using System.Security.Claims;
using BackendBiblioMate.Helpers;

namespace UnitTestsBiblioMate.Helpers
{
    /// <summary>
    /// Unit tests for <see cref="TokenHelper"/> extension methods.
    /// Covers behavior for extracting <c>UserId</c> from a <see cref="ClaimsPrincipal"/>.
    /// </summary>
    public class TokenHelperTests
    {
        /// <summary>
        /// Verifies that <see cref="TokenHelper.TryGetUserId"/> 
        /// throws an <see cref="ArgumentNullException"/> when the provided principal is <c>null</c>.
        /// </summary>
        [Fact]
        public void TryGetUserId_NullPrincipal_Throws()
        {
            Assert.Throws<ArgumentNullException>(
                () => TokenHelper.TryGetUserId(null!, out _));
        }

        /// <summary>
        /// Verifies that <see cref="TokenHelper.TryGetUserId"/> 
        /// returns <c>false</c> and sets <c>id = 0</c> when the <c>NameIdentifier</c> claim is absent.
        /// </summary>
        [Fact]
        public void TryGetUserId_NoClaim_ReturnsFalse()
        {
            var user = new ClaimsPrincipal(new ClaimsIdentity());
            var result = user.TryGetUserId(out var id);
            Assert.False(result);
            Assert.Equal(0, id);
        }

        /// <summary>
        /// Verifies that <see cref="TokenHelper.TryGetUserId"/> 
        /// returns <c>false</c> and sets <c>id = 0</c> when the <c>NameIdentifier</c> claim is not a valid integer.
        /// </summary>
        [Fact]
        public void TryGetUserId_InvalidNumber_ReturnsFalse()
        {
            var identity = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, "NaN")
            });
            var user = new ClaimsPrincipal(identity);

            var result = user.TryGetUserId(out var id);

            Assert.False(result);
            Assert.Equal(0, id);
        }

        /// <summary>
        /// Verifies that <see cref="TokenHelper.TryGetUserId"/> 
        /// returns <c>true</c> and outputs the correct integer value when the claim is valid.
        /// </summary>
        [Fact]
        public void TryGetUserId_ValidClaim_ReturnsTrue()
        {
            var identity = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, "42")
            });
            var user = new ClaimsPrincipal(identity);

            var result = user.TryGetUserId(out var id);

            Assert.True(result);
            Assert.Equal(42, id);
        }

        /// <summary>
        /// Verifies that <see cref="TokenHelper.GetUserId"/> 
        /// throws an <see cref="ArgumentNullException"/> when the provided principal is <c>null</c>.
        /// </summary>
        [Fact]
        public void GetUserId_NullPrincipal_Throws()
        {
            Assert.Throws<ArgumentNullException>(
                () => TokenHelper.GetUserId(null!));
        }

        /// <summary>
        /// Verifies that <see cref="TokenHelper.GetUserId"/> 
        /// throws an <see cref="InvalidOperationException"/> when the <c>NameIdentifier</c> claim is missing.
        /// </summary>
        [Fact]
        public void GetUserId_NoClaim_Throws()
        {
            var user = new ClaimsPrincipal(new ClaimsIdentity());

            Assert.Throws<InvalidOperationException>(
                () => user.GetUserId());
        }

        /// <summary>
        /// Verifies that <see cref="TokenHelper.GetUserId"/> 
        /// throws a <see cref="FormatException"/> when the <c>NameIdentifier</c> claim is present but not a valid integer.
        /// </summary>
        [Fact]
        public void GetUserId_BadFormat_Throws()
        {
            var identity = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, "not-int")
            });
            var user = new ClaimsPrincipal(identity);

            Assert.Throws<FormatException>(
                () => user.GetUserId());
        }

        /// <summary>
        /// Verifies that <see cref="TokenHelper.GetUserId"/> 
        /// successfully parses and returns the <c>UserId</c> when the claim contains a valid integer.
        /// </summary>
        [Fact]
        public void GetUserId_Valid_ReturnsInt()
        {
            var identity = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, "123")
            });
            var user = new ClaimsPrincipal(identity);

            var id = user.GetUserId();

            Assert.Equal(123, id);
        }
    }
}
