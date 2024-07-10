using RemitaProxy.Models.NameEnq;
using RemitaProxy.Models.TranQuery;
using RemitaProxy.Models.Transfer;
using Momo.Common.Models.HttpService;
using Momo.Common.Actions;
using Momo.Common.Models;
using System.Text.Json;

namespace RemitaProxy.Actions
{
    public interface IRemitaOutward
    {
        Task<NameEnquiryPxResponse> NameEnquiry(NameEnquiryPxRequest Request);
        Task<TranQueryPxResponse> TransactionQuery(TranQueryPxRequest Request);
        Task<FundTransferPxResponse> Transfer(FundTransferPxRequest Request);
    }

    public class RemitaOutward : IRemitaOutward
    {

        private readonly IConfiguration Config;
        private readonly ILog Log;
        private readonly ICommonUtilities Utilities;
        private readonly IHttpService HttpService;
        private readonly ITransposer Transposer;
        private string TerminalId;
        private string Pin;
        private string SourceBank;

        public RemitaOutward(IConfiguration config, ILog log, ICommonUtilities utilities, IHttpService httpService, ITransposer transposer)
        {
            Log = log;
            Config = config;
            Utilities = utilities;
            HttpService = httpService;
            Transposer = transposer;
            SourceBank = Config.GetSection("SourceBank").Value;
        }




