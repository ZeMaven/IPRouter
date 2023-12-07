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
    public class FinancialInstitutionList
    {

        public async Task<string> InvokeAsync(string Request)
        {
            var Log = new Log();
            var HttpService = new HttpService();
            try
            {
                Log.Write("NibssInward.FinancialInstitutionList", $"Request from Nibss Enc: {Request}");
                var Pgp = new Pgp();

                var Req = Pgp.Decrypt(Request).Value;
                Log.Write("NibssInward.FinancialInstitutionList", $"Request : {Req}");

                var ReqObj = (FinancialInstitutionListRequest)CoralPay.Serialization.Xml.DeSerialize(Req, new FinancialInstitutionListRequest());

                //call router

                FinancialInstitutionListPxRequest MomoReq = new FinancialInstitutionListPxRequest
                {
                    Header = ReqObj.Header,
                    Record = ReqObj.Record,
                };

                var RouterResp = await HttpService.Call(MomoReq, Operation.FinancialInstitutionList);

                FinancialInstitutionListResponse Resp = new FinancialInstitutionListResponse();

                if (RouterResp.ResponseHeader.ResponseCode != "00")
                {
                    Resp = new FinancialInstitutionListResponse
                    {
                        ResponseCode = "01",
                        NumberOfRecords = 0,
                        ChannelCode = ReqObj.Header.ChannelCode,
                    };
                }
                else
                {
                    var RouterRespObj = JsonConvert.DeserializeObject<FinancialInstitutionListPxResponse>(RouterResp.ResponseContent);
                    Resp = new FinancialInstitutionListPxResponse
                    {
                        BatchNumber = RouterRespObj.BatchNumber,
                        ChannelCode = RouterRespObj.ChannelCode,
                        DestinationInstitutionCode = RouterRespObj.DestinationInstitutionCode,
                        NumberOfRecords = RouterRespObj.NumberOfRecords,
                        ResponseCode = RouterRespObj.ResponseCode
                    };
                }
                var RespJson = JsonConvert.SerializeObject(Resp);
                Log.Write("NibssInward.FinancialInstitutionList", $"Response to Nibss:  {RespJson}");
                var Enc = Pgp.Encrypt(RespJson).Value;
                Log.Write("NibssInward.FinancialInstitutionList", $"Response to Nibss Enc:  {Enc}");

                return Enc;
            }
            catch (Exception Ex)
            {
                Log.Write("NibssInward.FinancialInstitutionList", $"Err: {Ex.Message}");
                return null;
            }
        }
    }
}