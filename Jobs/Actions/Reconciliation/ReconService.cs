using Jobs.Models.DataBase;
using Momo.Common.Models;
using Momo.Common.Models.Tables;

namespace Jobs.Actions.Reconciliation
{
    public interface IReconService
    {
        void Main();
    }

    public class ReconService : IReconService
    {
        private readonly IExcel _excel;
        private readonly IReconHelpers _helper;
        public ReconService(IReconHelpers helper, IExcel excel)
        {
            _excel = excel;
            _helper = helper;
        }


        public void Main()
        {
            if (_helper.DetermineDayType(DateTime.Now) == Week.Weekend) return;

            var MsrTrans = _helper.GetMsrTransactions();

            var EwpTran00 = _helper.GetCsvTransactions("EWP00");
            var EwpTran09 = _helper.GetCsvTransactions("EWP09");
            var EwpTran01 = _helper.GetCsvTransactions("EWP01");
            var EwpTran = EwpTran00.Concat(EwpTran01).Concat(EwpTran09).ToList();

            var NIPTran = _helper.GetExcelTransactions("NIP", true);
            var CIPTran = _helper.GetExcelTransactions("CIP", false);
            var ProcessorTrans = NIPTran.Concat(CIPTran).ToList();


            var FinalRecon = new List<ReconDetails>();
            var FinalReconTb = new List<DailyReconciliationTb>();


            foreach (var tran in EwpTran)
            {
                var MsrTran = MsrTrans.Where(x => x.PaymentRef == tran.PaymentRef).SingleOrDefault();
                var ProcessorTran = ProcessorTrans.Where(x => x.PaymentRef == tran.PaymentRef).SingleOrDefault();

                FinalRecon.Add(new ReconDetails
                {
                    Date = DateTime.Parse(tran.Date),
                    Amount = tran.Amount,
                    Processor = MsrTran?.Processor ?? ProcessorTran?.Processor,
                    MsrSessionId = MsrTran?.SessionId ?? "NA",
                    EwpSessionId = tran?.SessionId ?? "NA",
                    ProcessorSessionId = ProcessorTran?.SessionId ?? "NA",

                    EwpResponseCode = tran?.ResponseCode,
                    MsrResponseCode = MsrTran?.ResponseCode ?? "NA",
                    ProcessorResponseCode = ProcessorTran?.ResponseCode,


                    Remarks = _helper.CompareResponses(tran?.ResponseCode, MsrTran?.ResponseCode, ProcessorTran?.ResponseCode),

                    PaymentRef = tran.PaymentRef,
                });
            }

            FinalReconTb = FinalRecon.Select(x => new DailyReconciliationTb
            {
                Date = DateTime.Now,
                Amount = x.Amount,
                Processor = x.Processor,
                EwpResponseCode = x.EwpResponseCode,
                EwpSessionId = x.EwpSessionId,
                ProcessorSessionId = x.ProcessorSessionId,
                MsrResponseCode = x.MsrResponseCode,
                MsrSessionId = x.MsrSessionId,
                ProcessorResponseCode = x.ProcessorResponseCode,
                PaymentRef = x.PaymentRef,
                Remarks = x.Remarks
            }).ToList();
            MomoSwitchDbContext db = new();

            db.AddRangeAsync(FinalReconTb);
            db.SaveChangesAsync();

            var FileByte = _excel.Write(FinalRecon, "ReconReport");

            MemoryStream reportStream = new MemoryStream(FileByte);

            _helper.UploadReconciledFile(reportStream);
            _helper.DeleteUsedFiles();
            //Excel.Write(FinalRecon, "ReconReport", ((DateTimeOffset)DateTime.UtcNow).ToUnixTimeSeconds().ToString(), "C:/reports");
        }
    }
}
