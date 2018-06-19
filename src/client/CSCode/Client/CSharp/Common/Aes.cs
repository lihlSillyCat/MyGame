using System.Text;
using System.Security.Cryptography;

namespace War.Common
{
    public class Aes
    {
        private static readonly RijndaelManaged ms_RijndaelManager = new RijndaelManaged
        {
            Mode = CipherMode.CBC, Padding = PaddingMode.PKCS7
        };

        public static byte[] Encryptor(byte[] bs, string key, string iv)
        {
            ms_RijndaelManager.Key = Encoding.UTF8.GetBytes(key);
            ms_RijndaelManager.IV = Encoding.UTF8.GetBytes(iv);
            ICryptoTransform transform = ms_RijndaelManager.CreateEncryptor();
            return transform.TransformFinalBlock(bs, 0, bs.Length);
        }

        public static byte[] Decryptor(byte[] bs, string key, string iv)
        {
            ms_RijndaelManager.Key = Encoding.UTF8.GetBytes(key);
            ms_RijndaelManager.IV = Encoding.UTF8.GetBytes(iv);
            ICryptoTransform transform = ms_RijndaelManager.CreateDecryptor();
            return transform.TransformFinalBlock(bs, 0, bs.Length);
        }
    }
}
