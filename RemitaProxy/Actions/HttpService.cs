using Momo.Common.Models.HttpService;
using Momo.Common.Models;
using Momo.Common.Actions;
using System.Text;

using RemitaProxy.Models.TranQuery;
using Newtonsoft.Json;
using RemitaProxy.Models.Auth;
using Microsoft.Identity.Client;
using Microsoft.Extensions.Caching.Memory;

namespace RemitaProxy.Actions
{
    public interface IHttpService
    {
        Task<HttpServiceResponse> Call(object Request, Operation Op);
    }

    public class HttpService : IHttpService
    {
        private readonly ILog Log;
        private readonly IConfiguration Config;
        private readonly IMemoryCache TokenCache;

        public HttpService(IConfiguration config, ILog log, IMemoryCache tokenCache)
        {
            Log = log;
            Config = config;
            TokenCache = tokenCache;
        }

        public async Task<HttpServiceResponse> Call(object Request, Operation Op)
        {
            string Url = string.Empty;
            try
            {
                switch (Op)
                {
                    case Operation.NameEnqury:

                        Url = $"{Config.GetSection("RemitaUrl").Value}/account/lookup";
                        break;
                    case Operation.Transfer:
                        Url = $"{Config.GetSection("RemitaUrl").Value}/single/payment";
                        break;
                    case Operation.TranQuery:
                        TranQueryRequest tranQueryReq = (TranQueryRequest)Request;
                        Url = $"{Config.GetSection("RemitaUrl").Value}/single/payment/status/{tranQueryReq.transactionRef}";
                        break;
                    case Operation.Auth:
                        Url = $"{Config.GetSection("RemitaUrl").Value}/uaasvc/uaa/token";
                        break;
                }
                var Token = GetToken();
                var WireClient = new HttpClient();
                WireClient.DefaultRequestHeaders.Accept.Clear();
                WireClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {Token}");

                var JsonString = JsonConvert.SerializeObject(Request);
                //Log.Write("HttpService.Call", $"Request to Router: {JsonString}");
                Log.Write("HttpService.Call", $"Request to Remita: {JsonString}"); //block latter
                HttpResponseMessage response;



                if (Op == Operation.Transfer)
                    response = await WireClient.PostAsync(Url, new StringContent(JsonString, Encoding.UTF8, "application/json"));
                else
                    response = await WireClient.GetAsync(Url);


                string Result = await response.Content.ReadAsStringAsync();

                Log.Write("HttpService.Call", $"HttpStatus: {response.StatusCode}");
                Log.Write("HttpService.Call", $"HttpReasonPhrase: {response.ReasonPhrase}");
                Log.Write("HttpService.Call", $"Remita Response: {Result}");

                if (!response.IsSuccessStatusCode)
                {
                    Log.Write("HttpService.Call", $"Remita Url: {Url}");
                    return new HttpServiceResponse
                    {
                        ResponseHeader = new ResponseHeader
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
                        ResponseHeader = new ResponseHeader
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
                Log.Write("HttpService.Call", $"Remita Url: {Url}");
                return new HttpServiceResponse
                {
                    ResponseHeader = new ResponseHeader
                    {
                        ResponseCode = "01",
                        ResponseMessage = "System challenge"
                    },
                    ResponseContent = null
                };
            }
        }


        private string GetToken()
        {
            try
            {
                #region RemitaTokenCache
                var RemitaToken = TokenCache.Get<string>($"RmtToken");
                if (RemitaToken == null || RemitaToken.ToUpper() == "FAILED")
                {
                    RemitaToken = Auth().Result.ResponseMessage;
                    TokenCache.Set($"RmtToken", RemitaToken, TimeSpan.FromMinutes(55));
                }
                #endregion

                return RemitaToken;
            }
            catch (Exception Ex)
            {
                Log.Write("Remita.GetToken", $"Err: {Ex.Message}");
                return "";

            }
        }


        private async Task<ResponseHeader> Auth()
        {
            var Url = Config.GetSection("RemitaUrl").Value;
            var Username = Config.GetSection("Username").Value;
            var Password = Config.GetSection("Password").Value;

            var AuthReq = new
            {
                username = Username,
                password = Password
            };


            var RemitaResp = await Call(AuthReq, Operation.Auth);
            var JsonReq = JsonConvert.SerializeObject(RemitaResp);

            Log.Write("Remita.Auth", $"Request to Remita");
            Log.Write("Remita.FundTransfer", $"Response from Remita: {RemitaResp.ResponseContent}");
            if (RemitaResp.ResponseHeader.ResponseCode != "00")
            {
                return new ResponseHeader
                {
                    ResponseCode = "01",
                    ResponseMessage = "Auth failed"
                };

            }
            var RemitaRespObj = JsonConvert.DeserializeObject<Models.Auth.AuthResponse>(RemitaResp.ResponseContent);

            return new ResponseHeader
            {
                ResponseCode = "00",
                ResponseMessage = RemitaRespObj.data[0].accessToken
            };
        }
    }
}