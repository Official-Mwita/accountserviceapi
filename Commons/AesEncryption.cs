using System.Security.Cryptography;
using System.Text;

namespace accountservice.Commons
{
    public class AesEncryption
    {
        private byte[] key;
        private byte[] iV;

        private readonly string DEFAULT_KEY = "d43@18djfjscnvjd"; //Strong 16 character word
        private readonly string DEFAULT_IV = "tiudjshdurovj98="; //Random 16 character word

        private readonly IConfiguration? _config;

        //Using configuration file to get key and IV
        public AesEncryption(IConfiguration iconfig)
        {
            _config = iconfig;
            key = Encoding.UTF8.GetBytes(_config["EncryptionKey"] ?? DEFAULT_KEY);
            iV = Encoding.UTF8.GetBytes(_config["EncryptionIv"] ?? DEFAULT_IV);
        }


        public AesEncryption(string stringKey, string stringIv)
        {
            key = Encoding.UTF8.GetBytes(stringKey);
            iV = Encoding.UTF8.GetBytes(stringIv);
        }

        //Encryption and decryption methods

        /// <summary>
        /// Takes a plain text string. Encrypt it and return a 64 url encoding of the encrypted string
        /// </summary>
        /// <param name="payload"></param>
        /// <returns></returns>
        /// <exception cref="NotSupportedException"></exception>
        public string Encrypt(string payload)
        {
            if (string.IsNullOrEmpty(payload))
            {
                throw new NotSupportedException("Not support exception");
            }

            //continue with encryption
            byte[] encrypted;

            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = key;
                aesAlg.IV = iV;

                // Create an encryptor to perform the stream transform.
                ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

                // Create the streams used for encryption.
                using (MemoryStream msEncrypt = new MemoryStream())
                {
                    using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                        {
                            //Write all data to the stream.
                            swEncrypt.Write(payload);
                        }
                        encrypted = msEncrypt.ToArray();
                    }
                }
            }

            //Convert encrypted to base 64 string then url encode it
            return Encode64Url(encrypted);
        }

        /// <summary>
        /// Get an encrypted string, then return its plain based on the initialization parameters used to create this object
        /// parameters must match parameters used for encryption
        /// </summary>
        /// <param name="payload">An encrypted 64 base url string</param>
        /// <returns>Returns a plain of the encrypted payload</returns>
        /// <exception cref="NotSupportedException"></exception>
        public string Decrypt(string payload)
        {
            // Check payload availability.
            if (string.IsNullOrEmpty(payload))
                throw new NotSupportedException();

            // Create an Aes object
            // with the specified key and IV.
            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = key;
                aesAlg.IV = iV;

                // Create a decryptor to perform the stream transform.
                ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

                //Transform payload to bytes
                byte[] cipherText = Decode64Url(payload);
                // Create the streams used for decryption.
                using (MemoryStream msDecrypt = new MemoryStream(cipherText))
                {
                    using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                    {
                        using (StreamReader srDecrypt = new StreamReader(csDecrypt))
                        {

                            // Read the decrypted bytes from the decrypting stream
                            // and place them in a string.
                            payload = srDecrypt.ReadToEnd();
                        }
                    }
                }
            }

            return payload;
        }



        /// <summary>
        /// Get a string as input, then converts it to base 64 url encoded
        /// </summary>
        /// <param name="input"></param>
        /// <returns>A 64 url base encoded string</returns>
        private string Encode64Url(byte[] bytes)
        {
            string base64 = Convert.ToBase64String(bytes);
            string base64Url = base64.Replace("+", "-").Replace("/", "_").Replace("=", "");
            return base64Url;
        }


        /// <summary>
        /// Get a string as input, then converts it to base 64 url encoded
        /// </summary>
        /// <param name="input"></param>
        /// <returns>A 64 url base decoded string</returns>
        private byte[] Decode64Url(string input)
        {
            string base64Url = input.Replace("-", "+").Replace("_", "/");
            switch (base64Url.Length % 4)
            {
                case 2: base64Url += "=="; break;
                case 3: base64Url += "="; break;
            }
            return Convert.FromBase64String(base64Url);
        }

    }
}
