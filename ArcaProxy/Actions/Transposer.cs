using Momo.Common.Actions;
using Momo.Common.Models;

namespace ArcaProxy.Actions
{
    public interface ITransposer
    {
        ResponseHeader Response(string Status);
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

        public ResponseHeader Response(string Status)
        {

            switch (Status.ToUpper())
            {
                case "APPROVED":
                    return new ResponseHeader()
                    {
                        ResponseCode = "00",
                        ResponseMessage = "Successful"
                    };
                case "FAILED":
                    return new ResponseHeader()
                    {
                        ResponseCode = "96",
                        ResponseMessage = "Failed"
                    };
                case "DECLINED":
                    return new ResponseHeader()
                    {
                        ResponseCode = "96",
                        ResponseMessage = "Failed"
                    };
                case "IN_PROGRESS":
                    return new ResponseHeader()
                    {
                        ResponseCode = "09",
                        ResponseMessage = "Pending"
                    };
                case " PENDING_RETRY":
                    return new ResponseHeader()
                    {
                        ResponseCode = "09",
                        ResponseMessage = "Pending"
                    };





                   

                default:
                    return new ResponseHeader()
                    {
                        ResponseCode = "97",
                        ResponseMessage = "Unknown Response Code"
                    };



            }

        }
    }
}