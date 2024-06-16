using Cronos;
using Jobs.Actions.Requery;
using Momo.Common.Actions;

namespace Jobs.Actions.Analysis
{
    public class AnalysisWorker : BackgroundService
    {

        private readonly ILog Log;
        private string Schedule;
        private readonly CronExpression Cron;
        private readonly IAnalysisService Analyse;
        private readonly IConfiguration Config;


        public AnalysisWorker(ILog log, IAnalysisService analyse, IConfiguration config)
        {
            Config = config;
            Log = log;
            Analyse = analyse;
            Schedule = Config.GetSection("AnalysisSchedule").Value;
            Cron = CronExpression.Parse(Schedule);   //  "*/5 * * * *";
        }

        public override Task StartAsync(CancellationToken cancellationToken)
        {
            Log.Write("AnalysisWorker", "Worker service started");
            return base.StartAsync(cancellationToken);
        }

        public override Task StopAsync(CancellationToken cancellationToken)
        {
            Log.Write("AnalysisWorker", "Worker service stopped");
            return base.StopAsync(cancellationToken);
        }


        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            Log.Write("AnalysisWorker started at:", DateTimeOffset.Now.ToString());
            while (!stoppingToken.IsCancellationRequested)
            {
                var utcNow = DateTime.UtcNow;
                var nextUtc = Cron.GetNextOccurrence(utcNow);
                await Task.Delay(nextUtc.Value - utcNow, stoppingToken);

                Log.Write("AnalysisWorker Worker Cheking", $"{DateTimeOffset.Now}");
                Analyse.Analyse();
            }
        }

    }
}
