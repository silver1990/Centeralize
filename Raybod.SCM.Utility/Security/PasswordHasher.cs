using System.Security.Cryptography;
using System.Text;

namespace Raybod.SCM.Utility.Security
{
    public class PasswordHasher
    {
        private static byte[] GetMd5Hash(string inputString)
        {
            HashAlgorithm algorithm = MD5.Create();  //or use SHA1.Create();
            return algorithm.ComputeHash(Encoding.UTF8.GetBytes(inputString));
        }

        private static string GetMd5HashString(string inputString)
        {
            StringBuilder sb = new StringBuilder();
            foreach (byte b in GetMd5Hash(inputString))
                sb.Append(b.ToString("X2"));

            return sb.ToString();
        }

        private static byte[] GetSha256Hash(string inputString)
        {
            HashAlgorithm algorithm = SHA256.Create();  //or use SHA256.Create();
            return algorithm.ComputeHash(Encoding.UTF8.GetBytes(inputString));
        }

        private static string GetSha256HashString(string inputString)
        {
            StringBuilder sb = new StringBuilder();
            foreach (byte b in GetSha256Hash(inputString))
                sb.Append(b.ToString("X2"));

            return sb.ToString();
        }

        public string HashPassword(string password)
        {
            return GetSha256HashString(password);
        }
    }
}