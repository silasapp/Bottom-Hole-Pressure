using Microsoft.Extensions.Configuration;
using System.Security.Cryptography;
using System.Text;

namespace BHP.Helpers
{
    public class ElpsServices
    {
        public IConfiguration _configuration;

        public static string _elpsBaseUrl;
        public static string _elpsAppKey; // demo - "204579109396";
        public static string _elpsAppEmail;
        public static string appHash;
        public static string link;
        public static string public_key; // demo - "80160797-998c-4292-9118-92859539572b";

        /*
         * Intializing AppHash for passing.
         */
        public ElpsServices()
        {
            appHash = this.GenerateSHA512(_elpsAppEmail.Trim() + _elpsAppKey.Trim());
        }




        public bool CodeCheck(string email, string code)
        {
            var StringCode = public_key.ToUpper().Trim() + "." + email.ToUpper().Trim() + "." + _elpsAppKey.ToUpper().Trim();

            var hashCode = this.GenerateSHA512(StringCode);

            if (hashCode == code)
            {
                return true;
            }
            else
            {
                return false;
            }
        }



        /*
         * Generating api hash as app_id
         */
        public string GenerateSHA512(string inputString)
        {
            SHA512 sha512 = SHA512Managed.Create();
            byte[] bytes = Encoding.UTF8.GetBytes(inputString);
            byte[] hash = sha512.ComputeHash(bytes);
            StringBuilder sb = new StringBuilder();

            for (int i = 0; i < hash.Length; i++)
            {
                sb.Append(hash[i].ToString("X2"));
            }

            return sb.ToString();

        }



    }
}
