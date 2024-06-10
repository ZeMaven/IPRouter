using ArcaProxy.Models.NameEnq;
using ArcaProxy.Models.TranQuery;
using ArcaProxy.Models.Transfer;
using Momo.Common.Actions;
using Momo.Common.Models;
using Momo.Common.Models.HttpService;
using System.Text.Json;

namespace ArcaProxy.Actions
{
    public interface IArcaOutward
    {
        Task<NameEnquiryPxResponse> NameEnquiry(NameEnquiryPxRequest Request);
        Task<TranQueryPxResponse> TransactionQuery(TranQueryPxRequest Request);
        Task<FundTransferPxResponse> Transfer(FundTransferPxRequest Request);
    }

    public class ArcaOutward : IArcaOutward
    {
        private readonly IConfiguration Config;
        private readonly ILog Log;
        private readonly ICommonUtilities Utilities;
        private readonly IHttpService HttpService;
        private readonly ITransposer Transposer;

        private readonly IBankCodes BankCodes;
        private string SvaCode;
        private string SourceBank;

        public ArcaOutward(IConfiguration config, ILog log, ICommonUtilities utilities, IHttpService httpService, ITransposer transposer, IBankCodes bankcodes)
        {
            Log = log;
            Config = config;
            Utilities = utilities;
            HttpService = httpService;
            Transposer = transposer;
            BankCodes = bankcodes;
            SvaCode = Config.GetSection("SvaCode").Value;
            SourceBank = Config.GetSection("SourceBank").Value;
        }




        public async Task<NameEnquiryPxResponse> NameEnquiry(NameEnquiryPxRequest Request)
        {
            string SessionId = string.Empty;

            try
            {

                var JsonStr = JsonSerializer.Serialize(Request);
                Log.Write("Arca.NameEnquiry", $"Request from Router: {JsonStr}");
                var Bank = BankCodes.GetBank(Request.DestinationBankCode);

                var TranId = Utilities.CreateTransactionId();
                SessionId = $"{SourceBank}{TranId}";


                var DestinationBank = BankCodes.GetBank(Request.DestinationBankCode);
                var ArcaRequest = new NameEnqRequest
                {
                    accountNumber = Request.AccountId,
                    institutionCode = DestinationBank.CbnCode,
                    svaCode = SvaCode
                };




                var JsonReq = JsonSerializer.Serialize(ArcaRequest);
                Log.Write("Arca.NameEnquiry", $"Request is a GET");
                Log.Write("Arca.NameEnquiry", $"Request to Arca: {JsonReq}");
                var ArcaResp = await HttpService.Call(ArcaRequest, Operation.NameEnqury);





                Log.Write("Arca.NameEnquiry", $"Response from Arca: {ArcaResp.ResponseContent}");
                if (ArcaResp.ResponseHeader.ResponseCode != "00")
                {
                    return new NameEnquiryPxResponse
                    {
                        SessionId = Request.TransactionId,
                        //TransactionId = Request.SessionId,
                        SourceBankCode = SourceBank,
                        ResponseCode = "09",

                    };
                }               

                var ArcaRespObj = JsonSerializer.Deserialize<Models.NameEnq.NameEnqResponse>(ArcaResp.ResponseContent);


                NameEnquiryPxResponse Resp = new()
                {
                    SessionId = Request.TransactionId,
                    TransactionId = Request.TransactionId,
                    ResponseCode = "00",
                    ResponseMessage = "Successful",
                    SourceBankCode = SourceBank,
                    AccountName = ArcaRespObj.accountName,
                    AccountNumber = Request.AccountId,
                    ChannelCode = 0,
                    DestinationBankCode = Request.DestinationBankCode,
                    Bvn = string.Empty,
                    KycLevel = string.Empty,
                    ProxySessionId = string.Empty,
                };

                JsonStr = JsonSerializer.Serialize(Resp);
                Log.Write("Arca.NameEnquiry", $"Response to Router: {JsonStr}");
                return Resp;


            }
            catch (Exception Ex)
            {
                Log.Write("Arca.NameEnquiry", $"Err: {Ex.Message}");
                return new NameEnquiryPxResponse
                {
                    SessionId = SessionId,
                    TransactionId = Request.TransactionId,
                    SourceBankCode = SourceBank,
                    ResponseCode = "97",
                };
            }
        }


