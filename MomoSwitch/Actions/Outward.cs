using Momo.Common.Models;
using MomoSwitch.Models.Contracts.Momo;
using System.Text.Json;

namespace MomoSwitch.Actions
{
    public interface IOutward
    {
        Task<NameEnquiryResponse> NameEnquiry(NameEnquiryRequest Req);
        Task<TranQueryResponse> TranQuery(TranQueryRequest Req);
        Task<FundTransferResponse> Transfer(FundTransferRequest Req);
    }

    public class Outward : IOutward
    {
        private readonly ISwitchRouter SwitchRouter;
        private readonly ITransposer Transposer;
        private readonly IHttpService HttpService;
        private readonly ILog Log;
        public Outward(ISwitchRouter router, ILog log, ITransposer transposer, IHttpService httpService)
        {
            SwitchRouter = router;
            Transposer = transposer;
            Log = log;
            HttpService = httpService;
        }


        public async Task<NameEnquiryResponse> NameEnquiry(NameEnquiryRequest Req)
        {
            NameEnquiryResponse Resp;
            try
            {  //Review

                string JsonStr = JsonSerializer.Serialize(Req);
                Log.Write("Outward.NameEnqury", $"Request: {JsonStr}");
                var RouterDetail = SwitchRouter.Route(new Models.RouterRequest
                {
                    Amount = 0,
                    BankCode = Req.destinationInstitutionCode,
                    Date = DateTime.Now,
                });

                var ProcessorRequest = Transposer.ToProxyNameEnquiryRequest(Req);

                var ProcessorResp = await HttpService.Call(new Models.Internals.HttpService.HttpServiceRequest
                {
                    EndPoint = RouterDetail.NameEnquiryUrl,
                    Method = Models.Internals.HttpService.Method.Post,
                    RequestObject = ProcessorRequest
                });

                if (ProcessorResp.ResponseHeader.ResponseCode == Models.Internals.HttpService.HttpServiceStatus.Success)
                {
                    NameEnquiryPxResponse ProcessorRespObj = JsonSerializer.Deserialize<NameEnquiryPxResponse>(ProcessorResp.Object.ToString());

                    Resp = Transposer.ToMomoNameEnquiryResponse(ProcessorRespObj);

                    JsonStr = JsonSerializer.Serialize(Resp);
                    Log.Write("Outward.NameEnqury", $"Response: {JsonStr}");
                }
                else
                {
                    Resp = new NameEnquiryResponse
                    {
                        responseCode = "01",
                        responseMessage = "System challenge",
                        accountNumber = Req.accountNumber,
                        transactionId = Req.transactionId,
                    };
                    JsonStr = JsonSerializer.Serialize(Resp);
                    Log.Write("Outward.NameEnqury", $"Response: {JsonStr}");

                }
                return Resp;
            }
            catch (Exception Ex)
            {
                Log.Write("Outward.NameEnqury", $"Err: {Ex.Message}");
                return new NameEnquiryResponse
                {
                    responseCode = "01",
                    responseMessage = "System challenge",
                    accountNumber = Req.accountNumber,
                    transactionId = Req.transactionId,
                };
            }
        }


        public async Task<TranQueryResponse> TranQuery(TranQueryRequest Req)
        {
            TranQueryResponse Resp;
            try
            {
                string JsonStr = JsonSerializer.Serialize(Req);
                Log.Write("Outward.TranQuery", $"Request: {JsonStr}");

                var Tran= GetTransaction(Req.transactionId);

                var RouterDetail = SwitchRouter.Route(new Models.RouterRequest
                {
                    Processor = ""// Pass processor
                });
                var ProcessorRequest = Transposer.ToProxyTranQueryyRequest(Req);


                var ProcessorResp = await HttpService.Call(new Models.Internals.HttpService.HttpServiceRequest
                {
                    EndPoint = RouterDetail.TranQueryUrl,
                    Method = Models.Internals.HttpService.Method.Post,
                    RequestObject = ProcessorRequest
                });

                if (ProcessorResp.ResponseHeader.ResponseCode == Models.Internals.HttpService.HttpServiceStatus.Success)
                {
                    TranQueryPxResponse ProcessorRespObj = JsonSerializer.Deserialize<TranQueryPxResponse>(ProcessorResp.Object.ToString());

                    Resp = Transposer.ToMomoTranQueryResponse(ProcessorRespObj);

                    JsonStr = JsonSerializer.Serialize(Resp);
                    Log.Write("Outward.TranQuery", $"Response: {JsonStr}");
                }
                else
                {
                    Resp = new TranQueryResponse
                    {
                        responseCode = "01",
                        responseMessage = "System challenge",
                        message = "System challenge",
                        transactionId = Req.transactionId,
                    };
                    JsonStr = JsonSerializer.Serialize(Resp);
                    Log.Write("Outward.TranQuery", $"Response: {JsonStr}");

                }
                return Resp;
            }
            catch (Exception Ex)
            {
                Log.Write("Outward.TranQuery", $"Err: {Ex.Message}");
                return new TranQueryResponse
                {
                    responseCode = "01",
                    responseMessage = "System challenge",
                    message = "System challenge",
                    transactionId = Req.transactionId,
                };
            }
        }

