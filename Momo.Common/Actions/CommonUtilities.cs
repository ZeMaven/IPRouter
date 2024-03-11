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
            var Rnd2 = new Random().Next(1, 9).ToString();
            string TimeStamp = ((DateTimeOffset)DateTime.UtcNow).ToUnixTimeSeconds().ToString();
            var Date = DateTime.Now.ToString("ddMMyyhhmm");
            var Rnd1 = new Random().Next(100, 999).ToString();
            var TranId = $"{Date}{TimeStamp}{Rnd1}{Rnd2}";
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

    }
}
