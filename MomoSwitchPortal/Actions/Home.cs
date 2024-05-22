using Momo.Common.Actions;
using Momo.Common.Models.Tables;
using MomoSwitchPortal.Models.ViewModels.Home;
using MomoSwitchPortal.Models;
using System.Globalization;
using MomoSwitchPortal.Models.ViewModels.Transaction;

namespace MomoSwitchPortal.Actions
{
    public interface IHome
    {
        HomeViewModel GetDashboardData(List<HomeMiniTransaction> incomingTransactions, List<HomeMiniTransaction> outgoingTransactions);
        HomeViewModel GetTodayDashboardData(List<HomeMiniTransaction> todayIncomingTransactions, List<HomeMiniTransaction> todayOutgoingTransactions);        
    }
    public class Home : IHome
    {
        private ILog Log;

        public Home(ILog log)
        {
            Log = log;
        }

        public HomeViewModel GetDashboardData(List<HomeMiniTransaction> incomingTransactions, List<HomeMiniTransaction> outgoingTransactions)
        {
            try
            {

                var successfulOutgoingTransactions = outgoingTransactions.Where(x => x.ResponseCode == "00").ToList();
                var failedOutgoingTransactions = outgoingTransactions.Where(x => x.ResponseCode != "00" && x.ResponseCode != "09").ToList();
                var thisWeekTransactions = GetThisWeekSuccessfulTransactions(successfulOutgoingTransactions);

                var startDay = DateTime.Parse(DateTime.Now.ToString("yyyy-MM-dd") + " 00:00:00");
                var endDay = DateTime.Parse(DateTime.Now.ToString("yyyy-MM-dd") + " 23:59:59");

               
                var todaySuccessfulTransactions = successfulOutgoingTransactions.Where(x => x.Date >= startDay && x.Date <= endDay).ToList();
                var todayFailedTransactions = failedOutgoingTransactions.Where(x => x.Date >= startDay && x.Date <= endDay).ToList();
                var todayIncomingTransactions = incomingTransactions.Where(x => x.Date >= startDay && x.Date <= endDay).ToList();
                var todayOutgoingTransactions = outgoingTransactions.Where(x => x.Date >= startDay && x.Date <= endDay).ToList();


                    
                int currentMonth = DateTime.Today.Month;
                int currentYear = DateTime.Today.Year;

                var successfulMonthTransactions = GetMonthlyTrend(currentYear, currentMonth, successfulOutgoingTransactions);
                var failedMonthTransactions = GetMonthlyTrend(currentYear, currentMonth, failedOutgoingTransactions);

                var viewModel = new HomeViewModel
                {
                    ResponseHeader = new ResponseHeader
                    {
                        ResponseCode = "00",
                        ResponseMessage = "Success"
                    },
                    DashboardData = new DashboardData
                    {
                        TotalIncoming = todayIncomingTransactions.Sum(x => x.Amount).ToString("N", CultureInfo.InvariantCulture),
                        TotalIncomingCount = todayIncomingTransactions.Count,
                        TotalOutGoing = todayOutgoingTransactions.Sum(x => x.Amount).ToString("N", CultureInfo.InvariantCulture),
                        TotalOutGoingCount = todayOutgoingTransactions.Count,
                        TotalFailed = todayFailedTransactions.Sum(x => x.Amount).ToString("N", CultureInfo.InvariantCulture),
                        TotalFailedCount = todayFailedTransactions.Count,
                        TotalSuccessful = todaySuccessfulTransactions.Sum(x => x.Amount).ToString("N", CultureInfo.InvariantCulture),
                        TotalSuccessfulCount = todaySuccessfulTransactions.Count,
                        DailyTrend = new DailyTrendViewModel
                        {
                            Monday = thisWeekTransactions.Where(x => x.Date.DayOfWeek == DayOfWeek.Monday).ToList().Count,
                            Tuesday = thisWeekTransactions.Where(x => x.Date.DayOfWeek == DayOfWeek.Tuesday).ToList().Count,
                            Wednesday = thisWeekTransactions.Where(x => x.Date.DayOfWeek == DayOfWeek.Wednesday).ToList().Count,
                            Thursday = thisWeekTransactions.Where(x => x.Date.DayOfWeek == DayOfWeek.Thursday).ToList().Count,
                            Friday = thisWeekTransactions.Where(x => x.Date.DayOfWeek == DayOfWeek.Friday).ToList().Count,
                            Saturday = thisWeekTransactions.Where(x => x.Date.DayOfWeek == DayOfWeek.Saturday).ToList().Count,
                            Sunday = thisWeekTransactions.Where(x => x.Date.DayOfWeek == DayOfWeek.Sunday).ToList().Count
                        },
                       SuccessfulWeeklyTrend = successfulMonthTransactions,
                       FailedWeeklyTrend = failedMonthTransactions                  
                    }
                    
                    
                };

                return viewModel;
            }
            catch (Exception ex)
            {
                Log.Write("Home:GetDashboardData", $"eRR: {ex.Message}");
                return new HomeViewModel
                {
                    ResponseHeader = new ResponseHeader
                    {
                        ResponseCode = "01",
                        ResponseMessage = "System Challenge"
                    }
                };
            }
        }

