using System;
using System.Security.Cryptography;
using System.Text;

namespace CafeteriaUNAL.Utils
{
    public static class PasswordHelper
    {
        private const int SaltSize = 16;
        private const int HashSize = 32;
        private const int Iterations = 10000;

        public static string HashPassword(string password)
        {
            // Generar salt aleatorio
            byte[] salt = new byte[SaltSize];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(salt);
            }

            // Crear hash
            using (var pbkdf2 = new Rfc2898DeriveBytes(password, salt, Iterations, HashAlgorithmName.SHA256))
            {
                byte[] hash = pbkdf2.GetBytes(HashSize);

                // Combinar salt y hash
                byte[] hashBytes = new byte[SaltSize + HashSize];
                Array.Copy(salt, 0, hashBytes, 0, SaltSize);
                Array.Copy(hash, 0, hashBytes, SaltSize, HashSize);

                return Convert.ToBase64String(hashBytes);
            }
        }

        public static bool VerifyPassword(string password, string hashedPassword)
        {
            try
            {
                byte[] hashBytes = Convert.FromBase64String(hashedPassword);

                // Extraer salt
                byte[] salt = new byte[SaltSize];
                Array.Copy(hashBytes, 0, salt, 0, SaltSize);

                // Crear hash del password proporcionado
                using (var pbkdf2 = new Rfc2898DeriveBytes(password, salt, Iterations, HashAlgorithmName.SHA256))
                {
                    byte[] hash = pbkdf2.GetBytes(HashSize);

                    // Comparar hashes
                    for (int i = 0; i < HashSize; i++)
                    {
                        if (hashBytes[i + SaltSize] != hash[i])
                            return false;
                    }
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }

        public static string GenerarPasswordTemporal()
        {
            const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnpqrstuvwxyz23456789";
            var random = new Random();
            var result = new StringBuilder(8);

            for (int i = 0; i < 8; i++)
            {
                result.Append(chars[random.Next(chars.Length)]);
            }

            return result.ToString();
        }

        public static bool ValidarComplejidadPassword(string password)
        {
            if (string.IsNullOrEmpty(password) || password.Length < 6)
                return false;

            bool tieneMayuscula = false;
            bool tieneMinuscula = false;
            bool tieneNumero = false;

            foreach (char c in password)
            {
                if (char.IsUpper(c)) tieneMayuscula = true;
                if (char.IsLower(c)) tieneMinuscula = true;
                if (char.IsDigit(c)) tieneNumero = true;
            }

            return tieneMayuscula && tieneMinuscula && tieneNumero;
        }
    }
}