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
    public class AccountBlock
    {
        public async Task<string> InvokeAsync(string Request)
        {
            var Log = new Log();
            var HttpService = new HttpService();
            try
            {
                Log.Write("NibssInward.AccountBlock", $"Request from Nibss Enc: {Request}");
                var Pgp = new Pgp();

                var Req = Pgp.Decrypt(Request).Value;
                Log.Write("NibssInward.AccountBlock", $"Request : {Req}");

                var ReqObj = (AccountBlockRequest)CoralPay.Serialization.Xml.DeSerialize(Req, new AccountBlockRequest());

                //call router

                AccountBlockPxRequest MomoReq = new AccountBlockPxRequest
                {
                    TargetBankVerificationNumber = ReqObj.TargetBankVerificationNumber,
                    SessionID = ReqObj.SessionID,
                    TargetAccountNumber = ReqObj.TargetAccountNumber,
                    DestinationInstitutionCode = ReqObj.DestinationInstitutionCode,
                    TargetAccountName = ReqObj.TargetAccountName,                   
                    ReasonCode = ReqObj.ReasonCode,
                    ChannelCode = ReqObj.ChannelCode,
                    ReferenceCode = ReqObj.ReferenceCode,
                    Narration = ReqObj.Narration,
                };

                var RouterResp = await HttpService.Call(MomoReq, Operation.NameEnqury);

                AccountBlockResponse Resp = new AccountBlockResponse();

                if (RouterResp.ResponseHeader.ResponseCode != "00")
                {
                    Resp = new AccountBlockResponse
                    {
                        ResponseCode = "01",
                        TargetBankVerificationNumber = MomoReq.TargetBankVerificationNumber,
                        TargetAccountNumber = MomoReq.TargetAccountNumber,
                        TargetAccountName = MomoReq.TargetAccountName,
                        ReferenceCode = MomoReq.ReferenceCode,
                        ReasonCode = MomoReq.ReasonCode,
                     
                        DestinationInstitutionCode = MomoReq.DestinationInstitutionCode,
                        Narration = MomoReq.Narration,
                        SessionID = ReqObj.SessionID,
                        ChannelCode = ReqObj.ChannelCode
                    };
                }
                else
                {
                    var RouterRespObj = JsonConvert.DeserializeObject<AccountBlockPxResponse>(RouterResp.ResponseContent);
                    Resp = new AccountBlockResponse
                    {
                        ResponseCode = RouterRespObj.ResponseCode,
                        TargetBankVerificationNumber = RouterRespObj.TargetBankVerificationNumber,
                        TargetAccountNumber = RouterRespObj.TargetAccountNumber,
                        TargetAccountName = RouterRespObj.TargetAccountName,
                        ReferenceCode = RouterRespObj.ReferenceCode,
                        ReasonCode = RouterRespObj.ReasonCode,                    
                        DestinationInstitutionCode = RouterRespObj.DestinationInstitutionCode,
                        Narration = RouterRespObj.Narration,
                        SessionID = RouterRespObj.SessionID,
                        ChannelCode = RouterRespObj.ChannelCode
                    };
                }
                var RespJson = JsonConvert.SerializeObject(Resp);
                Log.Write("NibssInward.AccountBlock", $"Response to Nibss:  {RespJson}");
                var Enc = Pgp.Encrypt(RespJson).Value;
                Log.Write("NibssInward.AccountBlock", $"Response to Nibss Enc:  {Enc}");

                return Enc;
            }
            catch (Exception Ex)
            {
                Log.Write("NibssInward.AccountBlock", $"Err: {Ex.Message}");
                return null;
            }
        }
    }
}