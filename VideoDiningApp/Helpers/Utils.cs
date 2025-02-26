using System;
using System.Security.Cryptography;
using System.Text;

namespace VideoDiningApp.Helpers
{
    public static class Utils
    {
        public static string CalculateRFC2104HMAC(string data, string key)
        {
            using (HMACSHA256 hmac = new HMACSHA256(Encoding.UTF8.GetBytes(key)))
            {
                byte[] hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(data));
                return BitConverter.ToString(hash).Replace("-", "").ToLower();
            }
        }
    }
}
