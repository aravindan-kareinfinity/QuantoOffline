using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading.Tasks;

namespace Quanto.WinService
{
    public class License
    {
        static string privatekey = null;
        public static string Key
        {
            get
            {
                return CreateKey(PrivateKey);
            }
        }
        private const string CHARACTERS = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        public static string Encode36(int number)
        {
            number = Math.Abs(number);
            List<char> database = new List<char>(CHARACTERS);
            List<char> value = new List<char>();
            long tmp = number;

            while (tmp != 0)
            {
                value.Add(database[Convert.ToInt32(tmp % 36)]);
                tmp /= 36;
            }
            value.Reverse();
            return new string(value.ToArray());
        }

        public static string PrivateKey
        {
            get
            {
                if (!string.IsNullOrEmpty(privatekey))
                    return privatekey;

                ManagementClass mc = new ManagementClass("win32_processor");
                ManagementObjectCollection moc = mc.GetInstances();

                privatekey = "";
                foreach (ManagementObject mo in moc)
                {
                    if (privatekey == "")
                    {
                        privatekey = mo.Properties["processorID"].Value.ToString() + "-" + mo.Properties["SystemName"].Value.ToString();
                        break;
                    }
                }
                return privatekey;
            }
        }
        public static string CreateKey(string encryptionkey)
        {
            System.Security.Cryptography.MD5 md5Hasher = System.Security.Cryptography.MD5.Create();
            var hashed = md5Hasher.ComputeHash(Encoding.UTF8.GetBytes(encryptionkey));
            return Encode36(BitConverter.ToInt32(hashed, 0)) + "-" + Encode36(BitConverter.ToInt32(hashed, 4)) + "-" + Encode36(BitConverter.ToInt32(hashed, 8)) + "-" + Encode36(BitConverter.ToInt32(hashed, 12));
        }
    }
}
