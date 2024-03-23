using CipProxy.Models.Cip.NameEnq;
using CipProxy.Models.Cip.TranQuery;
using CipProxy.Models.Cip.Transfer;
using Momo.Common.Actions;
using Momo.Common.Models;
using Newtonsoft.Json;

namespace CipProxy.Actions
{
    public interface ICipInward
    {
        Task<string> NameEnquiry(string Request);
        Task<string> TransactionQuery(string Request);
        Task<string> Transfer(string Request);
    }

    public class CipInward : ICipInward
    {
        private readonly IConfiguration Config;
        private readonly ILog Log;
        private readonly ICommonUtilities Utilities;
        private readonly IHttpService HttpService;
        private readonly IPgp Pgp;

        public CipInward(IConfiguration config, ILog log, ICommonUtilities utilities, IHttpService httpService, IPgp pgp)
        {
            Log = log;
            Config = config;
            Utilities = utilities;
            HttpService = httpService;
            Pgp = pgp;
        }



        public async Task<string> NameEnquiry(string Request)
        {
            try
            {
                Log.Write("CipInward.NameEnquiry", $"Request from Cip Enc: {Request}");

                var ReqJson = Pgp.Decryption(Request);
                var SourceBank = Config.GetSection("SourceBank").Value;
                Log.Write("CipInward.NameEnquiry", $"Request from Cip: {ReqJson}");
                var ReqObj = JsonConvert.DeserializeObject<NameEnqRequest>(ReqJson);

                NameEnquiryPxRequest MomoReq = new()
                {
                    AccountId = ReqObj.accountId,
                    DestinationBankCode = ReqObj.destinationInstitutionId,
                    SourceBankCode = ReqObj.sessionId.Substring(0, 6),
                };

                var RouterResp = await HttpService.Call(MomoReq, Operation.NameEnqury);

                NameEnqResponse Resp = new();

                if (RouterResp.ResponseHeader.ResponseCode != "00")
                {
                    Resp = new NameEnqResponse
                    {
                        responseCode = "01",
                        responseMessage = "System challenge",
                        accountId = ReqObj.accountId,
                        sessionId = ReqObj.sessionId,
                    };
                }
                else
                {
                    var RouterRespObj = JsonConvert.DeserializeObject<NameEnquiryPxResponse>(RouterResp.ResponseContent);
                    Resp = new NameEnqResponse
                    {
                        responseCode = RouterRespObj.ResponseCode,
                        responseMessage = RouterRespObj.ResponseMessage,
                        accountId = RouterRespObj.AccountNumber,
                        sessionId = ReqObj.sessionId,
                        accountName = RouterRespObj.AccountName,
                        bvn = RouterRespObj.Bvn,
                        destinationInstitutionId = RouterRespObj.DestinationBankCode,
                        kycLevel = RouterRespObj.KycLevel,
                    };
                }
                var RespJson = JsonConvert.SerializeObject(Resp);
                Log.Write("CipInward.NameEnquiry", $"Response to Cip:  {RespJson}");
                var Enc = Pgp.Ecryption(RespJson);
                Log.Write("CipInward.NameEnquiry", $"Response to Cip Enc:  {Enc}");

                return Enc;
            }
            catch (Exception Ex)
            {
                Log.Write("CipInward.NameEnquiry", $"Err: {Ex.Message}");
                return null;
            }
        }




