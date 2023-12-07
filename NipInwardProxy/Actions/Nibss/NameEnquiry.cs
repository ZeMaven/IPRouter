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
    public class NameEnquiry
    {
        public async Task<string> InvokeAsync(string Request)
        {
            var Log = new Log();
            var HttpService = new HttpService();
            try
            {
                Log.Write("NibssInward.NameEnquiry", $"Request from Nibss Enc: {Request}");
                var Pgp = new Pgp();

                var Req = Pgp.Decrypt(Request).Value;
                Log.Write("NibssInward.NameEnquiry", $"Request : {Req}");

                var ReqObj = (NESingleRequest)CoralPay.Serialization.Xml.DeSerialize(Req, new NESingleRequest());

                //call router

                NameEnquiryPxRequest MomoReq = new NameEnquiryPxRequest
                {
                    AccountId = ReqObj.AccountNumber,
                    DestinationBankCode = ReqObj.DestinationInstitutionCode,
                    SourceBankCode = ReqObj.SessionID.Substring(0, 6),
                };

                var RouterResp = await HttpService.Call(MomoReq, Operation.NameEnqury);

                NESingleResponse Resp = new NESingleResponse();

                if (RouterResp.ResponseHeader.ResponseCode != "00")
                {
                    Resp = new NESingleResponse
                    {
                        ResponseCode = "01",
                    };
                }
                else
                {
                    var RouterRespObj = JsonConvert.DeserializeObject<NameEnquiryPxResponse>(RouterResp.ResponseContent);
                    Resp = new NESingleResponse
                    {
                        ResponseCode = RouterRespObj.ResponseCode,
                        AccountName = RouterRespObj.AccountName,
                        AccountNumber = RouterRespObj.AccountNumber,
                        SessionID = ReqObj.SessionID,
                        ChannelCode = ReqObj.ChannelCode,
                        BankVerificationNumber = RouterRespObj.Bvn,
                        DestinationInstitutionCode = RouterRespObj.DestinationBankCode,
                        KYCLevel = RouterRespObj.KycLevel,
                    };
                }
                var RespJson = JsonConvert.SerializeObject(Resp);
                Log.Write("NibssInward.NameEnquiry", $"Response to Nibss:  {RespJson}");
                var Enc = Pgp.Encrypt(RespJson).Value;
                Log.Write("NibssInward.NameEnquiry", $"Response to Nibss Enc:  {Enc}");

                return Enc;
            }
            catch (Exception Ex)
            {
                Log.Write("NibssInward.NameEnquiry", $"Err: {Ex.Message}");
                return null;
            }
        }
    }
}