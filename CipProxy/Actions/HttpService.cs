using CipProxy.Models;
using CoralPay.Cryptography.Pgp.Models;
using Momo.Common.Actions;
using Newtonsoft.Json;
using Org.BouncyCastle.Asn1.Ocsp;
using System.Text;

namespace CipProxy.Actions
{
    public interface IHttpService
    {/// <summary>
    /// Calling CIP
    /// </summary>
    /// <param name="TextRequest"></param>
    /// <param name="Op"></param>
    /// <returns></returns>
        Task<HttpServiceResponse> Call(string TextRequest, Operation Op);
        /// <summary>
        /// Calling the Router
        /// </summary>
        /// <param name="Request"></param>
        /// <param name="Op"></param>
        /// <returns></returns>
        Task<HttpServiceResponse> Call(object Request, Operation Op);

    }

    public class HttpService : IHttpService
    {
        private readonly ILog Log;
        private readonly IConfiguration Config;

        public HttpService(IConfiguration config, ILog log, IUtilities utilities)
        {
            Log = log;
            Config = config;
        }

        public async Task<HttpServiceResponse> Call(string EncTextRequest, Operation Op)
        {
            string Url = string.Empty;
            try
            {
                switch (Op)
                {
                    case Operation.NameEnqury:
                        Url = $"{Config.GetSection("CipUrl").Value}/NameEnquiry";
                        break;
                    case Operation.Transfer:
                        Url = $"{Config.GetSection("CipUrl").Value}/PostCredit";
                        break;
                    case Operation.TranQuery:
                        Url = $"{Config.GetSection("CipUrl").Value}/TransactionQuery";
                        break;
                }

                var WireClient = new HttpClient();
                //WireClient.DefaultRequestHeaders.Accept.Clear();
                //WireClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {NipToken}");

                HttpResponseMessage response = null;

                response = await WireClient.PostAsync(Url, new StringContent(EncTextRequest, Encoding.UTF8, "text/plain"));
                string Result = await response.Content.ReadAsStringAsync();

                Log.Write("HttpService.Call", $"HttpStatus: {response.StatusCode}");
                Log.Write("HttpService.Call", $"HttpReasonPhrase: {response.ReasonPhrase}");
                Log.Write("HttpService.Call", $"Cip Response: {Result}");

                if (string.IsNullOrEmpty(Result))
                {
                    Log.Write("HttpService.Call", $"Cip Url: {Url}");
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
                Log.Write("HttpService.Call", $"Cip Url: {Url}");
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



        public async Task<HttpServiceResponse> Call(object Request, Operation Op)
        {
            string Url = string.Empty;
            try
            {
                switch (Op)
                {
                    case Operation.NameEnqury:
                        Url = $"{Config.GetSection("RouterUrl").Value}/NameEnquiry";
                        break;
                    case Operation.Transfer:
                        Url = $"{Config.GetSection("RouterUrl").Value}/PostCredit";
                        break;
                    case Operation.TranQuery:
                        Url = $"{Config.GetSection("RouterUrl").Value}/TransactionQuery";
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











    public enum Operation { NameEnqury, Transfer, TranQuery }
}