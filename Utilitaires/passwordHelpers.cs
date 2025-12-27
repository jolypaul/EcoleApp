using System.Security.Cryptography;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;

namespace EcoleApp.Utilitaires
{
    public static class PasswordHelper
    {
        public static string HashPassword(string password)
        {
            byte[] salt = RandomNumberGenerator.GetBytes(128 / 8);

            string hashed = Convert.ToBase64String(
                KeyDerivation.Pbkdf2(
                    password,
                    salt,
                    KeyDerivationPrf.HMACSHA256,
                    iterationCount: 100000,
                    numBytesRequested: 256 / 8));

            return $"{Convert.ToBase64String(salt)}.{hashed}";
        }

        
        public static bool VerifyPassword(string password, string hash)
        {
            var parts = hash.Split('.');
            if (parts.Length != 2)
                return false;

            var salt = Convert.FromBase64String(parts[0]);
            var storedHash = parts[1];

            var computedHash = Convert.ToBase64String(
                KeyDerivation.Pbkdf2(
                    password,
                    salt,
                    KeyDerivationPrf.HMACSHA256,
                    iterationCount: 100000,
                    numBytesRequested: 256 / 8));

            return computedHash == storedHash;
        }
    }
}
