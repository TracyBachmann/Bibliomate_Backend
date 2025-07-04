using System.Security.Claims;
using BackendBiblioMate.Helpers;

namespace UnitTestsBiblioMate.Helpers
{
    /// <summary>
    /// Tests for <see cref="TokenHelper"/> extension methods.
    /// </summary>
    public class TokenHelperTests
    {
        /// <summary>
        /// TryGetUserId should throw when principal is null.
        /// </summary>
        [Fact]
        public void TryGetUserId_NullPrincipal_Throws()
        {
            Assert.Throws<ArgumentNullException>(
                () => TokenHelper.TryGetUserId(null!, out _));
        }

        /// <summary>
        /// TryGetUserId should return false and zero when claim is absent.
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
        /// TryGetUserId should return false and zero when claim is not an integer.
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
        /// TryGetUserId should return true and correct value when valid.
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
        /// GetUserId should throw when principal is null.
        /// </summary>
        [Fact]
        public void GetUserId_NullPrincipal_Throws()
        {
            Assert.Throws<ArgumentNullException>(
                () => TokenHelper.GetUserId(null!));
        }

        /// <summary>
        /// GetUserId should throw if NameIdentifier claim is missing.
        /// </summary>
        [Fact]
        public void GetUserId_NoClaim_Throws()
        {
            var user = new ClaimsPrincipal(new ClaimsIdentity());
            Assert.Throws<InvalidOperationException>(
                () => user.GetUserId());
        }

        /// <summary>
        /// GetUserId should throw if the claim value is not an integer.
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
        /// GetUserId should return the parsed integer when valid.
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