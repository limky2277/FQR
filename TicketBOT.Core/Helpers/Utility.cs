using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace TicketBOT.Core.Helpers
{
    public static class Utility
    {

        /// <summary>
        /// Encrypt a string.
        /// </summary>
        /// <param name="data">data to be parsed</param>
        /// <param name="info">info for parsing</param>
        public static string ParseEInfo(string data, string info)
        {
            if (data == null)
            {
                return null;
            }

            if (info == null)
            {
                info = String.Empty;
            }

            // Get the bytes of the string
            var bytesToBeEncrypted = Encoding.UTF7.GetBytes(data);
            var passwordBytes = Encoding.UTF7.GetBytes(info);

            // Hash the password with SHA256
            passwordBytes = SHA256.Create().ComputeHash(passwordBytes);

            var bytesEncrypted = GetEInfo(bytesToBeEncrypted, passwordBytes);

            return Convert.ToBase64String(bytesEncrypted);
        }

        /// <summary>
        /// ParseInfo from a string.
        /// </summary>
        /// <param name="text">String to be parse</param>
        /// <param name="info">info for parsing</param>
        /// <exception cref="FormatException"></exception>
        public static string ParseDInfo(string text, string info)
        {
            try
            {
                if (text == null)
                {
                    return null;
                }

                if (info == null)
                {
                    info = String.Empty;
                }

                // Get the bytes of the string
                var bytesToBeDecrypted = Convert.FromBase64String(text);
                var passwordBytes = Encoding.UTF7.GetBytes(info);

                passwordBytes = SHA256.Create().ComputeHash(passwordBytes);

                var bytesDecrypted = GetDInfo(bytesToBeDecrypted, passwordBytes);

                return Encoding.UTF7.GetString(bytesDecrypted);
            }
            catch (Exception)
            {
                return "";
            }
        }

        private static byte[] GetEInfo(byte[] bytesToBeEncrypted, byte[] passwordBytes)
        {
            byte[] encryptedBytes = null;

            // Set your salt here, change it to meet your flavor:
            // The salt bytes must be at least 8 bytes.
            var saltBytes = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 };

            using (MemoryStream ms = new MemoryStream())
            {
                using (RijndaelManaged AES = new RijndaelManaged())
                {
                    var key = new Rfc2898DeriveBytes(passwordBytes, saltBytes, 1000);

                    AES.KeySize = 256;
                    AES.BlockSize = 128;
                    AES.Key = key.GetBytes(AES.KeySize / 8);
                    AES.IV = key.GetBytes(AES.BlockSize / 8);

                    AES.Mode = CipherMode.CBC;

                    using (var cs = new CryptoStream(ms, AES.CreateEncryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(bytesToBeEncrypted, 0, bytesToBeEncrypted.Length);
                        cs.Close();
                    }

                    encryptedBytes = ms.ToArray();
                }
            }

            return encryptedBytes;
        }

        private static byte[] GetDInfo(byte[] bytesToBeDecrypted, byte[] passwordBytes)
        {
            byte[] decryptedBytes = null;

            // Set your salt here, change it to meet your flavor:
            // The salt bytes must be at least 8 bytes.
            var saltBytes = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 };

            using (MemoryStream ms = new MemoryStream())
            {
                using (RijndaelManaged AES = new RijndaelManaged())
                {
                    var key = new Rfc2898DeriveBytes(passwordBytes, saltBytes, 1000);

                    AES.KeySize = 256;
                    AES.BlockSize = 128;
                    AES.Key = key.GetBytes(AES.KeySize / 8);
                    AES.IV = key.GetBytes(AES.BlockSize / 8);
                    AES.Mode = CipherMode.CBC;

                    using (var cs = new CryptoStream(ms, AES.CreateDecryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(bytesToBeDecrypted, 0, bytesToBeDecrypted.Length);
                        cs.Close();
                    }

                    decryptedBytes = ms.ToArray();
                }
            }

            return decryptedBytes;
        }
    }
}
