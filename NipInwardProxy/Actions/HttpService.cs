using NipInwardProxy.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Net.Http;
using System.Text;
using Newtonsoft.Json;
using System.Threading.Tasks;

namespace NipInwardProxy.Actions
{
    public enum Operation { NameEnqury, Transfer, TranQuery, AccountBlock, AccountUnBlock, AmountBlock,AmountUnblock, BalanceEnquiry, Callback,
    DirectCredit,DirectCreditAdvice,FinancialInstitutionList, ManadateAdvice
     
    }
    public class HttpService
    {



        public async Task<HttpServiceResponse> Call(object Request, Operation Op)
        {
            string Url = string.Empty;
            var Log = new Log();
            try
            {
                switch (Op)
                {
                    case Operation.NameEnqury:
                        Url = $"{Properties.Settings.Default.RouterUrl}/NameEnquiry";
                        break;
                    case Operation.Transfer:
                        Url = $"{Properties.Settings.Default.RouterUrl}/FundTransfer";
                        break;
                    case Operation.TranQuery:
                        Url = $"{Properties.Settings.Default.RouterUrl}/TransactionQuery";
                        break;

                    case Operation.AccountBlock:
                        Url = $"{Properties.Settings.Default.RouterUrl}/AccountBlock";
                        break;

                    case Operation.AccountUnBlock:
                        Url = $"{Properties.Settings.Default.RouterUrl}/AccountUnBlock";
                        break;

                    case Operation.AmountBlock:
                        Url = $"{Properties.Settings.Default.RouterUrl}/AmountBlock";
                        break;

                    case Operation.AmountUnblock:
                        Url = $"{Properties.Settings.Default.RouterUrl}/AmountUnblock";
                        break;

                    case Operation.BalanceEnquiry:
                        Url = $"{Properties.Settings.Default.RouterUrl}/BalanceEnquiry";
                        break;

                    case Operation.Callback:
                        Url = $"{Properties.Settings.Default.RouterUrl}/Callback";
                        break;
                    case Operation.DirectCredit:
                        Url = $"{Properties.Settings.Default.RouterUrl}/DirectCredit";
                        break;

                    case Operation.DirectCreditAdvice:
                        Url = $"{Properties.Settings.Default.RouterUrl}/DirectCreditAdvice";
                        break;

                    case Operation.FinancialInstitutionList:
                        Url = $"{Properties.Settings.Default.RouterUrl}/FinancialInstitutionList";
                        break;

                    case Operation.ManadateAdvice:
                        Url = $"{Properties.Settings.Default.RouterUrl}/ManadateAdvice";
                        break;
                }


                var WireClient = new HttpClient();
                //WireClient.DefaultRequestHeaders.Accept.Clear();
                //WireClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {NipToken}");

                var JsonString = JsonConvert.SerializeObject(Request);
                Log.Write("HttpService.Call", $"Request to Router: {JsonString}");
                HttpResponseMessage response = null;

                response = await WireClient.PostAsync(Url, new StringContent(JsonString, Encoding.UTF8, "application/json"));
                string Result = await response.Content.ReadAsStringAsync();

                Log.Write("HttpService.Call", $"HttpStatus: {response.StatusCode}");
                Log.Write("HttpService.Call", $"HttpReasonPhrase: {response.ReasonPhrase}");
                Log.Write("HttpService.Call", $"Router Response: {Result}");

                if (string.IsNullOrEmpty(Result))
                {
                    Log.Write("HttpService.Call", $"Router Url: {Url}");
                    return new HttpServiceResponse
                    {
                        ResponseHeader = new Models.ResponseHeader
                        {
                            ResponseCode = "01",
                            ResponseMessage = response.ReasonPhrase
                        },
                        ResponseContent = null
                    };
                }
                else
                {
                    return new HttpServiceResponse
                    {
                        ResponseHeader = new Models.ResponseHeader
                        {
                            ResponseCode = "00",
                            ResponseMessage = response.ReasonPhrase
                        },
                        ResponseContent = Result
                    };
                }
            }
            catch (Exception Ex)
            {
                Log.Write("HttpService.Call", $"Err: {Ex.Message}");
                Log.Write("HttpService.Call", $"Router Url: {Url}");
                return new HttpServiceResponse
                {
                    ResponseHeader = new Models.ResponseHeader
                    {
                        ResponseCode = "01",
                        ResponseMessage = "System challenge"
                    },
                    ResponseContent = null
                };
            }
        }


    }
}