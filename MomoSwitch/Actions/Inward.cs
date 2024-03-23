using Azure.Core;
using Momo.Common.Actions;
using Momo.Common.Models;
using MomoSwitch.Models.Contracts.Specials.Router;

namespace MomoSwitch.Actions
{
    public interface IInward
    {
        Task<AccountBlockPxResponse> AccountBlock(AccountBlockPxRequest request);
        Task<AccountUnBlockPxResponse> AccountUnBlock(AccountUnblockPxRequest request);
        Task<AmountBlockPxResponse> AmountBlock(AmountBlockPxRequest request);
        Task<AmountUnblockPxResponse> AmountUnBlock(AmountUnblockPxRequest request);
        Task<BalanceEnquiryPxResponse> BalanceEnquiry(BalanceEnquiryPxRequest request);
        Task<string> CallBack(CallbackPxRequest request);
        Task<FundTransferPxResponse> DirectCredit(FundTransferPxRequest request);
        Task<DirectCreditAdvicePxResponse> DirectCreditAdvice(DirectCreditAdvicePxRequest request);
        Task<DirectDebitPxResponse> DirectDebit(DirectDebitPxRequest request);
        Task<DirectDebitAdvicePxResponse> DirectDebitAdvice(DirectDebitAdvicePxRequest request);
        Task<FinancialInstitutionListPxResponse> FinancialInstitutionList(FinancialInstitutionListPxRequest request);
        Task<FundTransferPxResponse> FundTransfer(FundTransferPxRequest request);
        Task<MandateAdvicePxResponse> ManadateAdvice(MandateAdvicePxRequest request);
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


                return new TranQueryPxResponse { TransactionId = request.SessionId };


            }
            catch (Exception Ex)
            {

                Log.Write("Inward.TransactionQuery", $"Err: {Ex.Message}");
                return new TranQueryPxResponse { TransactionId = request.SessionId };

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





        public async Task<AccountBlockPxResponse> AccountBlock(AccountBlockPxRequest request)
        {
            try
            {

                //Transform to momo request
                //Call momo
                //Tranform to Proxy response
                //Return to Proxy



                return new AccountBlockPxResponse
                {

                };

            }
            catch (Exception Ex)
            {
                Log.Write("Inward.AccountBlock", $"Err: {Ex.Message}");
                return new AccountBlockPxResponse
                {

                };
            }
        }



        public async Task<AccountUnBlockPxResponse> AccountUnBlock(AccountUnblockPxRequest request)
        {
            try
            {

                //Transform to momo request
                //Call momo
                //Tranform to Proxy response
                //Return to Proxy



                return new AccountUnBlockPxResponse
                {

                };

            }
            catch (Exception Ex)
            {
                Log.Write("Inward.AccountUnBlock", $"Err: {Ex.Message}");
                return new AccountUnBlockPxResponse
                {

                };
            }
        }




        public async Task<AmountBlockPxResponse> AmountBlock(AmountBlockPxRequest request)
        {
            try
            {

                //Transform to momo request
                //Call momo
                //Tranform to Proxy response
                //Return to Proxy



                return new AmountBlockPxResponse
                {

                };

            }
            catch (Exception Ex)
            {
                Log.Write("Inward.AmountBlock", $"Err: {Ex.Message}");
                return new AmountBlockPxResponse
                {

                };
            }
        }







        public async Task<AmountUnblockPxResponse> AmountUnBlock(AmountUnblockPxRequest request)
        {
            try
            {

                //Transform to momo request
                //Call momo
                //Tranform to Proxy response
                //Return to Proxy



                return new AmountUnblockPxResponse
                {

                };

            }
            catch (Exception Ex)
            {
                Log.Write("Inward.AmountUnBlock", $"Err: {Ex.Message}");
                return new AmountUnblockPxResponse
                {

                };
            }
        }



        public async Task<BalanceEnquiryPxResponse> BalanceEnquiry(BalanceEnquiryPxRequest request)
        {
            try
            {

                //Transform to momo request
                //Call momo
                //Tranform to Proxy response
                //Return to Proxy



                return new BalanceEnquiryPxResponse
                {

                };

            }
            catch (Exception Ex)
            {
                Log.Write("Inward.BalanceEnquiry", $"Err: {Ex.Message}");
                return new BalanceEnquiryPxResponse
                {

                };
            }
        }





        public async Task<string> CallBack(CallbackPxRequest request)
        {
            try
            {

                //Transform to momo request
                //Call momo
                //Tranform to Proxy response
                //Return to Proxy



                return "";

            }
            catch (Exception Ex)
            {
                Log.Write("Inward.CallBack", $"Err: {Ex.Message}");
                return "";
            }
        }



        public async Task<FundTransferPxResponse> DirectCredit(FundTransferPxRequest request)
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
                Log.Write("Inward.DirectCredit", $"Err: {Ex.Message}");
                return new FundTransferPxResponse
                {

                };
            }
        }




        public async Task<DirectDebitPxResponse> DirectDebit(DirectDebitPxRequest request)
        {
            try
            {

                //Transform to momo request
                //Call momo
                //Tranform to Proxy response
                //Return to Proxy



                return new DirectDebitPxResponse
                {

                };

            }
            catch (Exception Ex)
            {
                Log.Write("Inward.DirectDebit", $"Err: {Ex.Message}");
                return new DirectDebitPxResponse
                {

                };
            }
        }



        public async Task<DirectDebitAdvicePxResponse> DirectDebitAdvice(DirectDebitAdvicePxRequest request)
        {
            try
            {

                //Transform to momo request
                //Call momo
                //Tranform to Proxy response
                //Return to Proxy



                return new DirectDebitAdvicePxResponse
                {

                };

            }
            catch (Exception Ex)
            {
                Log.Write("Inward.DirectDebitAdvice", $"Err: {Ex.Message}");
                return new DirectDebitAdvicePxResponse
                {

                };
            }
        }




        public async Task<DirectCreditAdvicePxResponse> DirectCreditAdvice(DirectCreditAdvicePxRequest request)
        {
            try
            {

                //Transform to momo request
                //Call momo
                //Tranform to Proxy response
                //Return to Proxy



                return new DirectCreditAdvicePxResponse
                {

                };

            }
            catch (Exception Ex)
            {
                Log.Write("Inward.DirectCreditAdvice", $"Err: {Ex.Message}");
                return new DirectCreditAdvicePxResponse
                {

                };
            }
        }


        public async Task<FinancialInstitutionListPxResponse> FinancialInstitutionList(FinancialInstitutionListPxRequest request)
        {
            try
            {

                //Transform to momo request
                //Call momo
                //Tranform to Proxy response
                //Return to Proxy

                throw new NotImplementedException();

                return new FinancialInstitutionListPxResponse
                {

                };

            }
            catch (Exception Ex)
            {
                Log.Write("Inward.FinancialInstitutionList", $"Err: {Ex.Message}");
                return new FinancialInstitutionListPxResponse
                {

                };
            }
        }


        public async Task<MandateAdvicePxResponse> ManadateAdvice(MandateAdvicePxRequest request)
        {
            try
            {

                //Transform to momo request
                //Call momo
                //Tranform to Proxy response
                //Return to Proxy

                throw new NotImplementedException();

                return new MandateAdvicePxResponse
                {

                };

            }
            catch (Exception Ex)
            {
                Log.Write("Inward.ManadateAdvice", $"Err: {Ex.Message}");
                return new MandateAdvicePxResponse
                {

                };
            }
        }

















    }
}
