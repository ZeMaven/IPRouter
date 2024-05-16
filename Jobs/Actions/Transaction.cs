using Momo.Common.Actions;
using Momo.Common.Models.Tables;
using Jobs.Models;
using Jobs.Models.DataBase;
using System.Diagnostics;
using System.Text.Json;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Data;
using ExcelDataReader;
using System;

using System.IO;

namespace Jobs.Actions
{
    public interface ITransaction
    {
        void Analyse();
        void Requery();
    }

    public class Transaction : ITransaction
    {


        private readonly ILog Log;
        private readonly IConfiguration Config;
        public Transaction(ILog log, IConfiguration config)
        {
            Config = config;
            Log = log;
        }
        public void Requery()
        {
            try
            {
                var Db = new MomoSwitchDbContext();
                DateTime StartMin = DateTime.Now.AddMinutes(-int.Parse(Config.GetSection("QueryStartMin").Value));
                DateTime EndMin = DateTime.Now.AddMinutes(-int.Parse(Config.GetSection("QueryEndMin").Value));
                var CurrentMinute = DateTime.Now.Minute;

                var Trans = Db.TransactionTb.Where(x => (  x.ResponseCode == "96" || x.ResponseCode == "97") && x.Date > StartMin && x.Date < EndMin).ToList();
                Log.Write("Transaction.Requery", $"Got {Trans.Count} Transaction frequent requery");

                foreach (var Tran in Trans)
                {
                    GetTransaction(Tran.TransactionId);
                }
                Log.Write("Transaction.Requery", $"Finished {Trans.Count} Transaction to requery");


                //If schedule is every2 min, then this is every hr eg 01:02
                if (CurrentMinute == 2)
                {
                    DateTime FiveHourAgo = DateTime.Now.AddHours(-5);
                    var Trans1 = Db.TransactionTb.Where(x => (  x.ResponseCode == "96" || x.ResponseCode == "97") && x.Date < FiveHourAgo).ToList();
                    Log.Write("Transaction.Requery", $"Got {Trans.Count} Transaction 3Hourly requery");

                    foreach (var Tran in Trans1)
                    {
                        GetTransaction(Tran.TransactionId);
                    }
                    Log.Write("Transaction.Requery", $"Finished {Trans.Count} Transaction 3Hourly requery");
                }
            }
            catch (Exception Ex)
            {
                Log.Write("Transaction.Requery", $"Err: {Ex.Message}");
            }
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
                        Rate = (int)Math.Round((decimal)(Succ / BankTran.Count) * 100);

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


        private void GetTransaction(string TransactionId)
        {
            try
            {
                Log.Write("GetTransaction", $"Request {TransactionId}");
                var QueryUrl = Config.GetSection("QueryUrl").Value;

                var Resp = CoralPay.HttpHandler.Invoke.HttpService(new CoralPay.HttpHandler.Models.ServiceRequest
                {
                    DataFormat = CoralPay.HttpHandler.Models.DataFormat.Json,
                    EndPoint = QueryUrl,
                    HttpHeaders = new List<CoralPay.HttpHandler.Models.HttpHeader>
                   {
                        new CoralPay.HttpHandler.Models.HttpHeader
                        {
                             HeaderName="Key",
                             HeaderValue= Config.GetSection("SwitchQueryKey").Value
                        }
                   },
                    IsHttpHeader = true,
                    Method = CoralPay.HttpHandler.Models.HttpVerb.Post,
                    RequestObject = new QueryRequest { TransactionId = TransactionId },
                    //ResponseObject = new QueryResponse(),
                });

                if (Resp.ResponseHeader.ResponseCode == CoralPay.HttpHandler.Models.Status.Successful)
                {
                    Log.Write("GetTransaction", $"Response {Resp.Object}");
                }
                else
                {
                    Log.Write("GetTransaction", $"Conn err: {Resp.ResponseHeader.ResponseMessage} | URL: {QueryUrl}");
                }
            }
            catch (Exception Ex)
            {
                Log.Write("GetTransaction", $"Err: {Ex.Message}");
            }
        }


    }
}