        private List<HomeMiniTransaction> GetThisWeekSuccessfulTransactions(List<HomeMiniTransaction> transactions)
        {
            DateTime startOfWeek = DateTime.Today.AddDays(-(int)DateTime.Today.DayOfWeek + (int)DayOfWeek.Monday);
            DateTime endOfWeek = startOfWeek.AddDays(7).AddSeconds(-1);


            if (DateTime.Today.DayOfWeek == DayOfWeek.Sunday)
            {
                startOfWeek = startOfWeek.AddDays(-7);
                endOfWeek = endOfWeek.AddDays(-7);
            }

            var thisWeekTransactions = new List<HomeMiniTransaction>();
            foreach (var transaction in transactions)
            {
                if (transaction.Date >= startOfWeek && transaction.Date <= endOfWeek)
                {
                    thisWeekTransactions.Add(transaction);
                }
            }

            return thisWeekTransactions;
        }

        private MonthlyTrendViewModel GetHalfYearSuccessfulTransactions(int startingYear, int startingMonth, List<HomeMiniTransaction> transactions)
        {

            var halfYearSuccessfulTransactions = new MonthlyTrendViewModel
            {
                Months = new string[6],
                MonthsCount = new double[6]
            };
            for (int i = 0; i < 6; i++)
            {
                var month = new DateTime(startingYear, startingMonth, 1).ToString("MMMM");                
                var monthTransactions = transactions.Where(x => x.Date.Month == startingMonth).ToList();
                halfYearSuccessfulTransactions.Months[i] = month[..3];
                foreach (var transaction in monthTransactions)
                {
                    halfYearSuccessfulTransactions.MonthsCount[i] = monthTransactions.Count;
                }

                startingMonth++;
                if (startingMonth > 12)
                {
                    startingYear++;
                }
            }
                
            return halfYearSuccessfulTransactions;
        }

        private WeeklyTrendViewModel GetMonthlyTrend(int startingYear, int startingMonth, List<HomeMiniTransaction> transactions)
        {
            var currentDate = DateTime.Now;
            var startOfMonth = new DateTime(startingYear, startingMonth, 1);
            var endOfMonth = startOfMonth.AddMonths(1).AddDays(-1); // Last day of the current month

            var weekRanges = new List<(DateTime, DateTime)>();
            for (int i = 0; i < 5; i++)
            {
                var weekStart = DateTime.Parse(startOfMonth.AddDays(i * 7).ToString("yyyy-MM-dd") + " 00:00:00");
                var weekEnd = DateTime.Parse(weekStart.AddDays(6).ToString("yyyy-MM-dd") + " 23:59:59");
                weekRanges.Add((weekStart, weekEnd));
            }

            var weeklyTrendViewModel = new WeeklyTrendViewModel()
            {
                Weeks = new string[5],
                WeeksCount = new double?[5]
            };

            for (int i = 0; i < 5; i++)
            {
                var transactionsInWeek = transactions
                    .Where(t => t.Date >= weekRanges[i].Item1 && t.Date <= weekRanges[i].Item2)
                    .ToList();

                weeklyTrendViewModel.Weeks[i] = $"Week {i + 1}";
                weeklyTrendViewModel.WeeksCount[i] = transactionsInWeek.Count == 0 ? null : transactionsInWeek.Count;
            }               

            return weeklyTrendViewModel;
        }

        public HomeViewModel GetTodayDashboardData(List<HomeMiniTransaction> todayIncomingTransactions, List<HomeMiniTransaction> todayOutgoingTransactions)
        {
            try
            {

                var successfulOutgoingTransactions = todayOutgoingTransactions.Where(x => x.ResponseCode == "00").ToList();
                var failedOutgoingTransactions = todayOutgoingTransactions.Where(x => x.ResponseCode != "00" && x.ResponseCode != "09").ToList();                
    
                var todaySuccessfulTransactions = successfulOutgoingTransactions.ToList();
                var todayFailedTransactions = failedOutgoingTransactions.ToList();             



                int currentMonth = DateTime.Today.Month;
                int currentYear = DateTime.Today.Year;
              
                var viewModel = new HomeViewModel
                {
                    ResponseHeader = new ResponseHeader
                    {
                        ResponseCode = "00",
                        ResponseMessage = "Success"
                    },
                    DashboardData = new DashboardData
                    {
                        TotalIncoming = todayIncomingTransactions.Sum(x => x.Amount).ToString("N", CultureInfo.InvariantCulture),
                        TotalIncomingCount = todayIncomingTransactions.Count,
                        TotalOutGoing = todayOutgoingTransactions.Sum(x => x.Amount).ToString("N", CultureInfo.InvariantCulture),
                        TotalOutGoingCount = todayOutgoingTransactions.Count,
                        TotalFailed = todayFailedTransactions.Sum(x => x.Amount).ToString("N", CultureInfo.InvariantCulture),
                        TotalFailedCount = todayFailedTransactions.Count,
                        TotalSuccessful = todaySuccessfulTransactions.Sum(x => x.Amount).ToString("N", CultureInfo.InvariantCulture),
                        TotalSuccessfulCount = todaySuccessfulTransactions.Count,                    
                    }
                };

                return viewModel;
            }
            catch (Exception ex)
            {
                Log.Write("Home:GetDashboardData", $"eRR: {ex.Message}");
                return new HomeViewModel
                {
                    ResponseHeader = new ResponseHeader
                    {
                        ResponseCode = "01",
                        ResponseMessage = "System Challenge"
                    }
                };
            }
        }
    }
}
