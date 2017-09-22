using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace WebKey.Support
{
    public interface IEncryptDecrypt
    {
        string Encrypt(string plainText);
        string Encrypt(string plainText, string sharedSecret);
        string Decrypt(string cipherText);
        string Decrypt(string cipherText, string sharedSecret);
        void SetKey(string key);
        string GetKey();
        string ComputeHash(string plainText);
    }

    public class EncryptDecrypt : IEncryptDecrypt
    {
        private byte[] _salt;
        private int _iterationCount = 4000;
        private string _sharedSecret = null;
        private string _loginSalt = null;

        public EncryptDecrypt()
        {
            this._salt = SHA256Managed.Create().ComputeHash(Encoding.UTF8.GetBytes("SDSD£$£ Change this.."));
            _loginSalt = "FSDGS£$£ Change this..";
        }

        public void SetKey(string key)
        {
            _sharedSecret = key;
        }

        public string GetKey()
        {
            return _sharedSecret;
        }

        public string ComputeHash(string plainText)
        {
            if (string.IsNullOrEmpty(plainText))
                throw new ArgumentNullException("plainText");

            return Encrypt(plainText, _loginSalt);
        }

        public string Encrypt(string plainText)
        {
            return Encrypt(plainText, _sharedSecret);
        }

        public string Decrypt(string plainText)
        {
            return Decrypt(plainText, _sharedSecret);
        }

        public string Encrypt(string plainText, string sharedSecret)
        {
            if (string.IsNullOrEmpty(plainText))
                throw new ArgumentNullException("Invalid data provided");
            if (sharedSecret == null)
                throw new ArgumentNullException("Please set password");

            string outStr = null;
            AesManaged aesAlg = new AesManaged();

            try
            {
                var key = new Rfc2898DeriveBytes(sharedSecret, _salt, _iterationCount);

                aesAlg.Key = key.GetBytes(aesAlg.KeySize / 8);
                aesAlg.IV = key.GetBytes(aesAlg.BlockSize / 8);

                ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

                using (var msEncrypt = new MemoryStream())
                {
                    using (var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    {
                        using (var swEncrypt = new StreamWriter(csEncrypt))
                        {
                            swEncrypt.Write(plainText);
                        }
                    }
                    outStr = Convert.ToBase64String(msEncrypt.ToArray());
                }
            }
            finally
            {
                if (aesAlg != null)
                    aesAlg.Clear();
            }
            return outStr;
        }

        public string Decrypt(string cipherText, string sharedSecret)
        {
            if (string.IsNullOrEmpty(cipherText))
                throw new ArgumentNullException("Invalid data provided");
            if (sharedSecret == null)
                throw new ArgumentNullException("Please set password");

            string plaintext = null;
            AesManaged aesAlg = new AesManaged();

            try
            {
                var key = new Rfc2898DeriveBytes(sharedSecret, _salt, _iterationCount);

                aesAlg.Key = key.GetBytes(aesAlg.KeySize / 8);
                aesAlg.IV = key.GetBytes(aesAlg.BlockSize / 8);

                ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

                byte[] bytes = Convert.FromBase64String(cipherText);
                using (var msDecrypt = new MemoryStream(bytes))
                {
                    using (var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                    {
                        using (var srDecrypt = new StreamReader(csDecrypt))
                            plaintext = srDecrypt.ReadToEnd();
                    }
                }
            }
            finally
            {
                if (aesAlg != null)
                    aesAlg.Clear();
            }

            return plaintext;
        }
    }
}