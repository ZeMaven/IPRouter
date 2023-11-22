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
    public class DirectDebit
    {
        public async Task<string> InvokeAsync(string Request)
        {
            var Log = new Log();
            var HttpService = new HttpService();
            try
            {
                Log.Write("NibssInward.DirectDebit", $"Request from Nibss Enc: {Request}");
                var Pgp = new Pgp();

                var Req = Pgp.Decrypt(Request).Value;
                Log.Write("NibssInward.DirectDebit", $"Request : {Req}");

                var ReqObj = (FTSingleDebitRequest)CoralPay.Serialization.Xml.DeSerialize(Req, new FTSingleDebitRequest());

                //call router

                DirectDebitPxRequest MomoReq = new DirectDebitPxRequest
                {
                    TransactionLocation = ReqObj.TransactionLocation,
                    TransactionFee = ReqObj.TransactionFee,
                    SessionId = ReqObj.SessionId,
                    PaymentReference = ReqObj.PaymentReference,
                    NameEnquiryRef = ReqObj.NameEnquiryRef,
                    MandateReferenceNumber = ReqObj.MandateReferenceNumber,
                    DestinationInstitutionCode = ReqObj.DestinationInstitutionCode,
                    DebitKycLevel = ReqObj.DebitKycLevel,
                    DebitBankVerificationNumber = ReqObj.DebitBankVerificationNumber,
                    DebitAccountNumber = ReqObj.DebitAccountNumber,
                    Amount = ReqObj.Amount,
                    BeneficiaryAccountName = ReqObj.BeneficiaryAccountName,
                    BeneficiaryAccountNumber = ReqObj.BeneficiaryAccountNumber,
                    BeneficiaryBankVerificationNumber = ReqObj.BeneficiaryBankVerificationNumber,
                    BeneficiaryKycLevel = ReqObj.BeneficiaryKycLevel,
                    ChannelCode = ReqObj.ChannelCode,
                    DebitAccountName = ReqObj.DebitAccountName,
                    Narration = ReqObj.Narration,
                };

                var RouterResp = await HttpService.Call(MomoReq, Operation.NameEnqury);

                FTSingleDebitResponse Resp = new FTSingleDebitResponse();

                if (RouterResp.ResponseHeader.ResponseCode != "00")
                {
                    Resp = new FTSingleDebitResponse
                    {
                        ResponseCode = "01",
                        Narration = Resp.Narration,
                        TransactionLocation = ReqObj.TransactionLocation,
                        PaymentReference = ReqObj.PaymentReference,

                    };
                }
                else
                {
                    var RouterRespObj = JsonConvert.DeserializeObject<DirectDebitPxResponse>(RouterResp.ResponseContent);
                    Resp = new FTSingleDebitResponse
                    {
                        ResponseCode = RouterRespObj.ResponseCode,
                        Narration = RouterRespObj.Narration,
                        Amount = RouterRespObj.Amount,
                        BeneficiaryAccountName = RouterRespObj.BeneficiaryAccountName,
                        BeneficiaryAccountNumber = RouterRespObj.BeneficiaryAccountNumber,
                        BeneficiaryBankVerificationNumber = RouterRespObj.BeneficiaryBankVerificationNumber,
                        BeneficiaryKycLevel = RouterRespObj.BeneficiaryKycLevel,
                        ChannelCode = RouterRespObj.ChannelCode,
                        DebitAccountName = RouterRespObj.DebitAccountName,
                        DebitAccountNumber = RouterRespObj.DebitAccountNumber,
                        DebitBankVerificationNumber = RouterRespObj.DebitBankVerificationNumber,
                        DebitKycLevel = RouterRespObj.DebitKycLevel,
                        DestinationInstitutionCode = RouterRespObj.DestinationInstitutionCode,
                        MandateReferenceNumber = RouterRespObj.MandateReferenceNumber,
                        NameEnquiryRef = RouterRespObj.NameEnquiryRef,
                        PaymentReference = RouterRespObj.PaymentReference,
                        SessionId = RouterRespObj.SessionId,
                        TransactionFee = RouterRespObj.TransactionFee,
                        TransactionLocation = RouterRespObj.TransactionLocation,


                    };
                }
                var RespJson = JsonConvert.SerializeObject(Resp);
                Log.Write("NibssInward.DirectDebit", $"Response to Nibss:  {RespJson}");
                var Enc = Pgp.Encrypt(RespJson).Value;
                Log.Write("NibssInward.DirectDebit", $"Response to Nibss Enc:  {Enc}");

                return Enc;
            }
            catch (Exception Ex)
            {
                Log.Write("NibssInward.DirectDebit", $"Err: {Ex.Message}");
                return null;
            }
        }
    }
}