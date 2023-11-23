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
    public class AmountUnblock
    {
        public async Task<string> InvokeAsync(string Request)
        {
            var Log = new Log();
            var HttpService = new HttpService();
            try
            {
                Log.Write("NibssInward.AmountUnblock", $"Request from Nibss Enc: {Request}");
                var Pgp = new Pgp();

                var Req = Pgp.Decrypt(Request).Value;
                Log.Write("NibssInward.AmountUnblock", $"Request : {Req}");

                var ReqObj = (AmountUnblockRequest)CoralPay.Serialization.Xml.DeSerialize(Req, new AmountUnblockRequest());

                //call router

                AmountUnblockPxRequest MomoReq = new AmountUnblockPxRequest
                {
                    TargetBankVerificationNumber = ReqObj.TargetBankVerificationNumber,
                    SessionID = ReqObj.SessionID,
                    TargetAccountNumber = ReqObj.TargetAccountNumber,
                    DestinationInstitutionCode = ReqObj.DestinationInstitutionCode,
                    TargetAccountName = ReqObj.TargetAccountName,
                    Amount = ReqObj.Amount,
                    ReasonCode = ReqObj.ReasonCode,
                    ChannelCode = ReqObj.ChannelCode,
                    ReferenceCode = ReqObj.ReferenceCode,
                    Narration = ReqObj.Narration,
                };

                var RouterResp = await HttpService.Call(MomoReq, Operation.NameEnqury);

                AmountUnblockResponse Resp = new AmountUnblockResponse();

                if (RouterResp.ResponseHeader.ResponseCode != "00")
                {
                    Resp = new AmountUnblockResponse
                    {
                        ResponseCode = "01",
                        TargetBankVerificationNumber = MomoReq.TargetBankVerificationNumber,
                        TargetAccountNumber = MomoReq.TargetAccountNumber,
                        TargetAccountName = MomoReq.TargetAccountName,
                        ReferenceCode = MomoReq.ReferenceCode,
                        ReasonCode = MomoReq.ReasonCode,
                        Amount = MomoReq.Amount,
                        DestinationInstitutionCode = MomoReq.DestinationInstitutionCode,
                        Narration = MomoReq.Narration,
                        SessionID = ReqObj.SessionID,
                        ChannelCode = ReqObj.ChannelCode
                    };
                }
                else
                {
                    var RouterRespObj = JsonConvert.DeserializeObject<AmountUnblockPxResponse>(RouterResp.ResponseContent);
                    Resp = new AmountUnblockResponse
                    {
                        ResponseCode = RouterRespObj.ResponseCode,
                        TargetBankVerificationNumber = RouterRespObj.TargetBankVerificationNumber,
                        TargetAccountNumber = RouterRespObj.TargetAccountNumber,
                        TargetAccountName = RouterRespObj.TargetAccountName,
                        ReferenceCode = RouterRespObj.ReferenceCode,
                        ReasonCode = RouterRespObj.ReasonCode,
                        Amount = RouterRespObj.Amount,
                        DestinationInstitutionCode = RouterRespObj.DestinationInstitutionCode,
                        Narration = RouterRespObj.Narration,
                        SessionID = ReqObj.SessionID,
                        ChannelCode = ReqObj.ChannelCode
                    };
                }
                var RespJson = JsonConvert.SerializeObject(Resp);
                Log.Write("NibssInward.AmountUnblock", $"Response to Nibss:  {RespJson}");
                var Enc = Pgp.Encrypt(RespJson).Value;
                Log.Write("NibssInward.AmountUnblock", $"Response to Nibss Enc:  {Enc}");

                return Enc;
            }
            catch (Exception Ex)
            {
                Log.Write("NibssInward.AmountUnblock", $"Err: {Ex.Message}");
                return null;
            }
        }
    }
}