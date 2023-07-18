using System.Collections;
using System.Security.Cryptography;

namespace GrpcServiceDemo.Utils
{
    public class PasswordService
    {
        public static string Hash(string password)
        {
            using var sha256 = SHA256.Create();
            byte[] bytes = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));

            return Convert.ToBase64String(bytes);
        }

        public static bool Verify(string password, string hashedPassword)
        {
            byte[] passwordBytes = Convert.FromBase64String(hashedPassword);

            using var sha256 = SHA256.Create();
            byte[] hashBytes = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));

            return StructuralComparisons.StructuralEqualityComparer.Equals(passwordBytes, hashBytes);
        }
    }
}
