using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;
using System.Web;

namespace Quanto
{
    public static class Utility
    {
        public static string ToBase64(this byte[] source)
        {
            //        +/=
            string s = Convert.ToBase64String(source); // Regular base64 encoder
            s = s.Split('=')[0]; // Remove any trailing '='s
            s = s.Replace('+', '-'); // 62nd char of encoding
            s = s.Replace('/', '_'); // 63rd char of encoding
            return s;
        }

        public static byte[] FromBase64(this string source)
        {
            string s = source;
            s = s.Replace('-', '+'); // 62nd char of encoding
            s = s.Replace('_', '/'); // 63rd char of encoding
            switch (s.Length % 4) // Pad with trailing '='s
            {
                case 0: break; // No pad chars in this case
                case 2: s += "=="; break; // Two pad chars
                case 3: s += "="; break; // One pad char
                default:
                    return null;
            }
            return Convert.FromBase64String(s);
        }

        public static string ToJSON(this object obj)
        {
            return JsonConvert.SerializeObject(obj);
        }
        public static string Encrypt(this string source,string additionalKey)
        {
            return source;
        }
        public static string Decrypt(this string source, string additionalKey)
        {
            return source;
        }

        public static T ToEnum<T>(this string source) where T : struct
        {
            T t = new T();
            Enum.TryParse<T>(source, out t);
            return t;
        }

        public static T Json2Object<T>(this string source)
        {
            return (T) JsonConvert.DeserializeObject(source,typeof(T));
        }
        public static DateTimeOffset Convert2Timezone(this DateTime source)
        {
            return (DateTimeOffset)source.ToUniversalTime();
        }
        public static string OTP(int lenthofpass)
        {
            string allowedChars = "";
            allowedChars = "a,b,c,d,e,f,g,h,i,j,k,l,m,n,o,p,q,r,s,t,u,v,w,x,y,z,";
            allowedChars += "A,B,C,D,E,F,G,H,I,J,K,L,M,N,O,P,Q,R,S,T,U,V,W,X,Y,Z,";
            allowedChars += "2,3,4,5,6,7,8,9";
            char[] sep = { ',' };
            string[] arr = allowedChars.Split(sep);
            string passwordString = "";
            string temp = "";
            Random rand = new Random();
            for (int i = 0; i < lenthofpass; i++)
            {
                temp = arr[rand.Next(0, arr.Length)];
                passwordString += temp;
            }
            return passwordString;
        }
        //static public X509Certificate2 GetCert(string cn, TimeSpan expirationLength, string pwd = "", string filename = null)
        //{
        //    // http://stackoverflow.com/questions/18339706/how-to-create-self-signed-certificate-programmatically-for-wcf-service
        //    // http://stackoverflow.com/questions/21629395/http-listener-with-https-support-coded-in-c-sharp
        //    // https://msdn.microsoft.com/en-us/library/system.security.cryptography.x509certificates.storename(v=vs.110).aspx
        //    // create DN for subject and issuer
        //    var base64encoded = string.Empty;
        //    if (filename != null && File.Exists(filename))
        //    {
        //        base64encoded = File.ReadAllText(filename);
        //    }
        //    else
        //    {
        //        base64encoded = CreateCertContent(cn, expirationLength, pwd);
        //        if (filename != null)
        //        {
        //            File.WriteAllText(filename, base64encoded);
        //        }
        //    }
        //    // instantiate the target class with the PKCS#12 data (and the empty password)
        //    var rlt = new System.Security.Cryptography.X509Certificates.X509Certificate2(
        //        System.Convert.FromBase64String(base64encoded), pwd,
        //        // mark the private key as exportable (this is usually what you want to do)
        //        // mark private key to go into the Machine store instead of the current users store
        //        X509KeyStorageFlags.Exportable | X509KeyStorageFlags.MachineKeySet | X509KeyStorageFlags.PersistKeySet
        //        );
        //    return rlt;
        //}

        //private static string CreateCertContent(string cn, TimeSpan expirationLength, string pwd)
        //{
        //    string base64encoded = string.Empty;
        //    var dn = new CX500DistinguishedName();
        //    dn.Encode("CN=" + cn, X500NameFlags.XCN_CERT_NAME_STR_NONE);

        //    CX509PrivateKey privateKey = new CX509PrivateKey();
        //    privateKey.ProviderName = "Microsoft Strong Cryptographic Provider";
        //    privateKey.Length = 2048;
        //    privateKey.KeySpec = X509KeySpec.XCN_AT_KEYEXCHANGE;
        //    privateKey.KeyUsage = X509PrivateKeyUsageFlags.XCN_NCRYPT_ALLOW_DECRYPT_FLAG |
        //                          X509PrivateKeyUsageFlags.XCN_NCRYPT_ALLOW_KEY_AGREEMENT_FLAG;
        //    privateKey.MachineContext = true;
        //    privateKey.ExportPolicy = X509PrivateKeyExportFlags.XCN_NCRYPT_ALLOW_PLAINTEXT_EXPORT_FLAG;
        //    privateKey.Create();

        //    // Use the stronger SHA512 hashing algorithm
        //    var hashobj = new CObjectId();
        //    hashobj.InitializeFromAlgorithmName(ObjectIdGroupId.XCN_CRYPT_HASH_ALG_OID_GROUP_ID,
        //        ObjectIdPublicKeyFlags.XCN_CRYPT_OID_INFO_PUBKEY_ANY,
        //        AlgorithmFlags.AlgorithmFlagsNone, "SHA512");

        //    // Create the self signing request
        //    var cert = new CX509CertificateRequestCertificate();
        //    cert.InitializeFromPrivateKey(X509CertificateEnrollmentContext.ContextMachine, privateKey, "");
        //    cert.Subject = dn;
        //    cert.Issuer = dn; // the issuer and the subject are the same
        //    cert.NotBefore = DateTime.Now.Date;
        //    // this cert expires immediately. Change to whatever makes sense for you
        //    cert.NotAfter = cert.NotBefore + expirationLength;
        //    cert.HashAlgorithm = hashobj; // Specify the hashing algorithm
        //    cert.Encode(); // encode the certificate

        //    // Do the final enrollment process
        //    var enroll = new CX509Enrollment();
        //    enroll.InitializeFromRequest(cert); // load the certificate
        //    enroll.CertificateFriendlyName = cn; // Optional: add a friendly name
        //    string csr = enroll.CreateRequest(); // Output the request in base64
        //    // and install it back as the response
        //    enroll.InstallResponse(InstallResponseRestrictionFlags.AllowUntrustedCertificate,
        //        csr, EncodingType.XCN_CRYPT_STRING_BASE64, pwd); // no password
        //    // output a base64 encoded PKCS#12 so we can import it back to the .Net security classes
        //    base64encoded = enroll.CreatePFX(pwd, // no password, this is for internal consumption
        //        PFXExportOptions.PFXExportChainWithRoot);
        //    return base64encoded;
        //}
        static private string GetAppId()
        {
            Assembly assembly = Assembly.GetExecutingAssembly();

            //The following line (part of the original answer) is misleading.
            //**Do not** use it unless you want to return the System.Reflection.Assembly type's GUID.
            //Console.WriteLine(assembly.GetType().GUID.ToString());

            // The following is the correct code.
            var attribute = (GuidAttribute)assembly.GetCustomAttributes(typeof(GuidAttribute), true)[0];
            var id = attribute.Value;
            return id;
        }
    }

}