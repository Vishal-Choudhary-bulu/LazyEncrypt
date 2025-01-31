using System.Security.Cryptography;
using System.Text;
using UnityEngine;

namespace LazyEncrypt.Core
{
    public static class SHA256Hasher
    {
        /// <summary>
        /// Hashes the given input string using SHA-256 with the provided secret.
        /// </summary>
        /// <param name="input">The input string to hash.</param>
        /// <param name="secret">The secret key used to enhance security.</param>
        /// <returns>SHA-256 hash as a hexadecimal string.</returns>
        public static string Hash(string input, string secret)
        {
            if (string.IsNullOrEmpty(secret))
            {
                Debug.LogError("Secret key is missing!");
                return null;
            }

            var combined = input + secret;
            using var sha256 = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(combined);
            var hashBytes = sha256.ComputeHash(bytes);

            // Convert hash to hexadecimal string
            var sb = new StringBuilder();
            foreach (var b in hashBytes)
            {
                sb.Append(b.ToString("x2"));
            }
            return sb.ToString();
        }
    }
}