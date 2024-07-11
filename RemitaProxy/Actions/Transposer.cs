using Momo.Common.Actions;
using Momo.Common.Models;
using System.ComponentModel.Design;

namespace RemitaProxy.Actions
{
    public interface ITransposer
    {
        BankDetails GetBank(string BankCode);
        ResponseHeader Response(string ResponseCode);
    }

    public class Transposer : ITransposer
    {


        private readonly IConfiguration Config;
        private readonly ILog Log;


        public Transposer(IConfiguration config, ILog log)
        {
            Log = log;
            Config = config;

        }

        public ResponseHeader Response(string ResponseCode)
        {
            switch (ResponseCode)
            {
                case "00":
                    return new ResponseHeader()
                    {
                        ResponseCode = "00",
                        ResponseMessage = "Successful"
                    };
                case "02":
                    return new ResponseHeader()
                    {
                        ResponseCode = "02",
                        ResponseMessage = "bad request"
                    };
                case "07":
                    return new ResponseHeader()
                    {
                        ResponseCode = "07",
                        ResponseMessage = "Invalid account"
                    };
                case "16":
                    return new ResponseHeader()
                    {
                        ResponseCode = "16",
                        ResponseMessage = "Unknown bank code"
                    };
                case "99":
                    return new ResponseHeader()
                    {
                        ResponseCode = "99",
                        ResponseMessage = "Invalid source account"
                    };
                case "03":
                    return new ResponseHeader()
                    {
                        ResponseCode = "03",
                        ResponseMessage = "Invalid source account"
                    };
                case "13":
                    return new ResponseHeader()
                    {
                        ResponseCode = "13",
                        ResponseMessage = "Invalid amount"
                    };
                case "17":
                    return new ResponseHeader()
                    {
                        ResponseCode = "17",
                        ResponseMessage = "Invalid source bank code"
                    };
                case "18":
                    return new ResponseHeader()
                    {
                        ResponseCode = "18",
                        ResponseMessage = "Invalid baneficiary bank code"
                    };
                case "26":
                    return new ResponseHeader()
                    {
                        ResponseCode = "26",
                        ResponseMessage = "Duplicate record"
                    };
                case "31":
                    return new ResponseHeader()
                    {
                        ResponseCode = "38",
                        ResponseMessage = "Unauthorized debit account"
                    };
                case "10":
                    return new ResponseHeader()
                    {
                        ResponseCode = "10",
                        ResponseMessage = "Card has been locked"
                    };
                case "51":
                    return new ResponseHeader()
                    {
                        ResponseCode = "51",
                        ResponseMessage = "Insufficientfund"
                    };
                case "52":
                    return new ResponseHeader()
                    {
                        ResponseCode = "52",
                        ResponseMessage = "Insufficientfund"
                    };
                case "62":
                    return new ResponseHeader()
                    {
                        ResponseCode = "62",
                        ResponseMessage = "Transfer limit exceeded"
                    };
                case "91":
                    return new ResponseHeader()
                    {
                        ResponseCode = "91",
                        ResponseMessage = "Beneficiary bank not available"
                    };
                case "94":
                    return new ResponseHeader()
                    {
                        ResponseCode = "94",
                        ResponseMessage = "Duplicate transaction"
                    };
                case "96":
                    return new ResponseHeader()
                    {
                        ResponseCode = "96",
                        ResponseMessage = "Systemmalfunction"
                    };
                case "103":
                    return new ResponseHeader()
                    {
                        ResponseCode = "103",
                        ResponseMessage = "Debit ok"//??
                    };                
                default:
                    return new ResponseHeader()
                    {
                        ResponseCode = "97",
                        ResponseMessage = "Unknown Response Code"
                    };
            }

        }



        public BankDetails GetBank(string BankCode)
        {
            var BankList = Config.GetSection("BankCodes").Get<List<BankDetails>>();

            var Bank = BankList!.Where(x => x.CbnCode == BankCode || x.NibssCode == BankCode).FirstOrDefault();


            if (Bank != null)
                return Bank;
            else
                return new BankDetails();

        }

    }
}
