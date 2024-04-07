using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Momo.Common.Actions
{
    public interface ICommonUtilities
    {
        string CreateTransactionId();
        string GetPasswordHash(string password);
    }

    public class CommonUtilities : ICommonUtilities
    {
        /// <summary>
        /// Returns 20 characters
        /// </summary>
        /// <returns></returns>
        public string CreateTransactionId()
        {
         
            var Rnd4 = new Random().Next(1000, 9999).ToString();
            var Rnd3 = new Random().Next(100, 999).ToString();
            var Rnd2 = new Random().Next(10, 99).ToString();
            var Rnd1 = new Random().Next(1, 9).ToString();
            string TimeStamp = ((DateTimeOffset)DateTime.UtcNow).ToUnixTimeSeconds().ToString();
            //var Date = DateTime.Now.ToString("ddMMyyhhmm");
            var Date = DateTime.Now.ToString("yyMMddHHmmssfff");
            var Rnd2a = new Random().Next(10, 99).ToString();
            var TranId = $"{Date}{Rnd1}{Rnd2}{Rnd4}{Rnd2a}";
            return TranId;
        }

        public string GetPasswordHash(string password)
        {
            var hashedValue = new StringBuilder();
            using (var hash = SHA256.Create())
            {
                var encoding = Encoding.UTF8;
                var result = hash.ComputeHash(encoding.GetBytes(password));

                foreach (var b in result)
                {
                    hashedValue.Append(b.ToString("x2"));
                }
            }
            return hashedValue.ToString();
        }


        public string ResponseCodeTranslate(string ResponseCode, string Processor)
        {

            return null;
        }




    }
}
