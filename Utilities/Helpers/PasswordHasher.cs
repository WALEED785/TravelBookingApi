// Utilities/PasswordHelper.cs
using System.Security.Cryptography;
using System.Text;

namespace TravelBookingApi.Utilities.Helpers
{ 
    public static class PasswordHelper
    {
        private const int SaltSize = 16; // 128 bits
        private const int KeySize = 32; // 256 bits
        private const int Iterations = 10000;
        private static readonly HashAlgorithmName Algorithm = HashAlgorithmName.SHA256;

        public static bool VerifyPassword(string password, string hashedPassword)
        {
            // Early return for invalid inputs
            if (string.IsNullOrWhiteSpace(password) ||
                string.IsNullOrWhiteSpace(hashedPassword) ||
                hashedPassword.Length < SaltSize + KeySize)
            {
                return false;
            }

            // Validate Base64 format first
            if (!IsValidBase64(hashedPassword))
            {
                return false;
            }

            try
            {
                var decoded = Convert.FromBase64String(hashedPassword);

                // Verify the decoded length matches expected size
                if (decoded.Length != SaltSize + KeySize)
                {
                    return false;
                }

                var salt = new byte[SaltSize];
                var hash = new byte[KeySize];

                Buffer.BlockCopy(decoded, 0, salt, 0, SaltSize);
                Buffer.BlockCopy(decoded, SaltSize, hash, 0, KeySize);

                var newHash = Rfc2898DeriveBytes.Pbkdf2(
                    Encoding.UTF8.GetBytes(password),
                    salt,
                    Iterations,
                    Algorithm,
                    KeySize);

                return CryptographicOperations.FixedTimeEquals(hash, newHash);
            }
            catch
            {
                // Catch any unexpected errors during verification
                return false;
            }
        }

        private static bool IsValidBase64(string str)
        {
            if (string.IsNullOrWhiteSpace(str))
                return false;

            try
            {
                // Check if the string length is a multiple of 4
                if (str.Length % 4 != 0)
                    return false;

                // Check for invalid characters
                foreach (var c in str)
                {
                    if (!char.IsLetterOrDigit(c) && c != '+' && c != '/' && c != '=')
                        return false;
                }

                // Try a test conversion
                var test = Convert.FromBase64String(str.Substring(0, Math.Min(4, str.Length)));
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static string HashPassword(string password)
        {
            if (string.IsNullOrWhiteSpace(password))
                throw new ArgumentException("Password cannot be empty");

            var salt = RandomNumberGenerator.GetBytes(SaltSize);
            var hash = Rfc2898DeriveBytes.Pbkdf2(
                Encoding.UTF8.GetBytes(password),
                salt,
                Iterations,
                Algorithm,
                KeySize);

            var result = new byte[SaltSize + KeySize];
            Buffer.BlockCopy(salt, 0, result, 0, SaltSize);
            Buffer.BlockCopy(hash, 0, result, SaltSize, KeySize);

            return Convert.ToBase64String(result);
        }
    }
}