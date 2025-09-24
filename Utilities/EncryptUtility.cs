using System;
using System.Security.Cryptography;
using BCrypt.Net;

namespace triage_backend.Utilities
{
    /// <summary>
    /// Utility class for hashing, verifying, and generating secure passwords using BCrypt.
    /// </summary>
    public static class EncryptUtility
    {
        /// <summary>
        /// Hashes a plain text password and returns the secure hash.
        /// </summary>
        /// <param name="plainPassword">The plain text password.</param>
        /// <param name="workFactor">The cost factor (default 8).</param>
        /// <returns>Hashed password ready to store in the database.</returns>
        public static string HashPassword(string plainPassword, int workFactor = 8)
        {
            if (string.IsNullOrWhiteSpace(plainPassword))
                throw new ArgumentException("La contraseña no puede estar vacía.", nameof(plainPassword));

            return BCrypt.Net.BCrypt.HashPassword(plainPassword, BCrypt.Net.BCrypt.GenerateSalt(workFactor));
        }

        /// <summary>
        /// Verifies that a plain text password matches the stored hash.
        /// </summary>
        /// <param name="plainPassword">The plain text password provided by the user.</param>
        /// <param name="storedHash">The stored password hash from the database.</param>
        /// <returns>True if the passwords match; false otherwise.</returns>
        public static bool VerifyPassword(string plainPassword, string storedHash)
        {
            if (string.IsNullOrWhiteSpace(plainPassword))
                throw new ArgumentException("La contraseña no puede estar vacía.", nameof(plainPassword));

            if (string.IsNullOrWhiteSpace(storedHash))
                throw new ArgumentException("El hash almacenado no puede estar vacío.", nameof(storedHash));

            try
            {
                return BCrypt.Net.BCrypt.Verify(plainPassword, storedHash);
            }
            catch
            {

                // Si el hash almacenado no es válido o está dañado, devuelve falso
                return false;
            }
        }
    }
}
