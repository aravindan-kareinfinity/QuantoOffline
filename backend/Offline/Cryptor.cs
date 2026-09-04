using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Security.Cryptography;

namespace WebAPI.Data
{
    public static class Cryptor
    {

        public class AESEncryption
        {

            // Change these keys
            private byte[] Key = { 123, 217, 19, 11, 24, 26, 85, 45, 114, 184, 27, 162, 37, 112, 222, 209, 241, 24, 175, 144, 173, 53, 196, 29, 24, 26, 17, 218, 131, 236, 53, 209 };
            private byte[] Vector = { 146, 64, 191, 111, 23, 3, 113, 119, 231, 121, 252, 112, 79, 32, 114, 156 };


            private ICryptoTransform EncryptorTransform, DecryptorTransform;
            private System.Text.UTF8Encoding UTFEncoder;

            public byte[] GetKeys(byte[] source, string compain)
            {
                byte[] bytes = UTFEncoder.GetBytes(compain);
                for (int i = 0; i < bytes.Length; i++)
                {
                    source[i] = bytes[i];
                }
                return source;
            }

            public AESEncryption(string key, string vector)
            {
                if (key.Length > Key.Length)
                    key = key.Substring(0, Key.Length);

                if (vector.Length > Vector.Length)
                    vector = vector.Substring(0, Vector.Length);

                //Used to translate bytes to text and vice versa
                UTFEncoder = new System.Text.UTF8Encoding();

                //This is our encryption method
                RijndaelManaged rm = new RijndaelManaged();

                Key = GetKeys(Key, key);
                Vector = GetKeys(Vector, vector);

                //Create an encryptor and a decryptor using our encryption method, key, and vector.
                EncryptorTransform = rm.CreateEncryptor(this.Key, this.Vector);
                DecryptorTransform = rm.CreateDecryptor(this.Key, this.Vector);
            }

            /// -------------- Two Utility Methods (not used but may be useful) -----------
            /// Generates an encryption key.
            static public byte[] GenerateEncryptionKey()
            {
                //Generate a Key.
                RijndaelManaged rm = new RijndaelManaged();
                rm.GenerateKey();
                return rm.Key;
            }

            /// Generates a unique encryption vector
            static public byte[] GenerateEncryptionVector()
            {
                //Generate a Vector
                RijndaelManaged rm = new RijndaelManaged();
                rm.GenerateIV();
                return rm.IV;
            }


            /// ----------- The commonly used methods ------------------------------    
            /// Encrypt some text and return a string suitable for passing in a URL.
            public string EncryptToString(string TextValue)
            {
                return ByteArrToString(Encrypt(TextValue));
            }

            /// Encrypt some text and return an encrypted byte array.
            public byte[] Encrypt(string TextValue)
            {
                //Translates our text value into a byte array.
                Byte[] bytes = UTFEncoder.GetBytes(TextValue);

                //Used to stream the data in and out of the CryptoStream.
                MemoryStream memoryStream = new MemoryStream();

                /*
                 * We will have to write the unencrypted bytes to the stream,
                 * then read the encrypted result back from the stream.
                 */
                #region Write the decrypted value to the encryption stream
                CryptoStream cs = new CryptoStream(memoryStream, EncryptorTransform, CryptoStreamMode.Write);
                cs.Write(bytes, 0, bytes.Length);
                cs.FlushFinalBlock();
                #endregion

                #region Read encrypted value back out of the stream
                memoryStream.Position = 0;
                byte[] encrypted = new byte[memoryStream.Length];
                memoryStream.Read(encrypted, 0, encrypted.Length);
                #endregion

                //Clean up.
                cs.Close();
                memoryStream.Close();

                return encrypted;
            }

            /// The other side: Decryption methods
            public string DecryptString(string EncryptedString)
            {
                return Decrypt(StrToByteArray(EncryptedString));
            }

            /// Decryption when working with byte arrays.    
            public string Decrypt(byte[] EncryptedValue)
            {
                #region Write the encrypted value to the decryption stream
                MemoryStream encryptedStream = new MemoryStream();
                CryptoStream decryptStream = new CryptoStream(encryptedStream, DecryptorTransform, CryptoStreamMode.Write);
                decryptStream.Write(EncryptedValue, 0, EncryptedValue.Length);
                decryptStream.FlushFinalBlock();
                #endregion

                #region Read the decrypted value from the stream.
                encryptedStream.Position = 0;
                Byte[] decryptedBytes = new Byte[encryptedStream.Length];
                encryptedStream.Read(decryptedBytes, 0, decryptedBytes.Length);
                encryptedStream.Close();
                #endregion
                return UTFEncoder.GetString(decryptedBytes);
            }

