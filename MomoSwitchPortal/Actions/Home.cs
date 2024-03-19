using Momo.Common.Actions;
using Momo.Common.Models.Tables;
using MomoSwitchPortal.Models.ViewModels.Home;
using MomoSwitchPortal.Models;
using System.Globalization;

namespace MomoSwitchPortal.Actions
{
    public interface IHome
    {
        HomeViewModel GetDashboardData(List<TransactionTb> incomingTransactions, List<TransactionTb> outgoingTransactions);
    }
    public class Home : IHome
    {
        private ILog Log;

        public Home(ILog log)
        {
            Log = log;
        }

        public HomeViewModel GetDashboardData(List<TransactionTb> incomingTransactions, List<TransactionTb> outgoingTransactions)
        {
            try
            {

                var successfulOutgoingTransactions = outgoingTransactions.Where(x => x.ResponseCode == "00").ToList();
                var failedOutgoingTransactions = outgoingTransactions.Where(x => x.ResponseCode != "00" && x.ResponseCode != "09").ToList();
                var thisWeekTransactions = GetThisWeekSuccessfulTransactions(successfulOutgoingTransactions);

                //successful half year 
               
                int currentMonth = DateTime.Today.Month;
                int currentYear = DateTime.Today.Year;
             
                int startingMonth = currentMonth <= 6 ? 1 : 7;            

                var successfulHalfYearTransactions = GetHalfYearSuccessfulTransactions(currentYear, startingMonth, successfulOutgoingTransactions);
                var failedHalfYearTransactions = GetHalfYearSuccessfulTransactions(currentYear, startingMonth, failedOutgoingTransactions);

                
                var viewModel = new HomeViewModel
                {
                    ResponseHeader = new ResponseHeader
                    {
                        ResponseCode = "00",
                        ResponseMessage = "Success"
                    },
                    DashboardData = new DashboardData
                    {
                        TotalIncoming = incomingTransactions.Sum(x => x.Amount).ToString("N", CultureInfo.InvariantCulture),
                        TotalOutGoing = outgoingTransactions.Sum(x => x.Amount).ToString("N", CultureInfo.InvariantCulture),
                        TotalIncomingCount = incomingTransactions.Count,
                        TotalOutGoingCount = outgoingTransactions.Count,
                        TotalFailed = failedOutgoingTransactions.Sum(x => x.Amount).ToString("N", CultureInfo.InvariantCulture),
                        TotalFailedCount = failedOutgoingTransactions.Count,
                        TotalSuccessful = successfulOutgoingTransactions.Sum(x => x.Amount).ToString("N", CultureInfo.InvariantCulture),
                        TotalSuccessfulCount = successfulOutgoingTransactions.Count,
                        WeeklyTrend = new WeeklyTrendViewModel
                        {
                            Monday = thisWeekTransactions.Where(x => x.Date.DayOfWeek == DayOfWeek.Monday).ToList().Count,
                            Tuesday = thisWeekTransactions.Where(x => x.Date.DayOfWeek == DayOfWeek.Tuesday).ToList().Count,
                            Wednesday = thisWeekTransactions.Where(x => x.Date.DayOfWeek == DayOfWeek.Wednesday).ToList().Count,
                            Thursday = thisWeekTransactions.Where(x => x.Date.DayOfWeek == DayOfWeek.Thursday).ToList().Count,
                            Friday = thisWeekTransactions.Where(x => x.Date.DayOfWeek == DayOfWeek.Friday).ToList().Count,
                            Saturday = thisWeekTransactions.Where(x => x.Date.DayOfWeek == DayOfWeek.Saturday).ToList().Count,
                            Sunday = thisWeekTransactions.Where(x => x.Date.DayOfWeek == DayOfWeek.Sunday).ToList().Count
                        },
                        SuccessfulHalfYearTrend = successfulHalfYearTransactions,
                        FailedHalfYearTrend = failedHalfYearTransactions
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

        private List<TransactionTb> GetThisWeekSuccessfulTransactions(List<TransactionTb> transactions)
        {
            DateTime startOfWeek = DateTime.Today.AddDays(-(int)DateTime.Today.DayOfWeek + (int)DayOfWeek.Monday);
            DateTime endOfWeek = startOfWeek.AddDays(7).AddSeconds(-1);


            if (DateTime.Today.DayOfWeek == DayOfWeek.Sunday)
            {
                startOfWeek = startOfWeek.AddDays(-7);
                endOfWeek = endOfWeek.AddDays(-7);
            }

            var thisWeekTransactions = new List<TransactionTb>();
            foreach (var transaction in transactions)
            {
                if (transaction.Date >= startOfWeek && transaction.Date <= endOfWeek)
                {
                    thisWeekTransactions.Add(transaction);
                }
            }

            return thisWeekTransactions;
        }

        private MonthlyTrendViewModel GetHalfYearSuccessfulTransactions(int startingYear, int startingMonth, List<TransactionTb> transactions)
        {

            var halfYearSuccessfulTransactions = new MonthlyTrendViewModel
            {
                Months = new string[6],
                MonthsCount = new int[6]
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

    }
}
