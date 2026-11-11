using System;
using System.Security.Cryptography;

namespace SistemaLogin.Security
{
    /// <summary>
    /// Genera y verifica hashes de contraseñas usando PBKDF2 (Rfc2898DeriveBytes).
    /// Cada contraseña tiene su propio salt aleatorio.
    /// </summary>
    public static class PasswordHasher
    {
        private const int SaltSize = 16;
        private const int HashSize = 32;
        private const int Iterations = 10000;

        public static string CreateSalt()
        {
            byte[] salt = new byte[SaltSize];
            using (RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider())
            {
                rng.GetBytes(salt);
            }
            return Convert.ToBase64String(salt);
        }

        public static string Hash(string value, string saltBase64)
        {
            if (value == null) throw new ArgumentNullException("value");
            if (string.IsNullOrEmpty(saltBase64)) throw new ArgumentException("El salt es obligatorio.", "saltBase64");

            byte[] salt = Convert.FromBase64String(saltBase64);
            using (Rfc2898DeriveBytes derive = new Rfc2898DeriveBytes(value, salt, Iterations))
            {
                return Convert.ToBase64String(derive.GetBytes(HashSize));
            }
        }

        public static bool Verify(string value, string saltBase64, string expectedHashBase64)
        {
            if (string.IsNullOrEmpty(saltBase64) || string.IsNullOrEmpty(expectedHashBase64))
                return false;

            try
            {
                string actual = Hash(value, saltBase64);
                byte[] actualBytes = Convert.FromBase64String(actual);
                byte[] expectedBytes = Convert.FromBase64String(expectedHashBase64);
                return FixedTimeEquals(actualBytes, expectedBytes);
            }
            catch
            {
                return false;
            }
        }

        private static bool FixedTimeEquals(byte[] a, byte[] b)
        {
            if (a == null || b == null || a.Length != b.Length)
                return false;

            int diff = 0;
            for (int i = 0; i < a.Length; i++)
            {
                diff |= a[i] ^ b[i];
            }
            return diff == 0;
        }
    }
}
