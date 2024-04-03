using CipProxy.Models.Cip.NameEnq;
using CipProxy.Models.Cip.TranQuery;
using CipProxy.Models.Cip.Transfer;
using Momo.Common.Actions;
using Momo.Common.Models;
using System.Text.Json;

namespace CipProxy.Actions
{


    public interface ICipOutward
    {
        Task<NameEnquiryPxResponse> NameEnquiry(NameEnquiryPxRequest Request);
        Task<TranQueryPxResponse> TransactionQuery(TranQueryPxRequest Request);
        Task<FundTransferPxResponse> Transfer(FundTransferPxRequest Request);
    }

    public class CipOutward : ICipOutward
    {
        private readonly IConfiguration Config;
        private readonly ILog Log;
        private readonly ICommonUtilities Utilities;
        private readonly IHttpService HttpService;
        private readonly IPgp Pgp;

        public CipOutward(IConfiguration config, ILog log, ICommonUtilities utilities, IHttpService httpService, IPgp pgp)
        {
            Log = log;
            Config = config;
            Utilities = utilities;
            HttpService = httpService;
            Pgp = pgp;
        }


        public async Task<NameEnquiryPxResponse> NameEnquiry(NameEnquiryPxRequest Request)
        {
            string SessionId = string.Empty;
            string SourceBank = string.Empty;
            try
            {
                var JsonStr = JsonSerializer.Serialize(Request);
                Log.Write("CipOutward.NameEnquiry", $"Request from Router: {JsonStr}");


                var TranId = Utilities.CreateTransactionId();
                SourceBank = Config.GetSection("SourceBank").Value;
                SessionId = $"{SourceBank}{TranId}";

                var CipRequest = new NameEnqRequest
                {
                    accountId = Request.AccountId,
                    sessionId = SessionId,
                    destinationInstitutionId = Request.DestinationBankCode
                };

                var JsonReq = JsonSerializer.Serialize(CipRequest);
                Log.Write("Cip.NameEnquiry", $"Request to Cip: {JsonReq}");

                var Enc = Pgp.Ecryption(JsonReq);
                Log.Write("Cip.NameEnquiry", $"Request to Cip Enc: {Enc}");

                var CipResp = await HttpService.Call(Enc, Operation.NameEnqury);

                Log.Write("Cip.NameEnquiry", $"Response from Cip Enc: {CipResp.ResponseContent}");
                if (CipResp.ResponseHeader.ResponseCode != "00")
                {
                    return new NameEnquiryPxResponse
                    {
                        ChannelCode = Request.ChannelCode,
                        SessionId = SessionId,
                        TransactionId = Request.TransactionId,
                        SourceBankCode = SourceBank,
                        ResponseCode = "01",
                    };
                }

                var CipPlainResp = Pgp.Decryption(CipResp.ResponseContent);
                Log.Write("Cip.NameEnquiry", $"Response from Cip: {CipPlainResp}");
                if (string.IsNullOrEmpty(CipPlainResp)) throw new Exception("Decryption failure");
                var CipRespObj = JsonSerializer.Deserialize<Models.Cip.NameEnq.NameEnqResponse>(CipPlainResp);

                NameEnquiryPxResponse Resp = new()
                {
                    SessionId = SessionId,
                    TransactionId = Request.TransactionId,
                    AccountName = CipRespObj.accountName,
                    AccountNumber = CipRespObj.accountId,
                    Bvn = CipRespObj.bvn,
                    DestinationBankCode = CipRespObj.destinationInstitutionId,
                    ResponseCode = CipRespObj.responseCode,
                    KycLevel = CipRespObj.kycLevel,
                    SourceBankCode = SourceBank,
                    ProxySessionId = SessionId,
                    ResponseMessage = CipRespObj.responseMessage,
                    ChannelCode = Request.ChannelCode
                };

                JsonStr = JsonSerializer.Serialize(Resp);
                Log.Write("Cip.NameEnquiry", $"Response to Router: {JsonStr}");
                return Resp;
            }
            catch (Exception Ex)
            {
                Log.Write("Cip.NameEnquiry", $"Err: {Ex.Message}");
                return new NameEnquiryPxResponse
                {
                    ChannelCode = Request.ChannelCode,
                    SessionId = SessionId,
                    TransactionId = Request.TransactionId,
                    SourceBankCode = SourceBank,
                    ResponseCode = "01",
                    ProxySessionId= SessionId
                };
            }
        }


