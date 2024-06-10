using Azure.Core;
using Microsoft.AspNetCore.Mvc.Filters;
using Momo.Common.Actions;
using System.IO;
using System.Text.Json;

namespace MomoSwitch.Actions
{
    public class ActionRequestLogger<T> : ActionFilterAttribute
    {
        private readonly ILog _log;
        public ActionRequestLogger(ILog log)
        {
            _log = log;
        }


        public override void OnActionExecuting(ActionExecutingContext context)
        {
            try
            {
                // Log the request details
                var request = context.HttpContext.Request;
                var method = request.Method;
                var path = request.Path;

        

                var req = context.ActionArguments["Req"];
                var JsonReq = JsonSerializer.Serialize(req);
                _log.Write($"{method}|{path}", $"Request: {JsonReq} ");
                var obj = JsonSerializer.Deserialize<T>(JsonReq);

            }
            catch (Exception ex)
            {
                _log.Write($"OnActionExecuting", $"BadRequest Error: {ex.Message} ");
            }                     
        }
    }
}
