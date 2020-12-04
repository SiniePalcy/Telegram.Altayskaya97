using System.Text;
using System.Security.Cryptography;

namespace Telegram.Altayskaya97.Core.Helpers
{
    public static class HashMaker
    {
        public static byte[] GetHash(string value)
        {
            var encryptor = SHA256.Create();
            var hash = encryptor.ComputeHash(Encoding.UTF8.GetBytes(value));
            return hash;
        }
    }
}
