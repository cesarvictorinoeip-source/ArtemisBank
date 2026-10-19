using System.Security.Cryptography;
using System.Text;

namespace ArtemisBank.Core.Application.Helpers
{
    public static class PasswordEncryptation
    {
        public static string ComputeSha256Hash(string password)
        {
            using SHA256 sha256Hash = SHA256.Create();
            byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(password));
            StringBuilder sb = new();
            foreach (var item in bytes)
            {
                sb.Append(item.ToString("x2"));
            }
            return sb.ToString();
        }
    }
}