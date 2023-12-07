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
    public class BalanceEnquiry
    {


        public async Task<string> InvokeAsync(string Request)
        {
            var Log = new Log();
            var HttpService = new HttpService();
            try
            {
                Log.Write("NibssInward.BalanceEnquiry", $"Request from Nibss Enc: {Request}");
                var Pgp = new Pgp();

                var Req = Pgp.Decrypt(Request).Value;
                Log.Write("NibssInward.BalanceEnquiry", $"Request : {Req}");

                var ReqObj = (BalanceEnquiryRequest)CoralPay.Serialization.Xml.DeSerialize(Req, new BalanceEnquiryRequest());

                //call router

                BalanceEnquiryPxRequest MomoReq = new BalanceEnquiryPxRequest
                {
                    ChannelCode = ReqObj.ChannelCode,
                    AuthorizationCode = ReqObj.AuthorizationCode,
                    DestinationInstitutionCode = ReqObj.DestinationInstitutionCode,
                    SessionID = ReqObj.SessionID,
                    TargetAccountName = ReqObj.TargetAccountName,
                    TargetAccountNumber = ReqObj.TargetAccountNumber,
                    TargetBankVerificationNumber = ReqObj.TargetBankVerificationNumber
                };

                var RouterResp = await HttpService.Call(MomoReq, Operation.BalanceEnquiry);

                BalanceEnquiryResponse Resp = new BalanceEnquiryResponse();

                if (RouterResp.ResponseHeader.ResponseCode != "00")
                {
                    Resp = new BalanceEnquiryResponse
                    {
                        ResponseCode = "01",
                        SessionID = ReqObj.SessionID
                    };
                }
                else
                {
                    var RouterRespObj = JsonConvert.DeserializeObject<BalanceEnquiryPxResponse>(RouterResp.ResponseContent);
                    Resp = new BalanceEnquiryResponse
                    {
                        ResponseCode = RouterRespObj.ResponseCode,
                        AuthorizationCode = RouterRespObj.AuthorizationCode,
                        AvailableBalance = RouterRespObj.AvailableBalance,
                        ChannelCode = RouterRespObj.ChannelCode,
                        DestinationInstitutionCode = RouterRespObj.DestinationInstitutionCode,
                        SessionID = RouterRespObj.SessionID,
                        TargetBankVerificationNumber = RouterRespObj.TargetBankVerificationNumber,
                        TargetAccountNumber = RouterRespObj.TargetAccountNumber,
                        TargetAccountName = RouterRespObj.TargetAccountName
                    };
                }
                var RespJson = JsonConvert.SerializeObject(Resp);
                Log.Write("NibssInward.BalanceEnquiry", $"Response to Nibss:  {RespJson}");
                var Enc = Pgp.Encrypt(RespJson).Value;
                Log.Write("NibssInward.BalanceEnquiry", $"Response to Nibss Enc:  {Enc}");

                return Enc;
            }
            catch (Exception Ex)
            {
                Log.Write("NibssInward.BalanceEnquiry", $"Err: {Ex.Message}");
                return null;
            }
        }
    }
}