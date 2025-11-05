using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace wpfpr3.Services
{
    public class Hash
    {
        public static string HashPassword(string password)
        {
            using(SHA256  sha256Hash = SHA256.Create())
            {
                byte[] inpBytePass = Encoding.UTF8.GetBytes(password);
                byte[] hashPass = sha256Hash.ComputeHash(inpBytePass);
                return BitConverter.ToString(hashPass).Replace("-", String.Empty);
            }
                
        }
    }
}
