using Cronos;
using Momo.Common.Actions;

namespace Jobs.Actions
{
    public class ReconWorker : BackgroundService
    {

        private readonly ILog Log;
        private string Schedule;
        private readonly CronExpression Cron;
        private readonly IReconcilation Recon;
        private readonly IConfiguration Config;


        public ReconWorker(ILog log, IReconcilation recon, IConfiguration config)
        {
            Config = config;
            Log = log;
            Recon = recon;
            Schedule = Config.GetSection("ReconSchedule").Value;
            Cron = CronExpression.Parse(Schedule);   //  "*/5 * * * *";
        }

        public override Task StartAsync(CancellationToken cancellationToken)
        {
            Log.Write("ReconWorker", "Worker service started");
            return base.StartAsync(cancellationToken);
        }

        public override Task StopAsync(CancellationToken cancellationToken)
        {
            Log.Write("ReconWorker", "Worker service stopped");
            return base.StopAsync(cancellationToken);
        }


        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            Log.Write("ReconWorker started at:", DateTimeOffset.Now.ToString());
            while (!stoppingToken.IsCancellationRequested)
            {
                var utcNow = DateTime.UtcNow;
                var nextUtc = Cron.GetNextOccurrence(utcNow);
                await Task.Delay(nextUtc.Value - utcNow, stoppingToken);

                Log.Write("ReconWorker Worker Cheking", $"{DateTimeOffset.Now}");
                Recon.Main();
            }
        }

    }
}
