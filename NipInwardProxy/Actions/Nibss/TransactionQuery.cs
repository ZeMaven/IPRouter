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
    public class TransactionQuery
    {

        public async Task<string> InvokeAsync(string Request)
        {
            var Log = new Log();
            var HttpService = new HttpService();
            try
            {
                Log.Write("NibssInward.TransactionQuery", $"Request from Nibss Enc: {Request}");
                var Pgp = new Pgp();

                var Req = Pgp.Decrypt(Request).Value;
                Log.Write("NibssInward.TransactionQuery", $"Request : {Req}");

                var ReqObj = (TSQuerySingleRequest)CoralPay.Serialization.Xml.DeSerialize(Req, new TSQuerySingleRequest());

                //call router

                TranQueryPxRequest MomoReq = new TranQueryPxRequest
                {
                    TransactionId = ReqObj.SessionID
                };

                var RouterResp = await HttpService.Call(MomoReq, Operation.TranQuery);

                TSQuerySingleResponse Resp = new TSQuerySingleResponse();

                if (RouterResp.ResponseHeader.ResponseCode != "00")
                {
                    Resp = new TSQuerySingleResponse
                    {
                        ResponseCode = "01",
                    };
                }
                else
                {
                    var RouterRespObj = JsonConvert.DeserializeObject<TranQueryPxResponse>(RouterResp.ResponseContent);
                    Resp = new TSQuerySingleResponse
                    {
                        ResponseCode = RouterRespObj.ResponseCode,
                        SourceInstitutionCode = RouterRespObj.SourceBankCode,
                        SessionID = ReqObj.SessionID,
                        ChannelCode = ReqObj.ChannelCode, 
                           
                    };
                }
                var RespJson = JsonConvert.SerializeObject(Resp);
                Log.Write("NibssInward.TransactionQuery", $"Response to Nibss:  {RespJson}");
                var Enc = Pgp.Encrypt(RespJson).Value;
                Log.Write("NibssInward.TransactionQuery", $"Response to Nibss Enc:  {Enc}");

                return Enc;
            }
            catch (Exception Ex)
            {
                Log.Write("NibssInward.TransactionQuery", $"Err: {Ex.Message}");
                return null;
            }
        }
    }
}