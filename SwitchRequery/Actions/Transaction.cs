using Momo.Common.Actions;
using SwitchRequery.Models;
using SwitchRequery.Models.DataBase;
using System.Diagnostics;
using System.Text.Json;

namespace SwitchRequery.Actions
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
            var Db = new MomoSwitchDbContext();
            DateTime TwoMinutesAgo = DateTime.Now.AddMinutes(-2);
            var Trans = Db.TransactionTb.Where(x => (x.ResponseCode == "01" || x.ResponseCode == "96" || x.ResponseCode == "97") && (x.PaymentDate < TwoMinutesAgo)).ToList();
            Log.Write("Transaction.Requesry", $"Got {Trans.Count} Transaction to requery");
            foreach (var Tran in Trans)
            {
                GetTransaction(Tran.TransactionId);
            }
            Log.Write("Transaction.Requery", $"Got {Trans.Count} Transaction to requery");
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
                    ResponseObject = new QueryResponse(),

                });


                if (Resp.ResponseHeader.ResponseCode == CoralPay.HttpHandler.Models.Status.Successful)
                {
                    Log.Write("GetTransaction", $"Response {JsonSerializer.Serialize((QueryResponse)Resp.Object)}");
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
