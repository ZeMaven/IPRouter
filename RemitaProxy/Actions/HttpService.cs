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

                        Url = $"{Config.GetSection("RemitaNameEnqUrl").Value}";
                        break;
                    case Operation.Transfer:
                        Url = $"{Config.GetSection("RemitaPostUrl").Value}";
                        break;
                    case Operation.TranQuery:
                        TranQueryRequest tranQueryReq = (TranQueryRequest)Request;
                        Url = $"{Config.GetSection("RemitaNameTxqUrl").Value}/{tranQueryReq.transactionRef}";
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



                if (Op == Operation.TranQuery)
                    response = await WireClient.GetAsync(Url);
                else
                    response = await WireClient.PostAsync(Url, new StringContent(JsonString, Encoding.UTF8, "application/json"));



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
            var Url = $"{Config.GetSection("RemitaNameAuthUrl").Value}";
            var Username = Config.GetSection("PassUser").Value;
            var Password = Config.GetSection("Password").Value;

            var AuthReq = new
            {
                username = Username,
                password = Password
            };

            var client = new HttpClient();
            // client.BaseAddress = new Uri(Url);
            var jsonReq = JsonConvert.SerializeObject(AuthReq);
            var HttpResp = await client.PostAsync(Url, new StringContent(jsonReq, Encoding.UTF8, "application/json"));
            var jsonResp = await HttpResp.Content.ReadAsStringAsync();
            Log.Write("Remita.Auth", $"Response from Remita: {jsonResp}");
            if (!HttpResp.IsSuccessStatusCode)
            {
                return new ResponseHeader
                {
                    ResponseCode = "01",
                    ResponseMessage = "Auth failed"
                };
            }
            var RemitaRespObj = JsonConvert.DeserializeObject<Models.Auth.AuthResponse>(jsonResp);
            return new ResponseHeader
            {
                ResponseCode = "00",
                ResponseMessage = RemitaRespObj.data[0].accessToken
            };
        }
    }
}