        private object GetTransaction(string transactionId)
        {
            throw new NotImplementedException();
        }

        public async Task<FundTransferResponse> Transfer(FundTransferRequest Req)
        {
            FundTransferResponse Resp;
            try
            {
                string JsonStr = JsonSerializer.Serialize(Req);
                Log.Write("Outward.Transfer", $"Request: {JsonStr}");
                var RouterDetail = SwitchRouter.Route(new Models.RouterRequest
                {
                    Amount = Req.amount,
                    BankCode = Req.destinationInstitutionCode,
                    Date = DateTime.Now,
                });
                var ProcessorRequest = Transposer.ToProxyFundTransferRequest(Req);

                var ProcessorResp = await HttpService.Call(new Models.Internals.HttpService.HttpServiceRequest
                {
                    EndPoint = RouterDetail.TransferUrl,
                    Method = Models.Internals.HttpService.Method.Post,
                    RequestObject = ProcessorRequest
                });

                if (ProcessorResp.ResponseHeader.ResponseCode == Models.Internals.HttpService.HttpServiceStatus.Success)
                {
                    FundTransferPxResponse ProcessorRespObj = JsonSerializer.Deserialize<FundTransferPxResponse>(ProcessorResp.Object.ToString());

                    Resp = Transposer.ToMomoFundTransferResponse(ProcessorRespObj);

                    JsonStr = JsonSerializer.Serialize(Resp);
                    Log.Write("Outward.Transfer", $"Response: {JsonStr}");
                }
                else
                {
                    Resp = new FundTransferResponse
                    {
                        responseCode = "01",// Change appropratly
                        amount = Req.amount,
                        beneficiaryAccountName = Req.beneficiaryAccountName,
                        beneficiaryAccountNumber = Req.beneficiaryAccountNumber,
                        beneficiaryBankVerificationNumber = Req.beneficiaryBankVerificationNumber,
                        beneficiaryKYCLevel = Req.beneficiaryKYCLevel,
                        channelCode = Req.channelCode,
                        debitAccountName = Req.originatorAccountName,
                        debitAccountNumber = Req.originatorAccountNumber,
                        debitBankVerificationNumber = Req.originatorBankVerificationNumber,
                        debitKYCLevel = Req.originatorKYCLevel,
                        destinationInstitutionCode = Req.destinationInstitutionCode,
                        mandateReferenceNumber = Req.mandateReferenceNumber,
                        nameEnquiryRef = Req.nameEnquiryRef,
                        narration = Req.originatorNarration,
                        paymentReference = Req.paymentReference,
                        sessionID = Req.transactionId,
                    };
                    JsonStr = JsonSerializer.Serialize(Resp);
                    Log.Write("Outward.Transfer", $"Response: {JsonStr}");

                }
                return Resp;
            }
            catch (Exception Ex)
            {
                Log.Write("Outward.Transfer", $"Err: {Ex.Message}");
                return new FundTransferResponse
                {
                    responseCode = "01",// Change appropratly
                    amount = Req.amount,
                    beneficiaryAccountName = Req.beneficiaryAccountName,
                    beneficiaryAccountNumber = Req.beneficiaryAccountNumber,
                    beneficiaryBankVerificationNumber = Req.beneficiaryBankVerificationNumber,
                    beneficiaryKYCLevel = Req.beneficiaryKYCLevel,
                    channelCode = Req.channelCode,
                    debitAccountName = Req.originatorAccountName,
                    debitAccountNumber = Req.originatorAccountNumber,
                    debitBankVerificationNumber = Req.originatorBankVerificationNumber,
                    debitKYCLevel = Req.originatorKYCLevel,
                    destinationInstitutionCode = Req.destinationInstitutionCode,
                    mandateReferenceNumber = Req.mandateReferenceNumber,
                    nameEnquiryRef = Req.nameEnquiryRef,
                    narration = Req.originatorNarration,
                    paymentReference = Req.paymentReference,
                    sessionID = Req.transactionId,
                };
            }
        }
    }
}