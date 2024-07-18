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

namespace Jobs.Actions.Requery
{
    public interface ITransaction
    {

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

                var Trans = Db.TransactionTb.Where(x => (x.ResponseCode == "01") && x.Date > EndMin && x.Date < StartMin).ToList();
                Log.Write("Transaction.Requery", $"Got {Trans.Count} Transaction frequent requery");

                foreach (var Tran in Trans)
                {
                    GetTransaction(Tran.TransactionId);
                }
                Log.Write("Transaction.Requery", $"Finished {Trans.Count} Transaction to requery");


                //If schedule is every2 min, then this is every hr eg 01:02
                if (CurrentMinute == 2)
                {
                    DateTime _24HourAgo = DateTime.Now.AddHours(-24);
                    var Trans1 = Db.TransactionTb.Where(x => (x.ResponseCode == "01") && x.Date < _24HourAgo).ToList();
                    Log.Write("Transaction.Requery", $"Got {Trans.Count} Transaction Hourly requery");

                    foreach (var Tran in Trans1)
                    {
                        GetTransaction(Tran.TransactionId);
                    }
                    Log.Write("Transaction.Requery", $"Finished {Trans.Count} Transaction Hourly requery");
                }
            }
            catch (Exception Ex)
            {
                Log.Write("Transaction.Requery", $"Err: {Ex.Message}");
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