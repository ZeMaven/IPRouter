using EtransactProxy.Models.NameEnq;
using EtransactProxy.Models.TranQuery;
using EtransactProxy.Models.Transfer;
using Momo.Common.Models.HttpService;
using Momo.Common.Actions;
using Momo.Common.Models;
using System.Text.Json;


namespace EtransactProxy.Actions
{
    public interface IEntranzactOutward
    {
        Task<NameEnquiryPxResponse> NameEnquiry(NameEnquiryPxRequest Request);
        Task<TranQueryPxResponse> TransactionQuery(TranQueryPxRequest Request);
        Task<FundTransferPxResponse> Transfer(FundTransferPxRequest Request);
    }

    public class EtranzactOutward : IEntranzactOutward
    {

        private readonly IConfiguration Config;
        private readonly ILog Log;
        private readonly ICommonUtilities Utilities;
        private readonly IHttpService HttpService;
        private readonly ITransposer Transposer;
        private string TerminalId;
        private string Pin;
        private string SourceBank;

        public EtranzactOutward(IConfiguration config, ILog log, ICommonUtilities utilities, IHttpService httpService, ITransposer transposer)
        {
            Log = log;
            Config = config;
            Utilities = utilities;
            HttpService = httpService;
            Transposer = transposer;
            Pin = Config.GetSection("Pin").Value;
            TerminalId = Config.GetSection("TerminalId").Value;
            SourceBank = Config.GetSection("SourceBank").Value;
        }




        public async Task<NameEnquiryPxResponse> NameEnquiry(NameEnquiryPxRequest Request)
        {
            string SessionId = string.Empty;

            try
            {

                var JsonStr = JsonSerializer.Serialize(Request);
                Log.Write("Etranzact.NameEnquiry", $"Request from Router: {JsonStr}");
                var Bank = Transposer.GetBank(Request.DestinationBankCode);

                var TranId = Utilities.CreateTransactionId();
                SessionId = $"{SourceBank}{TranId}";

                var EtranzactRequest = new NameEnqRequest
                {
                    action = "AQ",
                    terminalId = TerminalId,
                    transaction = new()
                    {
                        description = "Account Query",
                        amount = 0m,
                        bankCode = Bank?.CbnCode,
                        destination = Request.AccountId,
                        endPoint = "A",
                        pin = Pin,
                        reference = Request.TransactionId,
                    }
                };




                Log.Write("Etranzact.NameEnquiry", $"Request to Etransact: See request below |  SessionId: {SessionId}");
                var EtranzactResp = await HttpService.Call(EtranzactRequest, Operation.NameEnqury);


                EtranzactRequest.transaction.pin = "*****";
                var JsonReq = JsonSerializer.Serialize(EtranzactRequest);
                Log.Write("Etranzact.NameEnquiry", $"Request to Etransact: {JsonReq}");

                Log.Write("Etranzact.NameEnquiry", $"Response from Etransact Enc: {EtranzactResp.ResponseContent}");
                if (EtranzactResp.ResponseHeader.ResponseCode != "00")
                {
                    return new NameEnquiryPxResponse
                    {
                        SessionId = Request.TransactionId,
                        //TransactionId = Request.SessionId,
                        SourceBankCode = SourceBank,
                        ResponseCode = "09",

                    };
                }

                Log.Write("Etranzact.NameEnquiry", $"Response from Etranzact: {EtranzactResp.ResponseContent}");

                var EtranzactRespObj = JsonSerializer.Deserialize<Models.NameEnq.NameEnqResponse>(EtranzactResp.ResponseContent);
                var ResponseHeader = Transposer.Response(EtranzactRespObj.error);

                NameEnquiryPxResponse Resp = new()
                {
                    SessionId = EtranzactRespObj.reference,
                    TransactionId = Request.TransactionId,
                    ResponseCode = ResponseHeader.ResponseCode,
                    ResponseMessage = ResponseHeader.ResponseMessage,
                    SourceBankCode = SourceBank,
                    AccountName = EtranzactRespObj.error == "0" ? EtranzactRespObj.message : string.Empty,
                    AccountNumber = Request.AccountId,
                    ChannelCode = 0,
                    DestinationBankCode = Request.DestinationBankCode,
                    Bvn = string.Empty,
                    KycLevel = string.Empty,
                    ProxySessionId = string.Empty
                };

                JsonStr = JsonSerializer.Serialize(Resp);
                Log.Write("Etranzact.NameEnquiry", $"Response to Router: {JsonStr}");
                return Resp;


            }
            catch (Exception Ex)
            {
                Log.Write("Etranzact.NameEnquiry", $"Err: {Ex.Message}");
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
                Log.Write("Etranzact.TranQuery", $"Request from Router: {JsonStr}");


                var TranId = Utilities.CreateTransactionId();
                SessionId = $"{SourceBank}{TranId}";

                var EtranzactRequest = new TranQueryRequest
                {
                    action = "TS",
                    terminalId = TerminalId,
                    transaction = new()
                    {
                        description = "Status check",
                        pin = Pin,
                        reference = Request.TransactionId,
                        lineType = "OTHERS"
                    }
                };



                Log.Write("Etranzact.TranQuery", $"Request to Etransact: See request below |  TransactionId: {Request.TransactionId}");
                var EtranzactResp = await HttpService.Call(EtranzactRequest, Operation.TranQuery);

                EtranzactRequest.transaction.pin = "*****";
                var JsonReq = JsonSerializer.Serialize(EtranzactRequest);
                Log.Write("Etranzact.TranQuery", $"Request to Etranzact: {JsonReq}");
                Log.Write("Etranzact.TranQuery", $"Response from Etranzact: {EtranzactResp.ResponseContent}");
                if (EtranzactResp.ResponseHeader.ResponseCode != "00")
                {
                    return new TranQueryPxResponse
                    {
                        SessionId = Request.SessionId,
                        //TransactionId = Request.SessionId,
                        SourceBankCode = SourceBank,
                        ResponseCode = "09",

                    };
                }

                Log.Write("Etranzact.TranQuery", $"Response from Etranzact: {EtranzactResp.ResponseContent}");

                var EtranzactRespObj = JsonSerializer.Deserialize<Models.TranQuery.TranQueryResponse>(EtranzactResp.ResponseContent);
                var ResponseHeader = Transposer.Response(EtranzactRespObj.error);

                TranQueryPxResponse Resp = new()
                {
                    SessionId = Request.SessionId,
                    TransactionId = Request.TransactionId,
                    ResponseCode = ResponseHeader.ResponseCode,
                    ResponseMessage = ResponseHeader.ResponseMessage,
                    SourceBankCode = SourceBank,
                    Amount = EtranzactRespObj.amount,
                    Narration = null,
                    BenefAccountName = null,
                    BenefAccountNumber = null,
                    BenfBankCode = null,
                    SourceAccountName = null,
                    SourceAccountNumber = null,
                    ProcessorTranId = EtranzactRespObj.reference
                };

                JsonStr = JsonSerializer.Serialize(Resp);
                Log.Write("Etranzact.TranQuery", $"Response to Router: {JsonStr}");
                return Resp;
            }
            catch (Exception Ex)
            {
                Log.Write("Etranzact.TranQuery", $"Err: {Ex.Message}");
                return new TranQueryPxResponse
                {
                    SessionId = SessionId,
                    TransactionId = Request.TransactionId,
                    SourceBankCode = SourceBank,
                    ResponseCode = "97",
                };
            }
        }


