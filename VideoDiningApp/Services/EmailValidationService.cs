using System.Net;
using System.Text.RegularExpressions;

namespace VideoDiningApp.Services
{

    public class EmailValidationService
    {
        public static bool IsValidGmail(string email)
        {
            if (!Regex.IsMatch(email, @"^[a-zA-Z0-9._%+-]+@gmail\.com$"))
                return false;

            try
            {
                string domain = email.Split('@')[1];
                var mxRecords = Dns.GetHostAddresses(domain);
                return mxRecords.Length > 0;
            }
            catch
            {
                return false; 
            }
        }
    }
}