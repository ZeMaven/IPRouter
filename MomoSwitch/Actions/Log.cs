using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace MomoSwitch.Actions
{
    public interface ILog
    {
        void Write(string Method, string Message);
    }

    public class Log : ILog
    {
        private readonly IConfiguration Configuration;
        private readonly ILogger<Log> logger;

        static private readonly object WriteLock = new object();

        public Log(IConfiguration config, ILogger<Log> _logger)
        {
            Configuration = config;
            logger = _logger;

        }

        public void Write(string Method, string Message)
        {
            try
            {
                lock (WriteLock)
                {
                    var LogPath = Configuration.GetSection("LogPath").Value;

                    if (!Directory.Exists(LogPath)) Directory.CreateDirectory(LogPath);
                    var Writer = new StreamWriter($"{LogPath}/{DateTime.Today.ToString("dd-MMM-yyyy")}.Txt ", true);
                    Writer.Write($"~ {DateTime.Now.ToString("dd-MMM-yyyy HH:mm:ss fff")} | Method: {Method} | Message: {Message}  {Environment.NewLine}");
                    Writer.Close();
                }
            }
            catch
            {
                logger.LogInformation($"~ {DateTime.Now.ToString("dd-MMM-yyyy HH:mm:ss fff")} | Method: {Method} | Message: {Message}");
            }
        }
    }
}