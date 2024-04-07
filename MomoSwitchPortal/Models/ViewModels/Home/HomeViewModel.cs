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
        public int TotalOutGoingCount { get; set; }
        public int TotalIncomingCount { get; set; }
        public string TotalSuccessful { get; set; }
        public string TotalFailed { get; set; }
        public int TotalSuccessfulCount { get; set; }
        public int TotalFailedCount { get; set; }
        public DailyTrendViewModel DailyTrend { get; set; }
        public WeeklyTrendViewModel SuccessfulWeeklyTrend { get; set; }
        public WeeklyTrendViewModel FailedWeeklyTrend { get; set; }
        public List<HomeMiniTransaction> RecentTransactions { get; set; }        
        public int TotalUsers { get; set; }
        public int TotalSwitches { get; set; }
        public int TotalTransactions { get; set; }
    }
    public class DailyTrendViewModel
    {
        public int Monday { get; set; }
        public int Tuesday { get; set; }
        public int Wednesday { get; set; }
        public int Thursday { get; set; }
        public int Friday { get; set; }
        public int Saturday { get; set; }
        public int Sunday { get; set; }
    }

    public class MonthlyTrendViewModel
    {
        public string[] Months { get; set; }        
        public int[] MonthsCount { get; set; }        
    }

    public class WeeklyTrendViewModel
    {
        public string[] Weeks { get; set; }
        public int[] WeeksCount { get; set; }
    }
}