            /// Convert a string to a byte array.  NOTE: Normally we'd create a Byte Array from a string using an ASCII encoding (like so).
            //      System.Text.ASCIIEncoding encoding = new System.Text.ASCIIEncoding();
            //      return encoding.GetBytes(str);
            // However, this results in character values that cannot be passed in a URL.  So, instead, I just
            // lay out all of the byte values in a long string of numbers (three per - must pad numbers less than 100).
            public byte[] StrToByteArray(string str)
            {
                if (str.Length == 0)
                    throw new Exception("Invalid string value in StrToByteArray");

                byte val;
                byte[] byteArr = new byte[str.Length / 3];
                int i = 0;
                int j = 0;
                do
                {
                    val = byte.Parse(str.Substring(i, 3));
                    byteArr[j++] = val;
                    i += 3;
                }
                while (i < str.Length);
                return byteArr;
            }

            // Same comment as above.  Normally the conversion would use an ASCII encoding in the other direction:
            //      System.Text.ASCIIEncoding enc = new System.Text.ASCIIEncoding();
            //      return enc.GetString(byteArr);    
            public string ByteArrToString(byte[] byteArr)
            {
                byte val;
                string tempStr = "";
                for (int i = 0; i <= byteArr.GetUpperBound(0); i++)
                {
                    val = byteArr[i];
                    if (val < (byte)10)
                        tempStr += "00" + val.ToString();
                    else if (val < (byte)100)
                        tempStr += "0" + val.ToString();
                    else
                        tempStr += val.ToString();
                }
                return tempStr;
            }
        }

        public static string DefaultPassword = "TTDWebAPI";
        public static string DefaultToken = "PopinToken";

        public static string EncryptString(string InputText, string Password)
        {
            // We are now going to create an instance of the
            // Rihndael class. 
            RijndaelManaged RijndaelCipher = new RijndaelManaged();

            // First we need to turn the input strings into a byte array.
            byte[] PlainText = System.Text.Encoding.Unicode.GetBytes(InputText);

            // We are using salt to make it harder to guess our key
            // using a dictionary attack.
            byte[] Salt = Encoding.ASCII.GetBytes(Password.Length.ToString());

            // The (Secret Key) will be generated from the specified
            // password and salt.
            PasswordDeriveBytes SecretKey = new PasswordDeriveBytes(Password, Salt);

            // Create a encryptor from the existing SecretKey bytes.
            // We use 32 bytes for the secret key
            // (the default Rijndael key length is 256 bit = 32 bytes) and
            // then 16 bytes for the IV (initialization vector),
            // (the default Rijndael IV length is 128 bit = 16 bytes)
            ICryptoTransform Encryptor = RijndaelCipher.CreateEncryptor(SecretKey.GetBytes(32), SecretKey.GetBytes(16));

            // Create a MemoryStream that is going to hold the encrypted bytes
            MemoryStream memoryStream = new MemoryStream();

            // Create a CryptoStream through which we are going to be processing our data.
            // CryptoStreamMode.Write means that we are going to be writing data
            // to the stream and the output will be written in the MemoryStream
            // we have provided. (always use write mode for encryption)
            CryptoStream cryptoStream = new CryptoStream(memoryStream, Encryptor, CryptoStreamMode.Write);

            // Start the encryption process.
            cryptoStream.Write(PlainText, 0, PlainText.Length);

            // Finish encrypting.
            cryptoStream.FlushFinalBlock();

            // Convert our encrypted data from a memoryStream into a byte array.
            byte[] CipherBytes = memoryStream.ToArray();



            // Close both streams.
            memoryStream.Close();   
            cryptoStream.Close();

            // Convert encrypted data into a base64-encoded string.
            // A common mistake would be to use an Encoding class for that.
            // It does not work, because not all byte values can be
            // represented by characters. We are going to be using Base64 encoding
            // That is designed exactly for what we are trying to do.
            string EncryptedData = Convert.ToBase64String(CipherBytes);

            // Return encrypted string.
            return EncryptedData;
        }


        public static string DecryptString(string InputText, string Password)
        {
            RijndaelManaged RijndaelCipher = new RijndaelManaged();

            byte[] EncryptedData = Convert.FromBase64String(InputText);
            byte[] Salt = Encoding.ASCII.GetBytes(Password.Length.ToString());

            PasswordDeriveBytes SecretKey = new PasswordDeriveBytes(Password, Salt);

            // Create a decryptor from the existing SecretKey bytes.
            ICryptoTransform Decryptor = RijndaelCipher.CreateDecryptor(SecretKey.GetBytes(32), SecretKey.GetBytes(16));

            MemoryStream memoryStream = new MemoryStream(EncryptedData);

            // Create a CryptoStream. (always use Read mode for decryption).
            CryptoStream cryptoStream = new CryptoStream(memoryStream, Decryptor, CryptoStreamMode.Read);

            // Since at this point we don't know what the size of decrypted data
            // will be, allocate the buffer long enough to hold EncryptedData;
            // DecryptedData is never longer than EncryptedData.
            byte[] PlainText = new byte[EncryptedData.Length];

            // Start decrypting.
            int DecryptedCount = cryptoStream.Read(PlainText, 0, PlainText.Length);

            memoryStream.Close();
            cryptoStream.Close();

            // Convert decrypted data into a string.
            string DecryptedData = Encoding.Unicode.GetString(PlainText, 0, DecryptedCount);

            // Return decrypted string.  
            return DecryptedData;
        }

    }
}