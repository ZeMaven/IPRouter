using Momo.Common.Models.HttpService;
using Momo.Common.Models;
using Newtonsoft.Json;
using System.Text;
using Momo.Common.Actions;
using ArcaProxy.Models.NameEnq;
using ArcaProxy.Models.TranQuery;
using ArcaProxy.Models.Transfer;
using Microsoft.AspNetCore.Http.HttpResults;

namespace ArcaProxy.Actions
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

                        NameEnqRequest nameEnqReq = (NameEnqRequest)Request;
                        Url = $"{Config.GetSection("ArcaUrl").Value}/accountValidations?svaCode={nameEnqReq.svaCode}&accountNumber={nameEnqReq.accountNumber}&institutionCode={nameEnqReq.institutionCode}";
                        break;
                    case Operation.Transfer:
                        Url = $"{Config.GetSection("ArcaUrl").Value}/accountTransfers";
                        break;
                    case Operation.TranQuery:
                        TranQueryRequest tranQueryReq = (TranQueryRequest)Request;
                        Url = $"{Config.GetSection("ArcaUrl").Value}/accountTransferStatus?requestId={tranQueryReq.requestId}&svaCode={tranQueryReq.svaCode}";
                        break;
                }
                var Key = Config.GetSection("ArcaKey").Value;
                var WireClient = new HttpClient();
                WireClient.DefaultRequestHeaders.Accept.Clear();
                WireClient.DefaultRequestHeaders.Add("Key", Key);

                var JsonString = JsonConvert.SerializeObject(Request);
                //Log.Write("HttpService.Call", $"Request to Router: {JsonString}");
                Log.Write("HttpService.Call", $"Request to Arca: {JsonString}"); //block latter
                HttpResponseMessage response;



                if (Op == Operation.Transfer)
                    response = await WireClient.PostAsync(Url, new StringContent(JsonString, Encoding.UTF8, "application/json"));
                else
                    response = await WireClient.GetAsync(Url);


                string Result = await response.Content.ReadAsStringAsync();

                Log.Write("HttpService.Call", $"HttpStatus: {response.StatusCode}");
                Log.Write("HttpService.Call", $"HttpReasonPhrase: {response.ReasonPhrase}");
                Log.Write("HttpService.Call", $"Etransact Response: {Result}");

                if (!response.IsSuccessStatusCode)
                {
                    Log.Write("HttpService.Call", $"Arca Url: {Url}");
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
                Log.Write("HttpService.Call", $"Arca Url: {Url}");
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
