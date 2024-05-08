using ExcelDataReader;
using Jobs.Models;
using Jobs.Models.DataBase;
using Microsoft.Identity.Client;
using Momo.Common.Actions;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Net;
using System.Net.Security;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;


namespace Jobs.Actions
{
    public interface IReconcilation
    {
        void Main();
    }

    public class Reconcilation : IReconcilation
    {
        private readonly ILog Log;
        private readonly IConfiguration Config;
        private readonly IExcel Excel;

        public Reconcilation(IConfiguration config, ILog log, IExcel excel)
        {
            Log = log;
            Config = config;
            Excel = excel;

        }




        public void Main()
        {

            GetFilePath();
            var MsrTrans = GetMsrTransactions();

            var EwpTran = GetExcelTransactions("EWP");

            var NIPTran = GetExcelTransactions("NIP");

            var CIPTran = GetExcelTransactions("CIP");


            var ProcessorTrans = NIPTran.Concat(CIPTran).ToList();


            var FinalRecon = new List<FinalReconData>();

            foreach (var tran in EwpTran)
            {
                var MsrTran = MsrTrans.Where(x => x.PaymentRef == tran.PaymentRef).SingleOrDefault();
                var ProcessorTran = ProcessorTrans.Where(x => x.PaymentRef == tran.PaymentRef).SingleOrDefault();

                FinalRecon.Add(new FinalReconData
                {
                    Date = tran.Date,
                    Amount = tran.Amount,
                    Processor = MsrTran?.Processor ?? ProcessorTran?.Processor,
                    MsrSessionId = MsrTran?.SessionId ?? "NA",
                    EwpSessionId = tran?.SessionId ?? "NA",
                    ProcessorSessionId = ProcessorTran?.SessionId ?? "NA",

                    EwpResponseCode = tran?.ResponseCode,
                    MsrResponseCode = MsrTran?.ResponseCode ?? "NA",
                    ProcessorResponseCode = ProcessorTran?.ResponseCode,


                    Remarks = CompareResponses(tran?.ResponseCode, MsrTran?.ResponseCode, ProcessorTran?.ResponseCode),

                    PaymentRef = tran.PaymentRef,
                });
            }


            Excel.Write(FinalRecon, "ReconReport", ((DateTimeOffset)DateTime.UtcNow).ToUnixTimeSeconds().ToString(), "C:/reports");

        }





        public string CompareResponses(string var1, string var2, string var3)
        {
            if (var1 == var2 && var2 == var3)
                return "OK";
            else
                return "Investigate";
        }



        /// <summary>
        /// Yesterday transactions
        /// </summary>
        /// <returns></returns>
        private List<ProcessorReconData> GetMsrTransactions()
        {
            var Db = new MomoSwitchDbContext();
            var yesterday = DateTime.Now.AddDays(-1).Date;
            var MsrData = from tb in Db.TransactionTb where tb.Date.Date == yesterday select new { tb.TransactionId, tb.Date, tb.ResponseCode, tb.SessionId, tb.PaymentReference, tb.Processor, tb.Amount };

            var MsrTran = MsrData.ToList();
            var Processors = MsrTran.Select(x => x.Processor).Distinct().ToList();

            List<ProcessorReconData> Tran = MsrTran.Select(x => new ProcessorReconData
            {
                ResponseCode = x.ResponseCode,
                //Date = x.Date,
                SessionId = x.SessionId,
                Amount = x.Amount,
                Processor = x.Processor,
                PaymentRef = x.TransactionId,
            }).ToList();

            return Tran;
        }

        /// <summary>
        /// Get transaction from the excel file in a setup location
        /// </summary>
        /// <param name="Processor">Processor prifix</param>
        /// <returns></returns>

