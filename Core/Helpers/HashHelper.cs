using System.Text;
using System.Linq;
using System.Security.Cryptography;

namespace Telegram.Altayskaya97.Core.Helpers
{
    public static class HashHelper
    {
        public static byte[] ComputeHash(string value, Encoding encoding)
        {
            var encryptor = SHA256.Create();
            var hash = encryptor.ComputeHash(encoding.GetBytes(value));
            return hash;
        }

        public static string GetString(byte[] array)
        {
            char[] charArray = array.Select(b => (char)b).ToArray();
            return new string(charArray);
        }

        public static byte[] GetBytes(string source)
        {
            var charArray = source.ToCharArray();
            return charArray.Select(c => (byte)c).ToArray();
        }
    }
}
