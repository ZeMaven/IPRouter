using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace Momo.Common.Actions
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
                        logger.LogInformation($"~ {DateTime.Now.ToString("dd-MMM-yyyy HH:mm:ss fff")} | Method: {Method} | Message: {Message}");
                    }            
            }
            catch
            {
                logger.LogInformation($"~ {DateTime.Now.ToString("dd-MMM-yyyy HH:mm:ss fff")} | Method: {Method} | Message: {Message}");
            }
        }

        public void Write(string Message, LogType? LogType = null, string Method = null, string Tag = null)
        {
            try
            {
                Task.Run(() =>
                {
                    lock (WriteLock)
                    {
                        var LogPath = Configuration.GetSection("LogPath").Value;
                        if (!Directory.Exists(LogPath)) Directory.CreateDirectory(LogPath);
                        using (var Writer = new StreamWriter($"{LogPath}/{DateTime.Today:dd-MMM-yyyy}.Txt", true))
                        {
                            string MethodName = Method ?? GetCurrentMethodName();
                            Writer.Write($"~ {DateTime.Now:dd-MMM-yyyy HH:mm:ss fff} | {FormatLogMessage(LogType, MethodName, Message, Tag)}{Environment.NewLine}");
                        }
                        logger.LogInformation($"~ {DateTime.Now:dd-MMM-yyyy HH:mm:ss fff} | {FormatLogMessage(LogType, Method ?? GetCurrentMethodName(), Message, Tag)}");
                    }
                });
            }
            catch
            {
                string methodName = Method ?? GetCurrentMethodName();
                logger.LogInformation($"~ {DateTime.Now:dd-MMM-yyyy HH:mm:ss fff} | {FormatLogMessage(LogType, methodName, Message, Tag)}");
            }
        }

        private string GetCurrentMethodName()
        {
            var stackTrace = new StackTrace();
            var methodBase = stackTrace.GetFrame(2).GetMethod(); // Get calling method, 2 frames up in the stack
            return $"{methodBase.DeclaringType.FullName}.{methodBase.Name}";
        }

        private string FormatLogMessage(LogType? logType, string method, string message, string tag)
        {
            string logTypeStr = logType.HasValue ? $"LogType: {logType} | " : "";
            string methodStr = !string.IsNullOrEmpty(method) ? $"Method: {method} | " : "";
            string tagStr = !string.IsNullOrEmpty(tag) ? $"Tag: {tag} | " : "";
            return $"{logTypeStr}{methodStr}Message: {message}{tagStr}";
        }


    }

    public enum LogType
    {
        Info,
        Warning,
        Request,
        Response,
        Tag,
        Error
    }

}