        public async Task<string> TransactionQuery(string Request)
        {
            try
            {
                Log.Write("CipInward.TransactionQuery", $"Request Enc: {Request}");

                var ReqJson = Pgp.Decryption(Request);
                var SourceBank = Config.GetSection("SourceBank").Value;
                Log.Write("CipInward.TransactionQuery", $"Request : {ReqJson}");
                var ReqObj = JsonConvert.DeserializeObject<TranQueryRequest>(ReqJson);

                TranQueryPxRequest MomoReq = new()
                {
                    SessionId = ReqObj.sessionId,
                };

                var RouterResp = await HttpService.Call(Request, Operation.NameEnqury);

                TranQueryResponse Resp = new();

                if (RouterResp.ResponseHeader.ResponseCode != "00")
                {
                    Resp = new TranQueryResponse
                    {
                        responseCode = "09",
                        responseMessage = "Transaction details not available now",
                        sessionId = ReqObj.sessionId,
                    };
                }
                else
                {
                    var RouterRespObj = JsonConvert.DeserializeObject<TranQueryPxResponse>(RouterResp.ResponseContent);
                    Resp = new TranQueryResponse
                    {
                        responseCode = RouterRespObj.ResponseCode,
                        responseMessage = RouterRespObj.ResponseMessage,
                        creditAccountName = RouterRespObj.BenefAccountName,
                        sessionId = ReqObj.sessionId,
                        amount = RouterRespObj.Amount,
                        creditAccount = RouterRespObj.BenefAccountNumber,
                        narration = RouterRespObj.Narration,
                        destinationInstitutionId = RouterRespObj.BenfBankCode,
                        paymentRef = RouterRespObj.TransactionId,
                        sourceAccountId = RouterRespObj.SourceAccountNumber,
                        sourceAccountName = RouterRespObj.SourceAccountName,
                        transactionDate = RouterRespObj.Date,
                        channel = null,
                        group = null,
                        sector = null,
                    };
                }
                var RespJson = JsonConvert.SerializeObject(Resp);
                Log.Write("CipInward.TransactionQuery", $"Response to Cip:  {RespJson}");
                var Enc = Pgp.Ecryption(RespJson);
                Log.Write("CipInward.TransactionQuery", $"Response to Cip Enc:  {Enc}");

                return Enc;
            }
            catch (Exception Ex)
            {
                Log.Write("CipInward.TransactionQuery", $"Err: {Ex.Message}");
                return null;
            }
        }






        public async Task<string> Transfer(string Request)
        {
            try
            {
                Log.Write("CipInward.Transfer", $"Request Enc: {Request}");

                var ReqJson = Pgp.Decryption(Request);
                var SourceBank = Config.GetSection("SourceBank").Value;
                Log.Write("CipInward.Transfer", $"Request : {ReqJson}");
                var ReqObj = JsonConvert.DeserializeObject<TransferRequest>(ReqJson);

                TranQueryPxRequest MomoReq = new()
                {
                    SessionId = ReqObj.sessionId,
                };

                var RouterResp = await HttpService.Call(Request, Operation.NameEnqury);

                TransferResponse Resp = new();

                if (RouterResp.ResponseHeader.ResponseCode != "00")
                {
                    Resp = new TransferResponse
                    {
                        responseCode = "09",
                        responseMessage = "Transaction details not available now",
                        sessionId = ReqObj.sessionId,
                    };
                }
                else
                {
                    var RouterRespObj = JsonConvert.DeserializeObject<FundTransferPxResponse>(RouterResp.ResponseContent);
                    Resp = new TransferResponse
                    {
                        responseCode = RouterRespObj.ResponseCode,
                        responseMessage = RouterRespObj.ResponseMessage,
                        creditAccountName = RouterRespObj.BenefAccountName,
                        sessionId = ReqObj.sessionId,
                        amount = RouterRespObj.Amount,
                        creditAccount = RouterRespObj.BenefAccountNumber,
                        narration = RouterRespObj.Narration,
                        destinationInstitutionId = RouterRespObj.BenfBankCode,
                        paymentRef = RouterRespObj.TransactionId,
                        sourceAccountId = RouterRespObj.SourceAccountNumber,
                        sourceAccountName = RouterRespObj.SourceAccountName,
                        transactionDate = RouterRespObj.TransactionDate,
                        channel = null,
                        group = null,
                        sector = null,
                    };
                }
                var RespJson = JsonConvert.SerializeObject(Resp);
                Log.Write("CipInward.Transfer", $"Response to Cip:  {RespJson}");
                var Enc = Pgp.Ecryption(RespJson);
                Log.Write("CipInward.Transfer", $"Response to Cip Enc:  {Enc}");

                return Enc;
            }
            catch (Exception Ex)
            {
                Log.Write("CipInward.Transfer", $"Err: {Ex.Message}");
                return null;
            }
        }
    }
}
