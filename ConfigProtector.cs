using System;
using System.Security.Cryptography;
using System.Text;

namespace ATC4_HQ
{
    /// <summary>
    /// Provides DPAPI-based protection for configuration values.
    /// </summary>
    public static class ConfigProtector
    {
        /// <summary>
        /// Protects a plain-text configuration value and returns it as Base64 text.
        /// </summary>
        /// <param name="plainText">The value to protect.</param>
        /// <param name="entropy">Optional additional entropy used by DPAPI.</param>
        /// <returns>The protected value encoded as Base64 text.</returns>
        public static string Protect(string plainText, string? entropy = null)
        {
            ArgumentNullException.ThrowIfNull(plainText);

            byte[] encryptedBytes = ProtectedData.Protect(
                Encoding.UTF8.GetBytes(plainText),
                GetEntropyBytes(entropy),
                DataProtectionScope.CurrentUser);

            return Convert.ToBase64String(encryptedBytes);
        }

        /// <summary>
        /// Unprotects a Base64-encoded configuration value and returns the plain text.
        /// </summary>
        /// <param name="protectedText">The Base64-encoded protected value.</param>
        /// <param name="entropy">Optional additional entropy used by DPAPI.</param>
        /// <returns>The unprotected plain-text value.</returns>
        public static string Unprotect(string protectedText, string? entropy = null)
        {
            ArgumentNullException.ThrowIfNull(protectedText);

            byte[] decryptedBytes = ProtectedData.Unprotect(
                Convert.FromBase64String(protectedText),
                GetEntropyBytes(entropy),
                DataProtectionScope.CurrentUser);

            return Encoding.UTF8.GetString(decryptedBytes);
        }

        private static byte[]? GetEntropyBytes(string? entropy)
        {
            return string.IsNullOrEmpty(entropy) ? null : Encoding.UTF8.GetBytes(entropy);
        }
    }
}
