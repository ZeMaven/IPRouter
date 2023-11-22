using Azure.Core;
using Momo.Common.Models;

namespace MomoSwitch.Actions
{
    public interface IInward
    {
        Task<FundTransferPxResponse> FundTransfer(FundTransferPxRequest request);
        Task<NameEnquiryPxResponse> NameEnquiry(NameEnquiryPxRequest Request);
        Task<TranQueryPxResponse> TransactionQuery(TranQueryPxRequest request);
    }

    public class Inward : IInward
    {
        private readonly IConfiguration Config;
        private readonly ILog Log;



        public Inward(IConfiguration config, ILog log)
        {
            Log = log;
            Config = config;
        }



        public async Task<NameEnquiryPxResponse> NameEnquiry(NameEnquiryPxRequest Request)
        {
            try
            {

                //Transform to momo request
                //Call momo
                //Tranform to Proxy response
                //Return to Proxy




                return new NameEnquiryPxResponse
                {

                };


            }
            catch (Exception Ex)
            {
                Log.Write("Inward.NameEnquiry", $"Err: {Ex.Message}");
                return new NameEnquiryPxResponse
                {
                    SessionId = "",
                    TransactionId = Request.TransactionId,
                    SourceBankCode = "",
                    ResponseCode = "01",
                    ResponseMessage = ""
                };
            }
        }


        public async Task<TranQueryPxResponse> TransactionQuery(TranQueryPxRequest request)
        {
            try
            {

                //Transform to momo request
                //Call momo
                //Tranform to Proxy response
                //Return to Proxy


                return new TranQueryPxResponse { TransactionId = request.TransactionId };


            }
            catch (Exception Ex)
            {

                Log.Write("Inward.TransactionQuery", $"Err: {Ex.Message}");
                return new TranQueryPxResponse { TransactionId = request.TransactionId };

            }

        }





        public async Task<FundTransferPxResponse> FundTransfer(FundTransferPxRequest request)
        {
            try
            {

                //Transform to momo request
                //Call momo
                //Tranform to Proxy response
                //Return to Proxy



                return new FundTransferPxResponse
                {

                };

            }
            catch (Exception Ex)
            {
                Log.Write("Inward.FundTransfer", $"Err: {Ex.Message}");
                return new FundTransferPxResponse
                {

                };
            }
        }
    }
}
