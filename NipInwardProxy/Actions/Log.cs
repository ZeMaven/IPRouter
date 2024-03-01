using System;
using System.IO;
using System.Web.UI.WebControls;
using Microsoft.Extensions.Logging;

namespace NipInwardProxy.Actions
{
    public interface ILog
    {
        void Write(string Method, string Message);      
    }


    public class Log :ILog
    {
       
        static private readonly object WriteLock = new object();
        private readonly ILogger<Log> logger;

        public void Write(string Method, string Message)
        {
            try
            {
                lock (WriteLock)
                {
                    var LogPath = NipInwardProxy.Properties.Settings.Default.LogPath;

                    if (!Directory.Exists(LogPath)) Directory.CreateDirectory(LogPath);
                    var Writer = new StreamWriter($"{LogPath}/{DateTime.Today.ToString("dd-MMM-yyyy")}.Txt ", true);
                    Writer.Write($"~ {DateTime.Now.ToString("dd-MMM-yyyy HH:mm:ss fff")} | Method: {Method} | Message: {Message}  {Environment.NewLine}");
                    Writer.Close();
                    logger.LogInformation($"~ {DateTime.Now.ToString("dd-MMM-yyyy HH:mm:ss fff")} | Method: {Method} | Message: {Message}");
                }
            }
            catch
            {
                
            }
        }
    }
}
