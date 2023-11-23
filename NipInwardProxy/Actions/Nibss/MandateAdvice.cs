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
    public class MandateAdvice
    {

        public async Task<string> InvokeAsync(string Request)
        {
            var Log = new Log();
            var HttpService = new HttpService();
            try
            {
                Log.Write("NibssInward.MandateAdvice", $"Request from Nibss Enc: {Request}");
                var Pgp = new Pgp();

                var Req = Pgp.Decrypt(Request).Value;
                Log.Write("NibssInward.MandateAdvice", $"Request : {Req}");

                var ReqObj = (MandateAdviceRequest)CoralPay.Serialization.Xml.DeSerialize(Req, new MandateAdviceRequest());

                //call router

                MandateAdvicePxRequest MomoReq = new MandateAdvicePxRequest
                {
                    ChannelCode = ReqObj.ChannelCode,
                    BeneficiaryKYCLevel = ReqObj.BeneficiaryKYCLevel,
                    BeneficiaryBankVerificationNumber = ReqObj.BeneficiaryBankVerificationNumber,
                    BeneficiaryAccountNumber = ReqObj.BeneficiaryAccountNumber,
                    BeneficiaryAccountName = ReqObj.BeneficiaryAccountName,
                    DebitAccountName = ReqObj.DebitAccountName,
                    DebitAccountNumber = ReqObj.DebitAccountNumber,
                    DebitBankVerificationNumber = ReqObj.DebitBankVerificationNumber,
                    DebitKYCLevel = ReqObj.DebitKYCLevel,
                    Amount = ReqObj.Amount,
                    DestinationInstitutionCode = ReqObj.DestinationInstitutionCode,
                    MandateReferenceNumber = ReqObj.MandateReferenceNumber,
                    SessionID = ReqObj.SessionID,
                };

                var RouterResp = await HttpService.Call(MomoReq, Operation.NameEnqury);

                MandateAdviceResponse Resp = new MandateAdviceResponse();

                if (RouterResp.ResponseHeader.ResponseCode != "00")
                {
                    Resp = new MandateAdviceResponse
                    {
                        ResponseCode = "01",
                        SessionID = ReqObj.SessionID,
                        ChannelCode = ReqObj.ChannelCode
                    };
                }
                else
                {
                    var RouterRespObj = JsonConvert.DeserializeObject<MandateAdvicePxResponse>(RouterResp.ResponseContent);
                    Resp = new MandateAdviceResponse
                    {
                        ResponseCode = RouterRespObj.ResponseCode,

                        Amount = RouterRespObj.Amount,
                        BeneficiaryAccountName = RouterRespObj.BeneficiaryAccountName,
                        BeneficiaryAccountNumber = RouterRespObj.BeneficiaryAccountNumber,
                        BeneficiaryBankVerificationNumber = RouterRespObj.BeneficiaryBankVerificationNumber,
                        BeneficiaryKYCLevel = RouterRespObj.BeneficiaryKYCLevel,
                        DestinationInstitutionCode = RouterRespObj.DestinationInstitutionCode,
                        SessionID = RouterRespObj.SessionID,
                        MandateReferenceNumber = RouterRespObj.MandateReferenceNumber,
                        ChannelCode = RouterRespObj.ChannelCode,
                        DebitAccountName = RouterRespObj.DebitAccountName,
                        DebitAccountNumber = RouterRespObj.DebitAccountNumber,
                        DebitBankVerificationNumber = RouterRespObj.DebitBankVerificationNumber,
                        DebitKYCLevel = RouterRespObj.DebitKYCLevel
                    };
                }
                var RespJson = JsonConvert.SerializeObject(Resp);
                Log.Write("NibssInward.MandateAdvice", $"Response to Nibss:  {RespJson}");
                var Enc = Pgp.Encrypt(RespJson).Value;
                Log.Write("NibssInward.MandateAdvice", $"Response to Nibss Enc:  {Enc}");

                return Enc;
            }
            catch (Exception Ex)
            {
                Log.Write("NibssInward.MandateAdvice", $"Err: {Ex.Message}");
                return null;
            }
        }
    }
}