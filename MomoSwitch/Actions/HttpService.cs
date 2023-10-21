using MomoSwitch.Models.Internals.HttpService;
using System.Net;
using System.Text.Json;
using System.Text;

namespace MomoSwitch.Actions
{
    public interface IHttpService
    {
        Task<HttpServiceResponse> Call(HttpServiceRequest Request);
    }

    public class HttpService : IHttpService
    {

        private readonly ILog Log;
        public HttpService(ILog log)
        {
            Log = log;
        }

        public async Task<HttpServiceResponse> Call(HttpServiceRequest Request)
        {

            var WireClient = new HttpClient();
            if (Request.SslOveride == true)
            {
                HttpClientHandler clientHandler = new HttpClientHandler();
                clientHandler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true;
                WireClient = new HttpClient(clientHandler);
            }

            //WireClient.DefaultRequestHeaders.Accept.Clear();
            //WireClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {NipToken}");

            //string Auth = Convert.ToBase64String(Encoding.Default.GetBytes($"{Uname}:{Pswd}"));
            //WireClient.DefaultRequestHeaders.Add("Authorization", $"Basic {Auth}");


            try
            {
                HttpResponseMessage response = null;
                string ReqJson = JsonSerializer.Serialize(Request.RequestObject);


                if (Request.Method == Method.Post)
                    response = await WireClient.PostAsync(Request.EndPoint, new StringContent(ReqJson, Encoding.UTF8, "application/json"));

                if (Request.Method == Method.Get)
                    response = await WireClient.GetAsync(Request.EndPoint);


                string Result = response.Content.ReadAsStringAsync().Result;

                Log.Write("HttpService.Call", $"Response:{Result}");

                return new HttpServiceResponse
                {
                    Object = Result,

                    ResponseHeader = new Models.Internals.HttpService.HttpResponseHeader
                    {
                        HttpStatusCode = response.StatusCode,
                        ResponseCode = HttpServiceStatus.Success,
                        ResponseMessage = response.StatusCode.ToString(),
                    }
                };
            }
            catch (Exception ex)
            {
                Log.Write("HttpService.Call", $"Err Response:{ex.Message}");

                return new HttpServiceResponse
                {
                    Object = null,
                    ResponseHeader = new Models.Internals.HttpService.HttpResponseHeader

                    {
                        HttpStatusCode = HttpStatusCode.InternalServerError,
                        ResponseCode = HttpServiceStatus.Error,
                        ResponseMessage = ex.Message
                    }
                };
            }
        }
    }
}