using Momo.Common.Models;
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
    public class DirectCreditAdvice
    {
        public async Task<string> InvokeAsync(string Request)
        {
            var Log = new Log();
            var HttpService = new HttpService();
            try
            {
                Log.Write("NibssInward.DirectCreditAdvice", $"Request from Nibss Enc: {Request}");
                var Pgp = new Pgp();

                var Req = Pgp.Decrypt(Request).Value;
                Log.Write("NibssInward.DirectCreditAdvice", $"Request : {Req}");

                var ReqObj = (FTAdviceCreditRequest)CoralPay.Serialization.Xml.DeSerialize(Req, new FTAdviceCreditRequest());

                //call router

                DirectCreditAdvicePxRequest MomoReq = new DirectCreditAdvicePxRequest
                {
                    OriginatorAccountNumber = ReqObj.OriginatorAccountNumber,
                    DestinationInstitutionCode = ReqObj.DestinationInstitutionCode,
                    BeneficiaryAccountName = ReqObj.BeneficiaryAccountName,
                    Amount = ReqObj.Amount,
                    BeneficiaryAccountNumber = ReqObj.BeneficiaryAccountNumber,
                    BeneficiaryBankVerificationNumber = ReqObj.BeneficiaryBankVerificationNumber,
                    BeneficiaryKYCLevel = ReqObj.BeneficiaryKYCLevel,
                    OriginatorAccountName = ReqObj.OriginatorAccountName,
                    ChannelCode = ReqObj.ChannelCode,
                    MandateReferenceNumber = ReqObj.MandateReferenceNumber,
                    NameEnquiryRef = ReqObj.NameEnquiryRef,
                    Narration = ReqObj.Narration,
                    OriginatorBankVerificationNumber = ReqObj.OriginatorBankVerificationNumber,
                    OriginatorKYCLevel = ReqObj.OriginatorKYCLevel,
                    PaymentReference = ReqObj.PaymentReference,
                    SessionID = ReqObj.SessionID,
                    TransactionLocation = ReqObj.TransactionLocation,
                     TransactionFee = ReqObj.TransactionFee,
                };

                var RouterResp = await HttpService.Call(MomoReq, Operation.DirectCreditAdvice);

                FTAdviceCreditResponse Resp = new FTAdviceCreditResponse();

                if (RouterResp.ResponseHeader.ResponseCode != "00")
                {
                    Resp = new FTAdviceCreditResponse
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
                    var RouterRespObj = JsonConvert.DeserializeObject<FundTransferPxResponse>(RouterResp.ResponseContent);
                    Resp = new FTAdviceCreditResponse
                    {
                        ResponseCode = RouterRespObj.ResponseCode,
                        OriginatorAccountName = RouterRespObj.SourceAccountName,
                        Amount = RouterRespObj.Amount,
                        BeneficiaryAccountName = RouterRespObj.BenefAccountName,
                        BeneficiaryAccountNumber = RouterRespObj.BenefAccountNumber,
                        BeneficiaryBankVerificationNumber = RouterRespObj.BenefBvn,
                        BeneficiaryKYCLevel = RouterRespObj.BenefKycLevel,
                        DestinationInstitutionCode = RouterRespObj.BenfBankCode,
                        NameEnquiryRef = RouterRespObj.NameEnquiryRef,
                        Narration = RouterRespObj.Narration,
                        OriginatorAccountNumber = RouterRespObj.SourceAccountNumber,
                        OriginatorBankVerificationNumber = ReqObj.OriginatorBankVerificationNumber,
                        OriginatorKYCLevel = ReqObj.OriginatorKYCLevel,
                        TransactionLocation = ReqObj.TransactionLocation,
                        PaymentReference = ReqObj.PaymentReference,
                        SessionID = ReqObj.SessionID,
                        ChannelCode = ReqObj.ChannelCode
                    };
                }
                var RespJson = JsonConvert.SerializeObject(Resp);
                Log.Write("NibssInward.DirectCreditAdvice", $"Response to Nibss:  {RespJson}");
                var Enc = Pgp.Encrypt(RespJson).Value;
                Log.Write("NibssInward.DirectCreditAdvice", $"Response to Nibss Enc:  {Enc}");

                return Enc;
            }
            catch (Exception Ex)
            {
                Log.Write("NibssInward.DirectCreditAdvice", $"Err: {Ex.Message}");
                return null;
            }
        }
    }
}