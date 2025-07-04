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
        /// Prepares a random 32-byte key (fixed seed) and configures the service.
        /// </summary>
        public EncryptionServiceTest()
        {
            // Prepare a random 32-byte key and encode it in Base64
            var key = new byte[32];
            new Random(123).NextBytes(key);
            var base64Key = Convert.ToBase64String(key);

            var settings = new Dictionary<string, string?>
            {
                ["Encryption:Key"] = base64Key
            };
            IConfiguration config = new ConfigurationBuilder()
                .AddInMemoryCollection(settings)
                .Build();

            _service = new EncryptionService(config);
        }

        /// <summary>
        /// Ensures that the constructor throws <see cref="InvalidOperationException"/>
        /// when the encryption key is missing from configuration.
        /// </summary>
        [Fact]
        public void Constructor_ShouldThrow_WhenKeyMissing()
        {
            IConfiguration config = new ConfigurationBuilder().Build();
            Assert.Throws<InvalidOperationException>(() => new EncryptionService(config));
        }

        /// <summary>
        /// Ensures that the constructor throws <see cref="InvalidOperationException"/>
        /// when the encryption key is not valid Base64.
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
        /// when the Base64 key decodes to a length other than 32 bytes.
        /// </summary>
        [Fact]
        public void Constructor_ShouldThrow_WhenKeyWrongLength()
        {
            // Base64 key that decodes to less than 32 bytes
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

        /// <summary>
        /// Verifies that Encrypt returns an empty string
        /// when input is null or empty.
        /// </summary>
        /// <param name="input">The plaintext input.</param>
        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public void Encrypt_ShouldReturnEmpty_ForNullOrEmpty(string? input)
        {
            Assert.Equal(string.Empty, _service.Encrypt(input));
        }

        /// <summary>
        /// Verifies that Decrypt returns an empty string
        /// when input is null or empty.
        /// </summary>
        /// <param name="input">The cipher text input.</param>
        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public void Decrypt_ShouldReturnEmpty_ForNullOrEmpty(string? input)
        {
            Assert.Equal(string.Empty, _service.Decrypt(input));
        }

        /// <summary>
        /// Verifies that a plaintext encrypted and then decrypted
        /// returns the original text (round-trip).
        /// </summary>
        [Fact]
        public void EncryptDecrypt_ShouldRoundtripSuccessfully()
        {
            const string original = "Some sensitive text 🔒";
            var cipher = _service.Encrypt(original);

            Assert.False(string.IsNullOrWhiteSpace(cipher));

            var decrypted = _service.Decrypt(cipher);
            Assert.Equal(original, decrypted);
        }

        /// <summary>
        /// Ensures that Decrypt throws a FormatException>
        /// when the input is not valid Base64.
        /// </summary>
        [Fact]
        public void Decrypt_WithInvalidBase64_ShouldThrowFormatException()
        {
            Assert.Throws<FormatException>(() => _service.Decrypt("not-base64"));
        }

        /// <summary>
        /// Ensures that Decrypt throws a CryptographicException>
        /// when the cipher text has been tampered with.
        /// </summary>
        [Fact]
        public void Decrypt_WithTamperedCipher_ShouldThrowCryptographicException()
        {
            var cipher = _service.Encrypt("data");
            // Modify the end to corrupt the ciphertext
            var tampered = cipher[..^2] + "AA";
            Assert.Throws<CryptographicException>(() => _service.Decrypt(tampered));
        }
    }
}