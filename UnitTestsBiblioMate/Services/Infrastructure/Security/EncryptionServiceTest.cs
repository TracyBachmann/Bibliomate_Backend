using System.Text;
using BackendBiblioMate.Services.Infrastructure.Security;
using Microsoft.Extensions.Configuration;
using System.Security.Cryptography;

namespace UnitTestsBiblioMate.Services.Infrastructure.Security
{
    /// <summary>
    /// Unit tests for <see cref="EncryptionService"/>.
    /// Verifies encryption/decryption behavior and constructor error handling.
    /// </summary>
    public class EncryptionServiceTest
    {
        private readonly EncryptionService _service;

        /// <summary>
        /// Initializes a new instance of <see cref="EncryptionServiceTest"/>.
        /// Prepares a deterministic 32-byte key (random but fixed seed)
        /// and configures the service with it.
        /// </summary>
        public EncryptionServiceTest()
        {
            // Create a reproducible random 32-byte key
            var key = new byte[32];
            new Random(123).NextBytes(key);

            // Encode the key in Base64 for configuration
            var base64Key = Convert.ToBase64String(key);

            var settings = new Dictionary<string, string?>
            {
                ["Encryption:Key"] = base64Key
            };

            // Build IConfiguration and initialize the service
            IConfiguration config = new ConfigurationBuilder()
                .AddInMemoryCollection(settings)
                .Build();

            _service = new EncryptionService(config);
        }

        // ---------------- Constructor validation ----------------

        /// <summary>
        /// Ensures that the constructor throws <see cref="InvalidOperationException"/>
        /// when the encryption key is completely missing from configuration.
        /// </summary>
        [Fact]
        public void Constructor_ShouldThrow_WhenKeyMissing()
        {
            IConfiguration config = new ConfigurationBuilder().Build();
            Assert.Throws<InvalidOperationException>(() => new EncryptionService(config));
        }

        /// <summary>
        /// Ensures that the constructor throws <see cref="InvalidOperationException"/>
        /// when the configured encryption key is not valid Base64.
        /// </summary>
        [Fact]
        public void Constructor_ShouldThrow_WhenKeyInvalidBase64()
        {
            var settings = new Dictionary<string, string?>
            {
                ["Encryption:Key"] = "not-base64"
            };
            IConfiguration config = new ConfigurationBuilder()
                .AddInMemoryCollection(settings)
                .Build();

            Assert.Throws<InvalidOperationException>(() => new EncryptionService(config));
        }

        /// <summary>
        /// Ensures that the constructor throws <see cref="InvalidOperationException"/>
        /// when the Base64 key decodes to a length other than 32 bytes
        /// (AES-256 requires exactly 32).
        /// </summary>
        [Fact]
        public void Constructor_ShouldThrow_WhenKeyWrongLength()
        {
            // Base64 encoding of "short" → fewer than 32 bytes
            var shortKey = Convert.ToBase64String(Encoding.UTF8.GetBytes("short"));
            IConfiguration config = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["Encryption:Key"] = shortKey
                })
                .Build();

            var ex = Assert.Throws<InvalidOperationException>(() => new EncryptionService(config));
            Assert.Contains("32 bytes", ex.Message);
        }

        // ---------------- Encrypt ----------------

        /// <summary>
        /// Encrypt should return an empty string if the input
        /// is null or empty.
        /// </summary>
        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public void Encrypt_ShouldReturnEmpty_ForNullOrEmpty(string? input)
        {
            Assert.Equal(string.Empty, _service.Encrypt(input));
        }

        // ---------------- Decrypt ----------------

        /// <summary>
        /// Decrypt should return an empty string if the input
        /// is null or empty.
        /// </summary>
        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public void Decrypt_ShouldReturnEmpty_ForNullOrEmpty(string? input)
        {
            Assert.Equal(string.Empty, _service.Decrypt(input));
        }

        // ---------------- Round-trip ----------------

        /// <summary>
        /// Verifies that encrypting and then decrypting the same text
        /// returns the original string unchanged.
        /// </summary>
        [Fact]
        public void EncryptDecrypt_ShouldRoundtripSuccessfully()
        {
            const string original = "Some sensitive text 🔒";

            var cipher = _service.Encrypt(original);
            Assert.False(string.IsNullOrWhiteSpace(cipher)); // must produce something

            var decrypted = _service.Decrypt(cipher);
            Assert.Equal(original, decrypted);
        }

        // ---------------- Error cases for Decrypt ----------------

        /// <summary>
        /// Ensures that Decrypt throws a <see cref="FormatException"/>
        /// when the input string is not valid Base64.
        /// </summary>
        [Fact]
        public void Decrypt_WithInvalidBase64_ShouldThrowFormatException()
        {
            Assert.Throws<FormatException>(() => _service.Decrypt("not-base64"));
        }

        /// <summary>
        /// Ensures that Decrypt throws a <see cref="CryptographicException"/>
        /// when the ciphertext has been tampered with and is no longer valid.
        /// </summary>
        [Fact]
        public void Decrypt_WithTamperedCipher_ShouldThrowCryptographicException()
        {
            var cipher = _service.Encrypt("data");

            // Tamper with the last two chars of the Base64 string to corrupt it
            var tampered = cipher[..^2] + "AA";

            Assert.Throws<CryptographicException>(() => _service.Decrypt(tampered));
        }
    }
}