        private List<ProcessorReconData> GetExcelTransactions(string Processor)
        {

            GetFilePath();
            var EwpFilePath = GetfilePath(Processor);
            List<ProcessorReconData> ProcessorTran = new();
            using (var stream = File.Open(EwpFilePath, FileMode.Open, FileAccess.Read))
            {
                using (var reader = ExcelReaderFactory.CreateReader(stream))
                {
                    // Read Excel data
                    DataSet excelData = reader.AsDataSet();
                    DataTable dataTable = excelData.Tables[0];


                    int i = 0;
                    foreach (DataRow row in dataTable.Rows)
                    {
                        i++;
                        if (i == 1) continue;

                        switch (Processor)
                        {
                            case "EWP":

                                ProcessorTran.Add(new ProcessorReconData
                                {
                                    Date = row[2].ToString(),
                                    PaymentRef = row[1].ToString(),
                                    Amount = Convert.ToDecimal(row[12]),
                                    Processor = Processor,
                                    ResponseCode = row[3].ToString(),
                                    SessionId = row[0].ToString() //change later

                                });

                                break;
                            case "NIP":

                                ProcessorTran.Add(new ProcessorReconData
                                {
                                    Amount = Convert.ToDecimal(row[6]),
                                    // Date = Convert.ToDateTime(row["Date"]),
                                    PaymentRef = row[14].ToString(),
                                    Processor = Processor,
                                    ResponseCode = row[5].ToString(),
                                    SessionId = row[2].ToString()

                                });

                                break;
                            case "CIP":

                                ProcessorTran.Add(new ProcessorReconData
                                {
                                    Amount = Convert.ToDecimal(row[12]),
                                    PaymentRef = row[3].ToString(),
                                    Processor = Processor,
                                    ResponseCode = row[5].ToString(),
                                    SessionId = row[2].ToString()

                                });

                                break;

                            default:
                                //log
                                ProcessorTran.Add(new ProcessorReconData
                                {
                                });
                                break;


                        }




                    }
                }
            }
            return ProcessorTran;
        }




        private string GetfilePath(string ProcessorName)
        {
            string directoryPath = Config.GetSection("ReconFilePath").Value;
            string[] files = Directory.GetFiles(directoryPath, "*.xlsx");
            var EwpFilename = files.FirstOrDefault(x => Path.GetFileNameWithoutExtension(x).StartsWith(ProcessorName));
            string excelFilePath = Path.Combine(directoryPath, $"{EwpFilename}");
            return excelFilePath;
        }
        public static bool AcceptAllCertificates(
        object sender, X509Certificate certificate, X509Chain chain,
        SslPolicyErrors sslPolicyErrors)
        {
            // Always accept
            return true;
        }


      


            public void GetFilePath()
        {

            ServicePointManager.ServerCertificateValidationCallback = new RemoteCertificateValidationCallback((AcceptAllCertificates));

            string ftpServer = "ftps.coralpay.com:9192";
            string ftpUsername = "msrreconuser";
            string ftpPassword = "P@ssw0rd1234567890";
            string remoteDirectory = "reconciled";

            // FTP URI
            string ftpUri = $"ftp://{ftpServer}/{remoteDirectory}";

            // Create FTP request
            FtpWebRequest request = WebRequest.Create(ftpUri) as FtpWebRequest;
            request.Method = WebRequestMethods.Ftp.ListDirectoryDetails;           
            request.UseBinary = true;
            request.EnableSsl = true;
            request.UsePassive = true;
            request.KeepAlive = true;
            request.ImpersonationLevel = System.Security.Principal.TokenImpersonationLevel.Identification;
            ServicePoint sp = request.ServicePoint;
             

            sp.ConnectionLimit = 1;

            request.Credentials = new NetworkCredential(ftpUsername, ftpPassword,"coralpay.com");

            try
            {
                var jj=request.GetResponse();
                // Get the response
                using (FtpWebResponse response = (FtpWebResponse)request.GetResponse())
                {
                    Stream responseStream = response.GetResponseStream();
                    StreamReader reader = new StreamReader(responseStream);

                    // Read the response
                    Console.WriteLine(reader.ReadToEnd());

                    // Close streams
                    reader.Close();
                    response.Close();
                }

            }
            catch (WebException ex)
            {
                // Handle exceptions
                Console.WriteLine($"Error: {ex.Message}");
            }



        }
    }
}
