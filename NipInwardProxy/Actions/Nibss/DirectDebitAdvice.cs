using Newtonsoft.Json;
using NipInwardProxy.Models.Nibss;
using NipInwardProxy.Models.Router;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace NipInwardProxy.Actions.Nibss
{
    public class DirectDebitAdvice
    {

        public async Task<string> InvokeAsync(string Request)
        {
            var Log = new Log();
            var HttpService = new HttpService();
            try
            {
                Log.Write("NibssInward.DirectDebitAdvice", $"Request from Nibss Enc: {Request}");
                var Pgp = new Pgp();

                var Req = Pgp.Decrypt(Request).Value;
                Log.Write("NibssInward.DirectDebitAdvice", $"Request : {Req}");

                var ReqObj = (FTAdviceDebitRequest)CoralPay.Serialization.Xml.DeSerialize(Req, new FTAdviceDebitRequest());

                //call router

                DirectDebitAdvicePxRequest MomoReq = new DirectDebitAdvicePxRequest
                {
                    TransactionLocation = ReqObj.TransactionLocation,
                    TransactionFee = ReqObj.TransactionFee,
                    SessionID = ReqObj.SessionID,
                    PaymentReference = ReqObj.PaymentReference,
                    NameEnquiryRef = ReqObj.NameEnquiryRef,
                    MandateReferenceNumber = ReqObj.MandateReferenceNumber,
                    DestinationInstitutionCode = ReqObj.DestinationInstitutionCode,
                    BeneficiaryKYCLevel = ReqObj.BeneficiaryKYCLevel,
                    Amount = ReqObj.Amount,
                    BeneficiaryAccountName = ReqObj.BeneficiaryAccountName,
                    BeneficiaryAccountNumber = ReqObj.BeneficiaryAccountNumber,
                    BeneficiaryBankVerificationNumber = ReqObj.BeneficiaryBankVerificationNumber,
                    OriginatorKYCLevel = ReqObj.OriginatorKYCLevel,
                    ChannelCode = ReqObj.ChannelCode,
                    OriginatorAccountName = ReqObj.OriginatorAccountName,
                    OriginatorBankVerificationNumber = ReqObj.OriginatorBankVerificationNumber,
                    OriginatorAccountNumber = ReqObj.OriginatorAccountNumber,
                    Narration = ReqObj.Narration,
                };

                var RouterResp = await HttpService.Call(MomoReq, Operation.NameEnqury);

                FTAdviceCreditResponse Resp = new FTAdviceCreditResponse();

                if (RouterResp.ResponseHeader.ResponseCode != "00")
                {
                    Resp = new FTAdviceDebitResponse
                    {
                        ResponseCode = "01",
                        OriginatorAccountName = ReqObj.OriginatorAccountName,
                        Amount = ReqObj.Amount,
                        OriginatorBankVerificationNumber = ReqObj.OriginatorBankVerificationNumber,
                        OriginatorKYCLevel = ReqObj.OriginatorKYCLevel,
                        TransactionLocation = ReqObj.TransactionLocation,
                        PaymentReference = ReqObj.PaymentReference,
                        SessionID = ReqObj.SessionID,
                        ChannelCode = ReqObj.ChannelCode
                    };
                }
                else
                {
                    var RouterRespObj = JsonConvert.DeserializeObject<DirectDebitAdvicePxResponse>(RouterResp.ResponseContent);
                    Resp = new FTAdviceDebitResponse
                    {
                        ResponseCode = RouterRespObj.ResponseCode,
                        OriginatorAccountName = RouterRespObj.OriginatorAccountName,
                        Amount = RouterRespObj.Amount,
                        BeneficiaryAccountName = RouterRespObj.BeneficiaryAccountName,
                        BeneficiaryAccountNumber = RouterRespObj.BeneficiaryAccountNumber,
                        BeneficiaryBankVerificationNumber = RouterRespObj.BeneficiaryBankVerificationNumber,
                        BeneficiaryKYCLevel = RouterRespObj.BeneficiaryKYCLevel,
                        DestinationInstitutionCode = RouterRespObj.DestinationInstitutionCode,
                        NameEnquiryRef = RouterRespObj.NameEnquiryRef,
                        Narration = RouterRespObj.Narration,
                        OriginatorAccountNumber = RouterRespObj.OriginatorAccountNumber,
                        OriginatorBankVerificationNumber = ReqObj.OriginatorBankVerificationNumber,
                        OriginatorKYCLevel = ReqObj.OriginatorKYCLevel,
                        TransactionLocation = ReqObj.TransactionLocation,
                        PaymentReference = ReqObj.PaymentReference,
                        SessionID = ReqObj.SessionID,
                        TransactionFee = ReqObj.TransactionFee,
                        MandateReferenceNumber = ReqObj.MandateReferenceNumber,
                        ChannelCode = ReqObj.ChannelCode
                    };
                }
                var RespJson = JsonConvert.SerializeObject(Resp);
                Log.Write("NibssInward.DirectDebitAdvice", $"Response to Nibss:  {RespJson}");
                var Enc = Pgp.Encrypt(RespJson).Value;
                Log.Write("NibssInward.DirectDebitAdvice", $"Response to Nibss Enc:  {Enc}");

                return Enc;
            }
            catch (Exception Ex)
            {
                Log.Write("NibssInward.DirectDebitAdvice", $"Err: {Ex.Message}");
                return null;
            }
        }
    }
}