using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.Text;
namespace RSAencrypt
{

    public class rsa
    {

        public RSACryptoServiceProvider rsa_servidor = new RSACryptoServiceProvider(2048);
        public RSACryptoServiceProvider rsa_cliente = new RSACryptoServiceProvider(2048);
        public static string Encrypt(string text, RSAParameters publicParameters)
        {
            byte[] data = Encoding.Default.GetBytes(text);
            using (RSACryptoServiceProvider tester = new RSACryptoServiceProvider())
            {
                tester.ImportParameters(publicParameters);
                byte[] encrypted = tester.Encrypt(data, false);
                string base64 = Convert.ToBase64String(encrypted, 0, encrypted.Length);
                return base64;
            }
        }

        public static string Decrypt(string code, RSACryptoServiceProvider rsa)
        {
            byte[] encrypted = System.Convert.FromBase64String(code);
            byte[] decrypted = rsa.Decrypt(encrypted, false);
            string text = Encoding.UTF8.GetString(decrypted);
            return text;
        }
    }
}