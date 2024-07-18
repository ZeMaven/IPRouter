using Cronos;
using Momo.Common.Actions;

namespace Jobs.Actions.Requery
{
    public class RequeryWorker : BackgroundService
    {
        private readonly ILog Log;
        private string Schedule;
        private readonly CronExpression Cron;
        private readonly ITransaction Tran;
        private readonly IConfiguration Config;

        public RequeryWorker(ILog log, ITransaction tran, IConfiguration config)
        {
            Config = config;
            Log = log;
            Tran = tran;
            Schedule = Config.GetSection("RequerySchedule").Value;
            Cron = CronExpression.Parse(Schedule);   //  "*/5 * * * *";
        }

        public override Task StartAsync(CancellationToken cancellationToken)
        {
            Log.Write("RequeryWorker", "Worker service started");
            return base.StartAsync(cancellationToken);
        }

        public override Task StopAsync(CancellationToken cancellationToken)
        {
            Log.Write("RequeryWorker", "Worker service stopped");
            return base.StopAsync(cancellationToken);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            Log.Write("RequeryWorker started at:", DateTimeOffset.Now.ToString());
            while (!stoppingToken.IsCancellationRequested)
            {
                var utcNow = DateTime.UtcNow;
                var nextUtc = Cron.GetNextOccurrence(utcNow);
                await Task.Delay(nextUtc.Value - utcNow, stoppingToken);

                Log.Write("RequeryWorker Worker Cheking", $"{DateTimeOffset.Now}");
                Tran.Requery(); 
            }
        }
    }
}
