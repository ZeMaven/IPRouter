using Momo.Common.Models.Tables;

namespace MomoSwitchPortal.Models.ViewModels.Home
{
    public class HomeViewModel
    {
        public ResponseHeader ResponseHeader { get; set; }
        public DashboardData DashboardData { get; set; }
    }

    public class DashboardData
    {
        public string TotalIncoming { get; set; }
        public string TotalOutGoing { get; set; }
        public double TotalOutGoingCount { get; set; }
        public double TotalIncomingCount { get; set; }
        public double TotalOutGoingPercentage { get; set; }
        public double TotalIncomingPercentage { get; set; }
        public string TotalSuccessful { get; set; }
        public string TotalFailed { get; set; }
        public double TotalSuccessfulCount { get; set; }
        public double TotalFailedCount { get; set; }
        public double TotalSuccessfulPercentage { get; set; }
        public double TotalFailedPercentage { get; set; }
        public DailyTrendViewModel DailyTrend { get; set; }
        public WeeklyTrendViewModel SuccessfulWeeklyTrend { get; set; }
        public WeeklyTrendViewModel FailedWeeklyTrend { get; set; }
        public List<HomeMiniTransaction> RecentTransactions { get; set; }        
        public double TotalUsers { get; set; }
        public double TotalSwitches { get; set; }
        public double TotalTransactions { get; set; }

    }
    public class DailyTrendViewModel
    {
        public double Monday { get; set; }
        public double Tuesday { get; set; }
        public double Wednesday { get; set; }
        public double Thursday { get; set; }
        public double Friday { get; set; }
        public double Saturday { get; set; }
        public double Sunday { get; set; }
    }

    public class MonthlyTrendViewModel
    {
        public string[] Months { get; set; }        
        public double[] MonthsCount { get; set; }        
    }

    public class WeeklyTrendViewModel
    {
        public string[] Weeks { get; set; }
        public double?[] WeeksCount { get; set; }
    }
}
