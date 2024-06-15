using Cronos;
using Momo.Common.Actions;

namespace Jobs.Actions.Reconciliation
{
    public class ReconWorker : BackgroundService
    {

        private readonly ILog _log;
        private string Schedule;
        private readonly CronExpression _cron;
        private readonly IReconService _recon;
        private readonly IConfiguration _config;


        public ReconWorker(ILog log, IReconService recon, IConfiguration config)
        {
            _config = config;
            _log = log;
            _recon = recon;
            Schedule = _config.GetSection("ReconSchedule").Value;
            _cron = CronExpression.Parse(Schedule);   //  "*/5 * * * *";
        }

        public override Task StartAsync(CancellationToken cancellationToken)
        {
            _log.Write("ReconWorker", "Worker service started");
            return base.StartAsync(cancellationToken);
        }

        public override Task StopAsync(CancellationToken cancellationToken)
        {
            _log.Write("ReconWorker", "Worker service stopped");
            return base.StopAsync(cancellationToken);
        }


        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _log.Write("ReconWorker started at:", DateTimeOffset.Now.ToString());
            while (!stoppingToken.IsCancellationRequested)
            {
                var utcNow = DateTime.UtcNow;
                var nextUtc = _cron.GetNextOccurrence(utcNow);
                await Task.Delay(nextUtc.Value - utcNow, stoppingToken);

                _log.Write("ReconWorker Worker Cheking", $"{DateTimeOffset.Now}");
                _recon.Main();
            }
        }

    }
}
