using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AP.Shell
{
    public class Security
    {
        public static string HashSHA512(string value)
        {
            var sha1 = System.Security.Cryptography.SHA512.Create();
            var inputBytes = Encoding.ASCII.GetBytes(value);
            var hash = sha1.ComputeHash(inputBytes);

            var sb = new StringBuilder();
            for (var i = 0; i < hash.Length; i++)
            {
                sb.Append(hash[i].ToString("x2"));
            }
            return sb.ToString();
        }
    }
}
