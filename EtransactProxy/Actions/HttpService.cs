using Momo.Common.Models.HttpService;
using Momo.Common.Models;
using Momo.Common.Actions;
using System.Text;
using Newtonsoft.Json;

namespace EtransactProxy.Actions
{
    public interface IHttpService
    {
        Task<HttpServiceResponse> Call(object Request, Operation Op);
    }

    public class HttpService : IHttpService
    {
        private readonly ILog Log;
        private readonly IConfiguration Config;

        public HttpService(IConfiguration config, ILog log)
        {
            Log = log;
            Config = config;
        }

        public async Task<HttpServiceResponse> Call(object Request, Operation Op)
        {
            string Url = string.Empty;
            try
            {
                switch (Op)
                {
                    case Operation.NameEnqury:
                        Url = $"{Config.GetSection("EtransactUrl").Value}/account-query";
                        break;
                    case Operation.Transfer:
                        Url = $"{Config.GetSection("EtransactUrl").Value}/fund-transfer";
                        break;
                    case Operation.TranQuery:
                        Url = $"{Config.GetSection("EtransactUrl").Value}/transaction-status";
                        break;
                }

                var WireClient = new HttpClient();
                //WireClient.DefaultRequestHeaders.Accept.Clear();
                //WireClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {NipToken}");

                var JsonString = JsonConvert.SerializeObject(Request);
                //Log.Write("HttpService.Call", $"Request to Router: {JsonString}");
                Log.Write("HttpService.Call", $"Request to Etransact: {JsonString}"); //block latter
                HttpResponseMessage response = null;

                response = await WireClient.PostAsync(Url, new StringContent(JsonString, Encoding.UTF8, "application/json"));
                string Result = await response.Content.ReadAsStringAsync();

                Log.Write("HttpService.Call", $"HttpStatus: {response.StatusCode}");
                Log.Write("HttpService.Call", $"HttpReasonPhrase: {response.ReasonPhrase}");
                Log.Write("HttpService.Call", $"Etransact Response: {Result}");

                if (string.IsNullOrEmpty(Result))
                {
                    Log.Write("HttpService.Call", $"Etransact Url: {Url}");
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
                Log.Write("HttpService.Call", $"Router Url: {Url}");
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
    }
}