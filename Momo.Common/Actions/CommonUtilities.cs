using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Momo.Common.Actions
{
    public interface ICommonUtilities
    {
        string CreateTransactionId();
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



    }
}
