using System;
using System.Diagnostics.CodeAnalysis;
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
        /// Attempts to protect a plain-text configuration value without surfacing expected crypto failures as exceptions.
        /// </summary>
        /// <param name="plainText">The value to protect.</param>
        /// <param name="protectedText">The protected value encoded as Base64 text, or <c>null</c> when protection fails.</param>
        /// <param name="entropy">Optional additional entropy used by DPAPI.</param>
        /// <returns><c>true</c> when protection succeeds; otherwise, <c>false</c>.</returns>
        public static bool TryProtect(string? plainText, [NotNullWhen(true)] out string? protectedText, string? entropy = null)
        {
            protectedText = null;

            if (plainText is null)
            {
                return false;
            }

            try
            {
                protectedText = Protect(plainText, entropy);
                return true;
            }
            catch (CryptographicException)
            {
                return false;
            }
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

        /// <summary>
        /// Attempts to unprotect a Base64-encoded configuration value without surfacing expected parsing or crypto failures as exceptions.
        /// </summary>
        /// <param name="protectedText">The Base64-encoded protected value.</param>
        /// <param name="plainText">The unprotected plain-text value, or <c>null</c> when unprotection fails.</param>
        /// <param name="entropy">Optional additional entropy used by DPAPI.</param>
        /// <returns><c>true</c> when unprotection succeeds; otherwise, <c>false</c>.</returns>
        public static bool TryUnprotect(string? protectedText, [NotNullWhen(true)] out string? plainText, string? entropy = null)
        {
            plainText = null;

            if (protectedText is null)
            {
                return false;
            }

            try
            {
                plainText = Unprotect(protectedText, entropy);
                return true;
            }
            catch (FormatException)
            {
                return false;
            }
            catch (CryptographicException)
            {
                return false;
            }
        }

        private static byte[]? GetEntropyBytes(string? entropy)
        {
            if (entropy is null)
            {
                return null;
            }

            return entropy.Length == 0 ? Array.Empty<byte>() : Encoding.UTF8.GetBytes(entropy);
        }
    }
}
