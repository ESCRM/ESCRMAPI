using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Security.Cryptography;

namespace IDS.EBSTCRM.Base.Encryption
{
    public static class DES_CBC_PKCS5 
    {

        public static string Encrypt(string text, string key)
        {
            //string text = "punch|Punch54321|Martin Exner Jensen|martin@lbe.dk|1|1|";

            //string key = "12345678";		// Key must be 64 bits for DES
            string iv = "0000000000000000";	// Init vector must match what is used on the other end

            byte[] textBytes = new byte[text.Length];
            textBytes = Encoding.UTF8.GetBytes(text);	// Guess that UTF8-encoding is used

            byte[] keyBytes = new byte[key.Length];
            keyBytes = Encoding.UTF8.GetBytes(key);

            byte[] ivBytes = new byte[iv.Length];
            ivBytes = HexStringToByteArray(iv);

            byte[] encrptedBytes = Encrypt(textBytes, keyBytes, ivBytes);

            return ByteArrayToHexString(encrptedBytes);
        }

        public static byte[] Encrypt(byte[] clearData, byte[] Key, byte[] IV)
        {
            MemoryStream ms = new MemoryStream();
            // Create a symmetric algorithm.
            DES alg = DES.Create();
            alg.Padding = PaddingMode.PKCS7;	// PKCS#5 and #7 are the same (Wikipedia: Padding (cryptography))
            alg.Mode = CipherMode.CBC;
            alg.Key = Key;
            alg.IV = IV;

            CryptoStream cs = new CryptoStream(ms, alg.CreateEncryptor(), CryptoStreamMode.Write);
            cs.Write(clearData, 0, clearData.Length);
            cs.Close();

            byte[] encryptedData = ms.ToArray();
            return encryptedData;
        }

        public static byte[] HexStringToByteArray(string hex)
        {
            if (hex.Length % 2 == 1)
                throw new Exception("The binary key cannot have an odd number of digits");

            byte[] arr = new byte[hex.Length >> 1];

            for (int i = 0; i < hex.Length >> 1; ++i)
            {
                arr[i] = (byte)((GetHexVal(hex[i << 1]) << 4) + (GetHexVal(hex[(i << 1) + 1])));
            }

            return arr;
        }

        public static int GetHexVal(char hex)
        {
            int val = (int)hex;
            //For uppercase A-F letters:
            //return val - (val < 58 ? 48 : 55);
            //For lowercase a-f letters:
            //return val - (val < 58 ? 48 : 87);
            //Or the two combined, but a bit slower:
            return val - (val < 58 ? 48 : (val < 97 ? 55 : 87));
        }

        public static string ByteArrayToHexString(byte[] Bytes)
        {
            StringBuilder Result = new StringBuilder(Bytes.Length * 2);
            string HexAlphabet = "0123456789ABCDEF";

            foreach (byte B in Bytes)
            {
                Result.Append(HexAlphabet[(int)(B >> 4)]);
                Result.Append(HexAlphabet[(int)(B & 0xF)]);
            }

            return Result.ToString();
        }
	

    }
}