        public async Task<FundTransferPxResponse> Transfer(FundTransferPxRequest Request)
        {

            var Bank = Transposer.GetBank(Request.DestinationBankCode);

            string SessionId = string.Empty;

            try
            {
                var JsonStr = JsonSerializer.Serialize(Request);
                Log.Write("Etranzact.FundTransfer", $"Request from Router: {JsonStr}");


                var TranId = Utilities.CreateTransactionId();
                SessionId = $"{SourceBank}{TranId}";

                var EtranzactRequest = new TransferRequest
                {
                    action = "FT",
                    terminalId = TerminalId,
                    transaction = new Transaction
                    {
                        amount = Request.Amount,
                        bankCode = Bank?.CbnCode,
                        description = Request.Narration,
                        destination = Request.BenefAccountNumber,
                        senderName = $"{Request.SourceAccountName};{Request.SourceAccountNumber};{Request.BenefAccountName}",
                        reference = Request.TransactionId,
                        endPoint = "A",
                        pin = Pin
                    }
                };


                Log.Write("Etranzact.FundTransfer", $"Request to Etransact: See request below |  SessionId: {SessionId}");
                var EtransactResp = await HttpService.Call(EtranzactRequest, Operation.Transfer);

                EtranzactRequest.transaction.pin = "*****";
                var JsonReq = JsonSerializer.Serialize(EtranzactRequest);

                Log.Write("Etranzact.FundTransfer", $"Request to Etransact: {JsonReq} |  SessionId: {SessionId}");
                Log.Write("Etranzact.FundTransfer", $"Response from Etransact: {EtransactResp.ResponseContent}  |  SessionId: {SessionId}");
                if (EtransactResp.ResponseHeader.ResponseCode != "00")
                {
                    return new FundTransferPxResponse
                    {
                        SessionId = SessionId,
                        TransactionId = Request.TransactionId,
                        SourceBankCode = SourceBank,
                        ResponseCode = "09",
                    };
                }

                var EtransactRespObj = JsonSerializer.Deserialize<Models.Transfer.TransferResponse>(EtransactResp.ResponseContent);

                var ResponseHeader = Transposer.Response(EtransactRespObj.error);

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
                    Amount = EtransactRespObj.amount,
                    BenefAccountNumber = Request.BenefAccountNumber,
                    ChannelCode = Request.ChannelCode,
                    BenfBankCode = Request.DestinationBankCode,
                    NameEnquiryRef = Request.NameEnquiryRef,
                    Narration = Request.Narration,
                    SourceAccountName = Request.SourceAccountName,
                    SourceAccountNumber = Request.SourceAccountNumber,
                    ProcessorTranId = EtransactRespObj.reference
                };

                JsonStr = JsonSerializer.Serialize(Resp);
                Log.Write("Etranzact.FundTransfer", $"Response to Router: {JsonStr}");
                return Resp;

            }
            catch (Exception Ex)
            {
                Log.Write("Etranzact.FundTransfer", $"Err: {Ex.Message}");
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