        public async Task<NameEnquiryPxResponse> NameEnquiry(NameEnquiryPxRequest Request)
        {
            string SessionId = string.Empty;

            try
            {
                var JsonStr = JsonSerializer.Serialize(Request);
                Log.Write("Remita.NameEnquiry", $"Request from Router: {JsonStr}");
                var Bank = Transposer.GetBank(Request.DestinationBankCode);

                var TranId = Utilities.CreateTransactionId();
                SessionId = $"{SourceBank}{TranId}";

                var CbnSourceBankCode = Transposer.GetBank(Request.SourceBankCode).CbnCode;
                var RemitaRequest = new NameEnqRequest
                {
                    sourceAccount = Request.AccountId,
                    sourceBankCode = CbnSourceBankCode
                };

                Log.Write("Remita.NameEnquiry", $"Request to Remita: See request below |  SessionId: {SessionId}");
                var RemitaResp = await HttpService.Call(RemitaRequest, Operation.NameEnqury);

                var JsonReq = JsonSerializer.Serialize(RemitaRequest);
                Log.Write("Remita.NameEnquiry", $"Request to Remita: {JsonReq}");

                Log.Write("Remita.NameEnquiry", $"Response from Remita Enc: {RemitaResp.ResponseContent}");
                if (RemitaResp.ResponseHeader.ResponseCode != "00")
                {
                    return new NameEnquiryPxResponse
                    {
                        SessionId = Request.TransactionId,
                        //TransactionId = Request.SessionId,
                        SourceBankCode = SourceBank,
                        ResponseCode = "09",
                    };
                }

                Log.Write("Remita.NameEnquiry", $"Response from Remita: {RemitaResp.ResponseContent}");

                var RemitaRespObj = JsonSerializer.Deserialize<Models.NameEnq.NameEnqResponse>(RemitaResp.ResponseContent)!;
                var ResponseHeader = Transposer.Response(RemitaRespObj.status);

                NameEnquiryPxResponse Resp = new()
                {
                    SessionId = SessionId,
                    TransactionId = Request.TransactionId,
                    ResponseCode = ResponseHeader.ResponseCode,
                    ResponseMessage = ResponseHeader.ResponseMessage,
                    SourceBankCode = SourceBank,
                    AccountName = RemitaRespObj.data!.sourceAccountName,
                    AccountNumber = RemitaRespObj.data.sourceAccount,
                    ChannelCode = 0,
                    DestinationBankCode = Request.DestinationBankCode,
                    Bvn = string.Empty,
                    KycLevel = string.Empty,
                    ProxySessionId = string.Empty
                };

                JsonStr = JsonSerializer.Serialize(Resp);
                Log.Write("Remita.NameEnquiry", $"Response to Router: {JsonStr}");
                return Resp;

            }
            catch (Exception Ex)
            {
                Log.Write("Remita.NameEnquiry", $"Err: {Ex.Message}");
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

            try
            {
                var JsonStr = JsonSerializer.Serialize(Request);
                Log.Write("Remita.TranQuery", $"Request from Router: {JsonStr}");


                var TranId = Utilities.CreateTransactionId();

                var RemitaRequest = new TranQueryRequest
                {
                    transactionRef = Request.SessionId,
                };
                var JsonReq = JsonSerializer.Serialize(RemitaRequest);
                Log.Write("Remita.TranQuery", $"Request to Remita: {JsonReq}");

                var RemitaResp = await HttpService.Call(RemitaRequest, Operation.TranQuery);

                Log.Write("Remita.TranQuery", $"Response from Remita: {RemitaResp.ResponseContent}");
                if (RemitaResp.ResponseHeader.ResponseCode != "00")
                {
                    return new TranQueryPxResponse
                    {
                        SessionId = Request.SessionId,
                        //TransactionId = Request.SessionId,
                        SourceBankCode = SourceBank,
                        ResponseCode = "09",

                    };
                }

                Log.Write("Remita.TranQuery", $"Response from Etranzact: {RemitaResp.ResponseContent}");

                var RemitaRespObj = JsonSerializer.Deserialize<Models.TranQuery.TranQueryResponse>(RemitaResp.ResponseContent);
                var ResponseHeader = Transposer.Response(RemitaRespObj.status);
                var DestinationBankCode = Transposer.GetBank(RemitaRespObj.data.destinationBankCode);
                TranQueryPxResponse Resp = new()
                {
                    SessionId = Request.SessionId,//same as tranref from Transfer request
                    TransactionId = Request.TransactionId,
                    ResponseCode = ResponseHeader.ResponseCode,
                    ResponseMessage = ResponseHeader.ResponseMessage,
                    SourceBankCode = SourceBank,
                    Amount = RemitaRespObj.data.amount,
                    Narration = null,
                    BenefAccountName = null,
                    BenefAccountNumber = null,
                    BenfBankCode = DestinationBankCode.NibssCode,
                    SourceAccountName = null,
                    SourceAccountNumber = null,
                    ProcessorTranId = RemitaRespObj.data.authorizationId
                };

                JsonStr = JsonSerializer.Serialize(Resp);
                Log.Write("Remita.TranQuery", $"Response to Router: {JsonStr}");
                return Resp;
            }
            catch (Exception Ex)
            {
                Log.Write("Remita.TranQuery", $"Err: {Ex.Message}");
                return new TranQueryPxResponse
                {
                    SessionId = Request.SessionId,
                    TransactionId = Request.TransactionId,
                    SourceBankCode = SourceBank,
                    ResponseCode = "97",
                };
            }
        }


        public async Task<FundTransferPxResponse> Transfer(FundTransferPxRequest Request)
        {           
            string SessionId = string.Empty;
            try
            {
                var JsonStr = JsonSerializer.Serialize(Request);
                Log.Write("Remita.FundTransfer", $"Request from Router: {JsonStr}");

                var TranId = Utilities.CreateTransactionId();
                SessionId = $"{SourceBank}{TranId}";

                var SourceBankCode = Transposer.GetBank(Request.SourceBankCode).CbnCode;
                var DestinationBankCode = Transposer.GetBank(Request.DestinationBankCode).CbnCode;
                var RemitaRequest = new TransferRequest
                {
                    customReference = Request.TransactionId,
                    currency = "NGN",
                    channel = "WEB",
                    transactionDescription = Request.Narration,
                    amount = Request.Amount,
                    transactionRef = SessionId,
                    destinationEmail = "",
                    sourceAccount = Request.SourceAccountNumber,
                    sourceAccountName = Request.SourceAccountName,
                    sourceBankCode = SourceBankCode,
                    destinationAccount = Request.BenefAccountNumber,
                    destinationAccountName = Request.SourceAccountName,
                    destinationBankCode = DestinationBankCode,
                    originalAccountNumber = "",
                    originalBankCode = ""
                };


                Log.Write("Remita.FundTransfer", $"Request to Remita: See request below |  SessionId: {SessionId}");
                var RemitaResp = await HttpService.Call(RemitaRequest, Operation.Transfer);

                var JsonReq = JsonSerializer.Serialize(RemitaRequest);

                Log.Write("Remita.FundTransfer", $"Request to Remita: {JsonReq} |  SessionId: {SessionId}");
                Log.Write("Remita.FundTransfer", $"Response from Remita: {RemitaResp.ResponseContent}  |  SessionId: {SessionId}");
                if (RemitaResp.ResponseHeader.ResponseCode != "00")
                {
                    return new FundTransferPxResponse
                    {
                        SessionId = SessionId,
                        TransactionId = Request.TransactionId,
                        SourceBankCode = SourceBank,
                        ResponseCode = "09",
                    };
                }

                var RemitaRespObj = JsonSerializer.Deserialize<Models.Transfer.TransferResponse>(RemitaResp.ResponseContent);

                var ResponseHeader = Transposer.Response(RemitaRespObj.status);

                FundTransferPxResponse Resp = new()
                {
                    SessionId = SessionId,
                    TransactionId = Request.TransactionId,
                    ResponseCode = ResponseHeader.ResponseCode,
                    ResponseMessage = ResponseHeader.ResponseMessage,
                    SourceBankCode = SourceBank,
                    BenefBvn = string.Empty,
                    BenefKycLevel = Request.BenefKycLevel,
                    BenefAccountName = Request.BenefAccountName,
                    Amount = RemitaRespObj.data.amount,
                    BenefAccountNumber = Request.BenefAccountNumber,
                    ChannelCode = Request.ChannelCode,
                    BenfBankCode = Request.DestinationBankCode,
                    NameEnquiryRef = Request.NameEnquiryRef,
                    Narration = Request.Narration,
                    SourceAccountName = Request.SourceAccountName,
                    SourceAccountNumber = Request.SourceAccountNumber,
                    ProcessorTranId = RemitaRespObj.data.authorizationId
                };

                JsonStr = JsonSerializer.Serialize(Resp);
                Log.Write("Remita.FundTransfer", $"Response to Router: {JsonStr}");
                return Resp;
            }
            catch (Exception Ex)
            {
                Log.Write("Remita.FundTransfer", $"Err: {Ex.Message}");
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