        public async Task<TranQueryPxResponse> TransactionQuery(TranQueryPxRequest Request)
        {
            string SessionId = string.Empty;
            try
            {
                var JsonStr = JsonSerializer.Serialize(Request);
                Log.Write("Arca.TranQuery", $"Request from Router: {JsonStr}");
                var TheSourceBank = BankCodes.GetBank(SourceBank);

                var TranId = Utilities.CreateTransactionId();


                var ArcaRequest = new TranQueryRequest
                {
                    requestId = Request.SessionId,
                    svaCode = SvaCode
                };



                var JsonReq = JsonSerializer.Serialize(ArcaRequest);
                Log.Write("Arca.TranQuery", $"Request to Arca: {JsonReq}");
                var ArcaResp = await HttpService.Call(ArcaRequest, Operation.TranQuery);



                Log.Write("Arca.TranQuery", $"Response from Arca: {ArcaResp.ResponseContent}");
                if (ArcaResp.ResponseHeader.ResponseCode != "00")
                {
                    return new TranQueryPxResponse
                    {
                        SessionId = Request.SessionId,
                        //TransactionId = Request.SessionId,
                        SourceBankCode = TheSourceBank.NibssCode,
                        ResponseCode = "09",

                    };
                }

                Log.Write("Arca.TranQuery", $"Response from Arca: {ArcaResp.ResponseContent}");

                var ArcaRespObj = JsonSerializer.Deserialize<Models.TranQuery.TranQueryResponse>(ArcaResp.ResponseContent);

                var ResponseHeader = Transposer.Response(ArcaRespObj.status);
                TranQueryPxResponse Resp = new()
                {
                    SessionId = Request.SessionId,
                    TransactionId = Request.TransactionId,
                    ResponseCode = ResponseHeader.ResponseCode,
                    ResponseMessage = ResponseHeader.ResponseMessage,
                    SourceBankCode = TheSourceBank.NibssCode,
                    Amount = 0,
                    Narration = null,
                    BenefAccountName = null,
                    BenefAccountNumber = null,
                    BenfBankCode = null,
                    SourceAccountName = null,
                    SourceAccountNumber = null,
                    ProcessorTranId = ArcaRespObj.accountTransferId
                };

                JsonStr = JsonSerializer.Serialize(Resp);
                Log.Write("Arca.TranQuery", $"Response to Router: {JsonStr}");
                return Resp;
            }
            catch (Exception Ex)
            {
                Log.Write("Arca.TranQuery", $"Err: {Ex.Message}");
                return new TranQueryPxResponse
                {
                    SessionId = SessionId,
                    TransactionId = Request.TransactionId,
                    SourceBankCode = null,
                    ResponseCode = "97",
                };
            }
        }


        public async Task<FundTransferPxResponse> Transfer(FundTransferPxRequest Request)
        {

            var DestinationBank = BankCodes.GetBank(Request.DestinationBankCode);
            var TheSourceBank = BankCodes.GetBank(SourceBank);
            string SessionId = string.Empty;

            try
            {
                var JsonStr = JsonSerializer.Serialize(Request);
                Log.Write("Arca.FundTransfer", $"Request from Router: {JsonStr}");


                var TranId = Utilities.CreateTransactionId();

                SessionId = $"{SvaCode}-{DateTime.Now:yyyyMMddmmhhss}-{Request.PaymentRef}";

                var ArcaRequest = new TransferRequest
                {
                    requestId = SessionId,
                    accountName = Request.BenefAccountName,
                    accountNumber = Request.BenefAccountNumber,
                    amount = Request.Amount,
                    currency = "NGN",
                    institutionCode = DestinationBank.CbnCode,
                    narration = Request.Narration,
                    svaCode = SvaCode,
                    transferInitiatornName = Request.SourceAccountName,
                    transactionDate = DateTime.Now,
                };

                var JsonReq = JsonSerializer.Serialize(ArcaRequest);
                Log.Write("Arca.FundTransfer", $"Request to Arca: {JsonReq} |  SessionId: {SessionId}");
                var ArcaResp = await HttpService.Call(ArcaRequest, Operation.Transfer);

                Log.Write("Arca.FundTransfer", $"Response from Arca: {ArcaResp.ResponseContent}  |  SessionId: {SessionId}");
                if (ArcaResp.ResponseHeader.ResponseCode != "00")
                {
                    return new FundTransferPxResponse
                    {
                        SessionId = SessionId,
                        TransactionId = Request.TransactionId,
                        SourceBankCode = TheSourceBank.NibssCode,
                        ResponseCode = "09",
                    };
                }

                var ArcaRespObj = JsonSerializer.Deserialize<Models.Transfer.TransferResponse>(ArcaResp.ResponseContent);

                var ResponseHeader = Transposer.Response(ArcaRespObj.responseCode);
                FundTransferPxResponse Resp = new()
                {
                    SessionId = SessionId,
                    TransactionId = Request.TransactionId,
                    ResponseCode = ResponseHeader.ResponseCode,
                    ResponseMessage = ResponseHeader.ResponseMessage,
                    SourceBankCode = TheSourceBank.NibssCode,
                    BenefBvn = string.Empty,
                    BenefKycLevel = Request.BenefKycLevel,
                    BenefAccountName = Request.BenefAccountName,
                    Amount = 0,
                    BenefAccountNumber = Request.BenefAccountNumber,
                    ChannelCode = Request.ChannelCode,
                    BenfBankCode = Request.DestinationBankCode,
                    NameEnquiryRef = Request.NameEnquiryRef,
                    Narration = Request.Narration,
                    SourceAccountName = Request.SourceAccountName,
                    SourceAccountNumber = Request.SourceAccountNumber,
                    TransactionDate = DateTime.Now,
                    ProcessorTranId = ArcaRespObj.accountTransferId
                };

                JsonStr = JsonSerializer.Serialize(Resp);
                Log.Write("Arca.FundTransfer", $"Response to Router: {JsonStr}");
                return Resp;

            }
            catch (Exception Ex)
            {
                Log.Write("Arca.FundTransfer", $"Err: {Ex.Message}");
                return new FundTransferPxResponse
                {
                    SessionId = SessionId,
                    TransactionId = Request.TransactionId,
                    SourceBankCode = SourceBank,
                    ResponseCode = "97",
                };
            }
        }
    }
}
