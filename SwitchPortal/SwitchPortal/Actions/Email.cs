using Momo.Common.Actions;
using Newtonsoft.Json;
using SwitchPortal.Models;

namespace SwitchPortal.Actions
{
    public interface IEmail
    {
        Task<ResponseHeader> SendEmail(MailRequest request);
    }
    public class Email : IEmail
    {
        private readonly ILog Log;
        private readonly IConfiguration _config;

        public Email(IConfiguration config, ILog log)
        {
            _config = config;
            Log = log;
        }

        public async Task<ResponseHeader> SendEmail(MailRequest request)
        {
            try
            {
                var client = new HttpClient();
                var url = _config.GetValue<string>("CoralMailService:SendEmailUrl");

                var sendEmailReqAsJson = JsonConvert.SerializeObject(request);
                Log.Write("Email:SendEmail", $"Sending email request. Receiver's Email: {request.To}");

                var response = await client.PostAsJsonAsync(url, request);

                var responseContent = await response.Content.ReadAsStringAsync();
                Log.Write("Email:SendEmail", $"Send email response: {responseContent}");

                var result = JsonConvert.DeserializeObject<ResponseHeader>(responseContent);
                return result;
            }
            catch (Exception ex)
            {
                Log.Write("CoralMailServiceClient:SendEmail", $"eRR: {ex.Message}");

                return new ResponseHeader
                {
                    ResponseCode = "01",
                    ResponseMessage = ex.Message
                };
            }
        }
    }
}
