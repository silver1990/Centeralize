using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Raybod.SCM.Utility.Security
{
    public class AesEncription
    {
        public static string AesEncrypt(string input, string key1, string key2)
        {
            var aes = new RijndaelManaged
            {
                KeySize = 256,
                BlockSize = 256,
                Padding = PaddingMode.PKCS7,
                Key = Convert.FromBase64String(key1),
                IV = Convert.FromBase64String(key2)
            };

            var encrypt = aes.CreateEncryptor(aes.Key, aes.IV);
            byte[] xBuff;
            using (var ms = new MemoryStream())
            {
                using (var cs = new CryptoStream(ms, encrypt, CryptoStreamMode.Write))
                {
                    var xXml = Encoding.UTF8.GetBytes(input);
                    cs.Write(xXml, 0, xXml.Length);
                }

                xBuff = ms.ToArray();
            }

            var output = Convert.ToBase64String(xBuff);
            return output;
        }

        public static string AesDecrypt(string input, string key1, string key2)
        {
            var aes = new RijndaelManaged
            {
                KeySize = 256,
                BlockSize = 256,
                Mode = CipherMode.CBC,
                Padding = PaddingMode.PKCS7,
                Key = Convert.FromBase64String(key1),
                IV = Convert.FromBase64String(key2)
            };

            var decrypt = aes.CreateDecryptor();
            byte[] xBuff = null;
            using (var ms = new MemoryStream())
            {
                using (var cs = new CryptoStream(ms, decrypt, CryptoStreamMode.Write))
                {
                    var xXml = Convert.FromBase64String(input);
                    cs.Write(xXml, 0, xXml.Length);
                }

                xBuff = ms.ToArray();
            }

            var output = Encoding.UTF8.GetString(xBuff);
            return output;
        }

    }

}