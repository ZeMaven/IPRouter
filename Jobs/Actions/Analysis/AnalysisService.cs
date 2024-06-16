using Jobs.Models.DataBase;
using Momo.Common.Actions;
using Momo.Common.Models.Tables;

namespace Jobs.Actions.Analysis
{
    public interface IAnalysisService
    {
        void Analyse();
    }

    public class AnalysisService : IAnalysisService
    {


        private readonly ILog Log;
        private readonly IConfiguration Config;
        public AnalysisService(ILog log, IConfiguration config)
        {
            Config = config;
            Log = log;
        }


        public void Analyse()
        {
            try
            {
                int Rate;
                var IntervalMin = Convert.ToInt16(Config.GetSection("AnalysisIntervalMin").Value);
                var HrAgo = DateTime.Now.AddHours(-IntervalMin);
                var Db = new MomoSwitchDbContext();
                var Tran = Db.TransactionTb.Where(x => x.Date > HrAgo).ToList();

                var AllBanks = Db.BanksTb.Select(x => x).Distinct().ToList();
                var BankCodeList = AllBanks.Select(x => x.BankCode).Distinct().ToList();
                List<PerformanceTb> Performance = new();
                foreach (var BankCode in BankCodeList)
                {
                    Rate = -1;
                    var BankTran = Tran.Where(x => x.BenefBankCode == BankCode).ToList();
                    if (BankTran.Count > 0)
                    {
                        var Succ = (decimal)BankTran.Where(x => x.ResponseCode == "00" && x.ResponseCode != "09").Count();
                        Rate = (int)Math.Round(Succ / BankTran.Count * 100);

                    }

                    Performance.Add(new PerformanceTb
                    {
                        Rate = Rate,
                        BankCode = BankCode,
                        BankName = AllBanks.Where(x => x.BankCode == BankCode).FirstOrDefault().BankName,
                        Time = DateTime.Now,
                        Remark = GetPerformance(Rate)
                    });
                }

                Db.PerformanceTb.RemoveRange(Db.PerformanceTb);

                Db.PerformanceTb.AddRange(Performance.OrderByDescending(x => x.Rate));

                Db.SaveChanges();

            }
            catch (Exception Ex)
            {
                Log.Write("Transaction.Analyse", $"Finished {Ex.Message}");
            }
        }


        private string GetPerformance(int rate)
        {
            switch (rate)
            {
                case int n when n >= 95 && n <= 100:
                    return "Excellent";
                case int n when n >= 90 && n <= 94:
                    return "Very Good";
                case int n when n >= 85 && n <= 89:
                    return "Good";
                case int n when n >= 80 && n <= 84:
                    return "Satisfactory";
                case int n when n >= 75 && n <= 79:
                    return "Fair";
                case int n when n >= 70 && n <= 74:
                    return "Poor";
                case int n when n >= 65 && n <= 69:
                    return "Very Poor";
                case int n when n >= 60 && n <= 64:
                    return "Unreliable";
                case int n when n >= 55 && n <= 59:
                    return "Unstable";
                case int n when n >= 1 && n <= 54:
                    return "Bad";
                case int n when n == 0:
                    return "No Success";
                case int n when n == -1:
                    return "No Data";
                default:
                    return "Out of range";
            }
        }
    }
}
