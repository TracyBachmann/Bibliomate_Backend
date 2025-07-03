using System.Security.Cryptography;
using System.Text;

namespace BackendBiblioMate.Services.Infrastructure.Security
{
    /// <summary>
    /// Provides AES-256-CBC encryption and decryption for sensitive strings.
    /// </summary>
    public class EncryptionService
    {
        private readonly byte[] _key;

        /// <summary>
        /// Initializes a new instance of the <see cref="EncryptionService"/> class.
        /// </summary>
        /// <param name="config">
        /// Application configuration; must contain a Base64-encoded 32-byte key under "Encryption:Key".
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="config"/> is null.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// Thrown if the key is missing, not valid Base64, or not exactly 32 bytes when decoded.
        /// </exception>
        public EncryptionService(IConfiguration config)
        {
            if (config == null) 
                throw new ArgumentNullException(nameof(config));

            var base64Key = config["Encryption:Key"]
                ?? throw new InvalidOperationException("Encryption key not configured in 'Encryption:Key'.");

            try
            {
                _key = Convert.FromBase64String(base64Key);
            }
            catch (FormatException ex)
            {
                throw new InvalidOperationException("Encryption key is not a valid Base64 string.", ex);
            }

            if (_key.Length != 32)
                throw new InvalidOperationException("Encryption key must be 32 bytes (256 bits) for AES-256.");
        }

        /// <summary>
        /// Encrypts the specified plaintext using AES-256-CBC.
        /// </summary>
        /// <param name="plain">
        /// The plaintext to encrypt. If null or empty, returns an empty string.
        /// </param>
        /// <returns>
        /// A Base64-encoded string containing the IV concatenated with the ciphertext.
        /// </returns>
        public string Encrypt(string? plain)
        {
            if (string.IsNullOrEmpty(plain))
                return string.Empty;

            using var aes = CreateAes();
            aes.GenerateIV();

            using var encryptor = aes.CreateEncryptor();
            var plainBytes  = Encoding.UTF8.GetBytes(plain);
            var cipherBytes = encryptor.TransformFinalBlock(plainBytes, 0, plainBytes.Length);

            // Prepend IV
            var combined = new byte[aes.IV.Length + cipherBytes.Length];
            Buffer.BlockCopy(aes.IV,       0, combined, 0,           aes.IV.Length);
            Buffer.BlockCopy(cipherBytes,  0, combined, aes.IV.Length, cipherBytes.Length);

            return Convert.ToBase64String(combined);
        }

        /// <summary>
        /// Decrypts the specified Base64-encoded IV+ciphertext string using AES-256-CBC.
        /// </summary>
        /// <param name="cipher">
        /// The Base64-encoded payload (IV + ciphertext). If null or empty, returns an empty string.
        /// </param>
        /// <returns>The decrypted plaintext string.</returns>
        /// <exception cref="FormatException">
        /// Thrown if <paramref name="cipher"/> is not valid Base64.
        /// </exception>
        /// <exception cref="CryptographicException">
        /// Thrown if decryption fails (e.g. tampered data).
        /// </exception>
        public string Decrypt(string? cipher)
        {
            if (string.IsNullOrEmpty(cipher))
                return string.Empty;

            var fullCipher = Convert.FromBase64String(cipher);

            using var aes = CreateAes();
            int ivLen = aes.BlockSize / 8;

            var iv          = new byte[ivLen];
            var cipherBytes = new byte[fullCipher.Length - ivLen];

            Buffer.BlockCopy(fullCipher, 0, iv,          0, ivLen);
            Buffer.BlockCopy(fullCipher, ivLen, cipherBytes, 0, cipherBytes.Length);

            aes.IV = iv;
            using var decryptor = aes.CreateDecryptor();
            var plainBytes      = decryptor.TransformFinalBlock(cipherBytes, 0, cipherBytes.Length);

            return Encoding.UTF8.GetString(plainBytes);
        }

        /// <summary>
        /// Creates a pre-configured  Aes instance for AES-256-CBC.
        /// </summary>
        /// <returns>
        /// An Aes object with Aes.Key, Aes.Mode, and Aes.Padding set.
        /// </returns>
        /// CryptographicException>
        /// Thrown if the AES algorithm cannot be instantiated.
        private Aes CreateAes()
        {
            var aes = Aes.Create()
                  ?? throw new CryptographicException("Unable to create AES algorithm instance.");

            aes.Key     = _key;
            aes.Mode    = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;
            return aes;
        }
    }
}