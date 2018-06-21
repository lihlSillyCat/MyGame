using System.Text;
using System.Security.Cryptography;

namespace War.Common
{
    public class AlgorithmUtils
    {
        public static string HashUtf8MD5(string text)
        {
            return HashTextMD5(text, Encoding.UTF8);
        }

        public static string HashTextMD5(string text, Encoding encoder)
        {
            return HashMD5(encoder.GetBytes(text));
        }


        public static string HashMD5(byte[] bs)
        {
            MD5 md5 = MD5.Create();
            bs = md5.ComputeHash(bs, 0, bs.Length);
            StringBuilder sb = new StringBuilder();
            foreach (byte b in bs)
            {
                sb.Append(b.ToString("x2"));
            }

            return sb.ToString();
        }
    }
}