        public async Task<TranQueryPxResponse> TransactionQuery(TranQueryPxRequest Request)
        {
            string SessionId = string.Empty;
            string SourceBank = string.Empty;
            try
            {
                var JsonStr = JsonSerializer.Serialize(Request);
                Log.Write("Cip.TranQuery", $"Request from Router: {JsonStr}");


                var TranId = Utilities.CreateTransactionId();
                SourceBank = Config.GetSection("SourceBank").Value;
                SessionId = $"{SourceBank}{TranId}";

                var CipRequest = new TranQueryRequest
                { 
                    sessionId = Request.SessionId,
                };

                var JsonReq = JsonSerializer.Serialize(CipRequest);
                Log.Write("Cip.TranQuery", $"Request to Cip: {JsonReq}");

                var Enc = Pgp.Ecryption(JsonReq);
                Log.Write("Cip.TranQuery", $"Request to Cip Enc: {Enc}");

                var CipResp = await HttpService.Call(Enc, Operation.TranQuery);

                Log.Write("Cip.TranQuery", $"Response from Cip Enc: {CipResp.ResponseContent}");
                if (CipResp.ResponseHeader.ResponseCode != "00")
                {
                    return new TranQueryPxResponse
                    {
                        SessionId = Request.SessionId,
                        //TransactionId = Request.SessionId,
                        SourceBankCode = SourceBank,
                        ResponseCode = "09",

                    };
                }

                var CipPlainResp = Pgp.Decryption(CipResp.ResponseContent);
                Log.Write("Cip.TranQuery", $"Response from Cip: {CipPlainResp}");
                if (string.IsNullOrEmpty(CipPlainResp)) throw new Exception("Decryption failure");
                var CipRespObj = JsonSerializer.Deserialize<Models.Cip.TranQuery.TranQueryResponse>(CipPlainResp);

                TranQueryPxResponse Resp = new()
                {
                    SessionId = Request.SessionId,
                    TransactionId = Request.SessionId,
                    ResponseCode = CipRespObj.responseCode,
                    ResponseMessage = CipRespObj.responseMessage,
                    SourceBankCode = SourceBank,
                    Amount = CipRespObj.amount,
                    Narration = CipRespObj.narration,
                    BenefAccountName = CipRespObj.creditAccountName,
                    BenefAccountNumber = CipRespObj.creditAccountName,
                    BenfBankCode = CipRespObj.destinationInstitutionId,
                    SourceAccountName = CipRespObj.sourceAccountName,
                    SourceAccountNumber = CipRespObj.sourceAccountId,

                };

                JsonStr = JsonSerializer.Serialize(Resp);
                Log.Write("Cip. TranQuery", $"Response to Router: {JsonStr}");
                return Resp;
            }
            catch (Exception Ex)
            {
                Log.Write("Cip.TranQuery", $"Err: {Ex.Message}");
                return new TranQueryPxResponse
                {
                    SessionId = SessionId,
                    TransactionId = Request.SessionId,
                    SourceBankCode = SourceBank,
                    ResponseCode = "09",
                };
            }
        }



        public async Task<FundTransferPxResponse> Transfer(FundTransferPxRequest Request)
        {
            string SessionId = string.Empty;
            string SourceBank = string.Empty;
            try
            {
                var JsonStr = JsonSerializer.Serialize(Request);
                Log.Write("Cip.FundTransfer", $"Request from Router: {JsonStr}");


                var TranId = Utilities.CreateTransactionId();
                SourceBank = Config.GetSection("SourceBank").Value;
                SessionId = $"{SourceBank}{TranId}";

                var CipRequest = new TransferRequest
                {
                    sessionId = SessionId,
                    amount = Request.Amount,
                    creditAccount = Request.BenefAccountNumber,
                    creditAccountName = Request.BenefAccountName,
                    sourceAccountId = Request.SourceAccountNumber,
                    destinationInstitutionId = Request.DestinationBankCode,
                    sourceAccountName = Request.SourceAccountName,
                    paymentRef = Request.TransactionId,
                    narration = Request.Narration,
                    channel = "WEB",
                    group = "0",
                    sector = "0", 
                };

                var JsonReq = JsonSerializer.Serialize(CipRequest);
                Log.Write("Cip.FundTransfer", $"Request to Cip: {JsonReq}");

                var Enc = Pgp.Ecryption(JsonReq);
                Log.Write("Cip.FundTransfer", $"Request to Cip Enc: {Enc}");

                var CipResp = await HttpService.Call(Enc, Operation.Transfer);

                Log.Write("Cip. FundTransfer", $"Response from Cip Enc: {CipResp.ResponseContent}");
                if (CipResp.ResponseHeader.ResponseCode != "00")
                {
                    return new FundTransferPxResponse
                    {
                        SessionId = SessionId,
                        TransactionId = Request.TransactionId,
                        SourceBankCode = SourceBank,
                        ResponseCode = "09",
                    };
                }

                var CipPlainResp = Pgp.Decryption(CipResp.ResponseContent);
                Log.Write("Cip.FundTransfer", $"Response from Cip: {CipPlainResp}");
                if (string.IsNullOrEmpty(CipPlainResp)) throw new Exception("Decryption failure");
                var CipRespObj = JsonSerializer.Deserialize<Models.Cip.Transfer.TransferResponse>(CipPlainResp);

                FundTransferPxResponse Resp = new()
                {
                    SessionId = SessionId,
                    TransactionId = Request.TransactionId,
                    ResponseCode = CipRespObj.responseCode,
                    ResponseMessage = CipRespObj.responseMessage,
                    SourceBankCode = SourceBank,
                    BenefBvn = string.Empty,
                    BenefKycLevel =Request.BenefKycLevel,
                    BenefAccountName = CipRespObj.creditAccountName,
                    Amount = CipRespObj.amount,
                    BenefAccountNumber = Request.BenefAccountNumber,
                    ChannelCode = Request.ChannelCode,
                    BenfBankCode = CipRespObj.destinationInstitutionId,
                    NameEnquiryRef = Request.NameEnquiryRef,
                    Narration = CipRespObj.narration,
                    SourceAccountName = CipRespObj.sourceAccountName,
                    SourceAccountNumber = Request.SourceAccountNumber,
                     
                     
                };

                JsonStr = JsonSerializer.Serialize(Resp);
                Log.Write("Cip.FundTransfer", $"Response to Router: {JsonStr}");
                return Resp;
            }
            catch (Exception Ex)
            {
                Log.Write("Cip.FundTransfer", $"Err: {Ex.Message}");
                return new FundTransferPxResponse
                {
                    SessionId = SessionId,
                    TransactionId = Request.TransactionId,
                    SourceBankCode = SourceBank,
                    ResponseCode = "09",
                };
            }
        }
    }
}