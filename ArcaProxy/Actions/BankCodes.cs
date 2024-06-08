using Momo.Common.Models;

namespace ArcaProxy.Actions
{
    public interface IBankCodes
    {
        BankDetails GetBank(string BankCode);
    }

    public class BankCodes : IBankCodes
    {
        private readonly IConfiguration Config;

        public BankCodes(IConfiguration config)
        {
            Config = config;
        }

        public BankDetails GetBank(string BankCode)
        {
            var BankList = Config.GetSection("BankCodes").Get<List<BankDetails>>();

            var Bank = BankList.Where(x => x.CbnCode == BankCode || x.NibssCode == BankCode).FirstOrDefault();


            if (Bank != null)
                return Bank;
            else
                return new BankDetails();

        }
    }
}