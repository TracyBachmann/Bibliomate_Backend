using System.Security.Cryptography;
using System.Text;

namespace backend.Services
{
    /// <summary>
    /// Provides AES-256-CBC encryption and decryption for sensitive strings.
    /// </summary>
    public class EncryptionService
    {
        private readonly byte[] _key;

        /// <summary>
        /// Initializes the service with a 32-byte key from configuration (base64 encoded).
        /// </summary>
        /// <param name="config">Application configuration (reads "Encryption:Key").</param>
        public EncryptionService(IConfiguration config)
        {
            var base64Key = config["Encryption:Key"]
                ?? throw new InvalidOperationException("Encryption key not configured.");
            _key = Convert.FromBase64String(base64Key);
            if (_key.Length != 32)
                throw new InvalidOperationException("Encryption key must be 32 bytes for AES-256.");
        }

        /// <summary>
        /// Encrypts the given plaintext using AES-256-CBC and returns a base64 string containing IV + ciphertext.
        /// </summary>
        /// <param name="plain">The plaintext to encrypt (may be null or empty).</param>
        /// <returns>Base64-encoded IV + ciphertext, or empty string if input is null/empty.</returns>
        public string Encrypt(string? plain)
        {
            if (string.IsNullOrEmpty(plain))
                return string.Empty;

            using var aes = Aes.Create();
            aes.Key     = _key;
            aes.Mode    = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;
            aes.GenerateIV();

            using var encryptor   = aes.CreateEncryptor();
            var plainBytes        = Encoding.UTF8.GetBytes(plain);
            var cipherBytes       = encryptor.TransformFinalBlock(plainBytes, 0, plainBytes.Length);

            // Prepend IV to ciphertext
            var result = new byte[aes.IV.Length + cipherBytes.Length];
            Buffer.BlockCopy(aes.IV,       0, result, 0,           aes.IV.Length);
            Buffer.BlockCopy(cipherBytes,  0, result, aes.IV.Length, cipherBytes.Length);

            return Convert.ToBase64String(result);
        }

        /// <summary>
        /// Decrypts a base64 string containing IV + ciphertext using AES-256-CBC.
        /// </summary>
        /// <param name="cipher">Base64-encoded IV + ciphertext (may be null or empty).</param>
        /// <returns>The decrypted plaintext, or empty string if input is null/empty.</returns>
        public string Decrypt(string? cipher)
        {
            if (string.IsNullOrEmpty(cipher))
                return string.Empty;

            var fullCipher = Convert.FromBase64String(cipher);

            using var aes = Aes.Create();
            aes.Key     = _key;
            aes.Mode    = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;

            // Extract IV
            var iv          = new byte[aes.BlockSize / 8];
            var cipherBytes = new byte[fullCipher.Length - iv.Length];
            Buffer.BlockCopy(fullCipher, 0, iv,          0, iv.Length);
            Buffer.BlockCopy(fullCipher, iv.Length, cipherBytes, 0, cipherBytes.Length);

            aes.IV = iv;

            using var decryptor = aes.CreateDecryptor();
            var plainBytes      = decryptor.TransformFinalBlock(cipherBytes, 0, cipherBytes.Length);

            return Encoding.UTF8.GetString(plainBytes);
        }
    }
}