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
    public class Callback
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

                var ReqObj = (FTAckCredit)CoralPay.Serialization.Xml.DeSerialize(Req, new FTAckCredit());

                //call router

                CallbackPxRequest MomoReq = new CallbackPxRequest
                {
                    OriginatorAccountName = ReqObj.OriginatorAccountName,
                    SessionID = ReqObj.SessionID,
                    DestinationInstitutionCode = ReqObj.DestinationInstitutionCode,
                    Amount = ReqObj.Amount,
                    BeneficiaryAccountName = ReqObj.BeneficiaryAccountName,
                    BeneficiaryAccountNumber = ReqObj.BeneficiaryAccountNumber,
                    BeneficiaryBankVerificationNumber = ReqObj.BeneficiaryBankVerificationNumber,
                    BeneficiaryKYCLevel = ReqObj.BeneficiaryKYCLevel,
                    ChannelCode = ReqObj.ChannelCode,
                    NameEnquiryRef = ReqObj.NameEnquiryRef,
                    Narration = ReqObj.Narration,
                    OriginatorAccountNumber = ReqObj.OriginatorAccountNumber,
                    OriginatorBankVerificationNumber = ReqObj.OriginatorBankVerificationNumber,
                    OriginatorKYCLevel = ReqObj.OriginatorKYCLevel,
                    PaymentReference = ReqObj.PaymentReference,
                    ResponseCode = ReqObj.ResponseCode,
                    TransactionLocation = ReqObj.TransactionLocation
                };

                var RouterResp = await HttpService.Call(MomoReq, Operation.NameEnqury);

                string Resp;

                if (RouterResp.ResponseHeader.ResponseCode != "00")
                {
                    Resp = "Failed";
                }
                else
                {
                    var RouterRespObj = JsonConvert.DeserializeObject<AccountBlockPxResponse>(RouterResp.ResponseContent);
                    Resp = "Ok";
                }

                Log.Write("NibssInward.AccountBlock", $"Response to Nibss:  {Resp}");


                return Resp;
            }
            catch (Exception Ex)
            {
                Log.Write("NibssInward.AccountBlock", $"Err: {Ex.Message}");
                return null;
            }
        }


    }
}