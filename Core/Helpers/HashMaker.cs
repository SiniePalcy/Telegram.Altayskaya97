using System.Text;
using System.Security.Cryptography;

namespace Telegram.Altayskaya97.Core.Helpers
{
    public static class HashMaker
    {
        public static byte[] ComputeHash(string value, Encoding encoding)
        {
            var encryptor = SHA256.Create();
            var hash = encryptor.ComputeHash(encoding.GetBytes(value));
            return hash;
        }
    }
}
