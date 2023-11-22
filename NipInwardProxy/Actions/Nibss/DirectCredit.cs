using Momo.Common.Models;
using Newtonsoft.Json;
using NipInwardProxy.Models.Nibss;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace NipInwardProxy.Actions.Nibss
{
    public class DirectCredit
    {

        public async Task<string> InvokeAsync(string Request)
        {
            var Log = new Log();
            var HttpService = new HttpService();
            try
            {
                Log.Write("NibssInward.DirectCredit", $"Request from Nibss Enc: {Request}");
                var Pgp = new Pgp();

                var Req = Pgp.Decrypt(Request).Value;
                Log.Write("NibssInward.DirectCredit", $"Request : {Req}");

                var ReqObj = (FTSingleCreditRequest)CoralPay.Serialization.Xml.DeSerialize(Req, new FTSingleCreditRequest());

                //call router

                FundTransferPxRequest MomoReq = new FundTransferPxRequest
                { 
                    TransactionId = ReqObj.SessionID,
                    BenefBvn = ReqObj.BeneficiaryBankVerificationNumber,
                    Amount = ReqObj.Amount,
                    BenefAccountName = ReqObj.BeneficiaryAccountName,
                    BenefAccountNumber = ReqObj.BeneficiaryAccountNumber,
                    BenefKycLevel = ReqObj.BeneficiaryKYCLevel,
                    DestinationBankCode = ReqObj.DestinationInstitutionCode,
                    NameEnquiryRef = ReqObj.NameEnquiryRef,
                    Narration = ReqObj.Narration,
                    SourceAccountName = ReqObj.OriginatorAccountName,
                    SourceAccountNumber = ReqObj.OriginatorAccountNumber,
                    SourceBankCode = Properties.Settings.Default.SourceBankCode
                };

                var RouterResp = await HttpService.Call(MomoReq, Operation.NameEnqury);

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
                    Resp = new FTSingleCreditResponse
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
                Log.Write("NibssInward.DirectCredit", $"Response to Nibss:  {RespJson}");
                var Enc = Pgp.Encrypt(RespJson).Value;
                Log.Write("NibssInward.DirectCredit", $"Response to Nibss Enc:  {Enc}");

                return Enc;
            }
            catch (Exception Ex)
            {
                Log.Write("NibssInward.DirectCredit", $"Err: {Ex.Message}");
                return null;
            }
        }
    }
}