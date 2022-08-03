using System.Security.Cryptography;
using System.IO;
using System;
using Microsoft.AspNetCore.Hosting;
using System.Text;
using Raybod.SCM.Services.Core;
using Raybod.SCM.DataTransferObject.License;

namespace Raybod.SCM.Services.Implementation
{
    public class Security : ISecurity
    {
        private byte[] Key = Convert.FromBase64String("UmF5Ym9kUmF2ZXNoIUAjJA==");
        private byte[] IV = Convert.FromBase64String("UmF5Ym9kUmF2ZXNoIUAjJA==");
        private const int Keysize = 128;
        private const int DerivationIterations = 1000;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private static UnicodeEncoding _encoder = new UnicodeEncoding();
        public Security(IWebHostEnvironment webHostEnvironment)
        {
            _webHostEnvironment = webHostEnvironment;
        }
        public byte[] EncodingFile(LicenceEntities licence)
        {


            
            
            licence.Licence = EncryptLicene(licence.Username,licence.IpAddress+":"+licence.Port);
           
            string serializeProfile = Newtonsoft.Json.JsonConvert.SerializeObject(licence);
            var resultEncrypt = EncryptStringToBytes(serializeProfile);
            return resultEncrypt;
        }
        public LicenceEntities DecryptFile()
        {
            
            try
            {
                string fileName = Path.Combine(_webHostEnvironment.ContentRootPath, "Files", "SecurityFile.dll");
                using (FileStream fsRead = new FileStream(fileName, FileMode.Open, FileAccess.Read))
                {
                    
                    BinaryReader br = new BinaryReader(fsRead);
                    long numBytes = new FileInfo(fileName).Length;
                    string decryptedText = DecryptStringFromBytes(br.ReadBytes((int)numBytes));
                    LicenceEntities DeserializeProfile = Newtonsoft.Json.JsonConvert.DeserializeObject<LicenceEntities>(decryptedText);
                    fsRead.Close();
                    DeserializeProfile.Licence = DecryptLicence(DeserializeProfile.Licence, DeserializeProfile.IpAddress + ":" + DeserializeProfile.Port);
                    return DeserializeProfile;
                }
                
            }
            catch (Exception ex)

            {
                
                return null;

            }
        }

        private byte[] EncryptStringToBytes(string profileText)
        {
            byte[] encryptedAuditTrail;

            using (Aes newAes = Aes.Create())
            {
                newAes.Key = Key;
                newAes.IV = IV;

                ICryptoTransform encryptor = newAes.CreateEncryptor(Key, IV);

                using (MemoryStream msEncrypt = new MemoryStream())
                {
                    using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                        {
                            swEncrypt.Write(profileText);
                        }
                        encryptedAuditTrail = msEncrypt.ToArray();
                    }
                }
            }

            return encryptedAuditTrail;
        }
        private string DecryptStringFromBytes(byte[] profileText)
        {
            string decryptText;

            using (Aes newAes = Aes.Create())
            {
                newAes.Key = Key;
                newAes.IV = IV;

                ICryptoTransform decryptor = newAes.CreateDecryptor(Key, IV);

                using (MemoryStream msDecrypt = new MemoryStream(profileText))
                {
                    using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                    {
                        using (StreamReader srDecrypt = new StreamReader(csDecrypt))
                        {
                            decryptText = srDecrypt.ReadToEnd();
                        }
                    }
                }
            }


            return decryptText;
        }

        public string DecryptLicence(string licence, string key)
        {
            try
            {


                // Salt and IV is randomly generated each time, but is preprended to encrypted cipher text
                // so that the same Salt and IV values can be used when decrypting.  
                string alignKey = "";
                while (alignKey.Length < 16)
                {
                    alignKey += key.Replace('.', '0');
                }
                string decryptText;

                using (Aes newAes = Aes.Create())
                {
                    newAes.Key = Encoding.ASCII.GetBytes(alignKey.Substring(0, 16));
                    newAes.IV = Encoding.ASCII.GetBytes(alignKey.Substring(0, 16));

                    ICryptoTransform decryptor = newAes.CreateDecryptor();

                    using (MemoryStream msDecrypt = new MemoryStream(Convert.FromBase64String(licence)))
                    {
                        using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                        {
                            using (StreamReader srDecrypt = new StreamReader(csDecrypt))
                            {
                                decryptText = srDecrypt.ReadToEnd();
                            }
                        }
                    }
                }


                return decryptText;
            }
            catch(Exception ex)
            {
                return null;
            }
        }

        public string EncryptLicene(string userId,string key)
        {
            string alignKey = "";
            while (alignKey.Length < 16)
            {
                alignKey += key.Replace('.', '0');
            }
           
            byte[] encryptedAuditTrail;

            using (Aes newAes = Aes.Create())
            {
                newAes.Key = Encoding.ASCII.GetBytes(alignKey.Substring(0,16));
                newAes.IV = Encoding.ASCII.GetBytes(alignKey.Substring(0, 16));

                ICryptoTransform encryptor = newAes.CreateEncryptor();

                using (MemoryStream msEncrypt = new MemoryStream())
                {
                    using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                        {
                            swEncrypt.Write(userId);
                        }
                        encryptedAuditTrail = msEncrypt.ToArray();
                    }
                }
            }

            return Convert.ToBase64String(encryptedAuditTrail);
        }
}


}