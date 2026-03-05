using System;
using System.Security.Cryptography;

namespace ExclusionEngine.Web
{
    public static class Security
    {
        public static string HashPassword(string password)
        {
            if (password == null) throw new ArgumentNullException(nameof(password));

            var salt = new byte[16];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(salt);
            }

            using (var derive = new Rfc2898DeriveBytes(password, salt, 10000, HashAlgorithmName.SHA256))
            {
                var hash = derive.GetBytes(32);
                return $"{Convert.ToBase64String(salt)}:{Convert.ToBase64String(hash)}";
            }
        }

        public static bool VerifyPassword(string password, string storedHash)
        {
            if (string.IsNullOrWhiteSpace(storedHash))
            {
                return false;
            }

            var parts = storedHash.Split(':');
            if (parts.Length != 2)
            {
                // Backward compatibility for old plaintext seed values.
                return string.Equals(password, storedHash, StringComparison.Ordinal);
            }

            var salt = Convert.FromBase64String(parts[0]);
            var expected = Convert.FromBase64String(parts[1]);

            using (var derive = new Rfc2898DeriveBytes(password ?? string.Empty, salt, 10000, HashAlgorithmName.SHA256))
            {
                var actual = derive.GetBytes(32);
                return ConstantTimeEquals(expected, actual);
            }
        }

        private static bool ConstantTimeEquals(byte[] left, byte[] right)
        {
            if (left == null || right == null || left.Length != right.Length)
            {
                return false;
            }

            var diff = 0;
            for (var i = 0; i < left.Length; i++)
            {
                diff |= left[i] ^ right[i];
            }

            return diff == 0;
        }
    }
}
