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
using FluentFTP;
using System;
using System.Reflection.Emit;
using Momo.Common.Models;
using Momo.Common.Models.Tables;
using CsvHelper;
using System.Globalization;
using CsvHelper.Configuration.Attributes;

namespace Jobs.Actions.Reconciliation
{
    public interface IReconHelpers
    {
        string CompareResponses(string var1, string var2, string var3);
        void DeleteUsedFiles();
        Week DetermineDayType(DateTime date);
        List<ProcessorReconData> ExtractExcelData(MemoryStream stream, string processor);
        List<ProcessorReconData> GetCsvTransactions(string Processor);
        List<ProcessorReconData> GetExcelTransactions(string processor, bool isMultipleFiles);
        MemoryStream GetFileStream(string processorName);
        List<MemoryStream> GetFileStreamList(string processorName);
        List<ProcessorReconData> GetMsrTransactions();
        void UploadReconciledFile(Stream fileStream);
    }

    public class ReconHelpers : IReconHelpers
    {
        private readonly ILog Log;
        private readonly IConfiguration Config;
        private readonly IExcel Excel;

        public ReconHelpers(IConfiguration config, ILog log, IExcel excel)
        {
            Log = log;
            Config = config;
            Excel = excel;
        }

        public Week DetermineDayType(DateTime date)
        {
            // Get the day of the week for the given date
            DayOfWeek dayOfWeek = date.DayOfWeek;

            // Check if the day is Saturday or Sunday
            if (dayOfWeek == DayOfWeek.Saturday || dayOfWeek == DayOfWeek.Sunday)
            {
                return Week.Weekend;
            }
            else if (dayOfWeek == DayOfWeek.Monday)
            {
                return Week.Monday;
            }
            else
            {
                return Week.Weekday;
            }
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
        public List<ProcessorReconData> GetMsrTransactions()
        {
            var Db = new MomoSwitchDbContext();
            DateTime yesterday;
            if (DetermineDayType(DateTime.Now) == Week.Monday)
                yesterday = DateTime.Now.AddDays(-3).Date;
            else
                yesterday = DateTime.Now.AddDays(-1).Date;


            var MsrData = from tb in Db.TransactionTb where tb.Date.Date >= yesterday && tb.Date.Date != DateTime.Today select new { tb.TransactionId, tb.Date, tb.ResponseCode, tb.SessionId, tb.PaymentReference, tb.Processor, tb.Amount };

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

        public List<ProcessorReconData> GetCsvTransactions(string Processor)
        {
            int i = 0;

            try
            {
                var stream = GetFileStream(Processor);
                stream.Position = 0;
                List<ProcessorReconData> ProcessorTran = new();
                using (var reader = new StreamReader(stream))
                {

                    using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
                    {
                        var records = csv.GetRecords<EwpTranDetails>();

                        var rows = from tb in records select new { tb.SessionId, tb.SuccAmount, tb.ExternalId, tb.FailedAmount, tb.PendingAmount, tb.Date };

                        foreach (var row in rows)
                        {
                            switch (Processor)
                            {
                                case "EWP09":

                                    ProcessorTran.Add(new ProcessorReconData
                                    {
                                        Date = row.Date,
                                        PaymentRef = row.ExternalId,
                                        Amount = Convert.ToDecimal(row.PendingAmount),
                                        Processor = Processor.Substring(0, 3),
                                        ResponseCode = "09",
                                        SessionId = row.SessionId //change later

                                    });

                                    break;
                                case "EWP00":

                                    ProcessorTran.Add(new ProcessorReconData
                                    {
                                        Date = row.Date,
                                        PaymentRef = row.ExternalId,
                                        Amount = Convert.ToDecimal(row.SuccAmount),
                                        Processor = Processor.Substring(0, 3),
                                        ResponseCode = "00",
                                        SessionId = row.SessionId //change later

                                    });

                                    break;
                                case "EWP01":
                                    ProcessorTran.Add(new ProcessorReconData
                                    {
                                        Date = row.Date,
                                        PaymentRef = row.ExternalId,
                                        Amount = Convert.ToDecimal(row.FailedAmount),
                                        Processor = Processor.Substring(0, 3),
                                        ResponseCode = "01",
                                        SessionId = row.SessionId //change later

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
            catch (Exception Ex)
            {
                Log.Write("GetCsvTransactions", $"Err: {Ex.Message} {i}");
                return new List<ProcessorReconData>();
            }
        }

        public List<ProcessorReconData> ExtractExcelData(MemoryStream stream, string processor)
        {
            List<ProcessorReconData> ProcessorTran = new();
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

                    switch (processor)
                    {
                        case "EWP09":

                            ProcessorTran.Add(new ProcessorReconData
                            {
                                Date = row[2].ToString(),
                                PaymentRef = row[1].ToString(),
                                Amount = Convert.ToDecimal(row[11]),
                                Processor = processor.Substring(0, 3),
                                ResponseCode = "09",
                                SessionId = row[0].ToString() //change later

                            });

                            break;
                        case "EWP00":

                            ProcessorTran.Add(new ProcessorReconData
                            {
                                Date = row[2].ToString(),
                                PaymentRef = row[1].ToString(),
                                Amount = Convert.ToDecimal(row[36]),
                                Processor = processor.Substring(0, 3),
                                ResponseCode = "00",
                                SessionId = row[0].ToString() //change later

                            });

                            break;
                        case "EWP01":

                            ProcessorTran.Add(new ProcessorReconData
                            {
                                Date = row[3].ToString(),
                                PaymentRef = row[2].ToString(),
                                Amount = Convert.ToDecimal(row[20]),
                                Processor = processor.Substring(0, 3),
                                ResponseCode = "01",
                                SessionId = row[0].ToString() //change later

                            });

                            break;
                        case "EWP":

                            ProcessorTran.Add(new ProcessorReconData
                            {
                                Date = row[2].ToString(),
                                PaymentRef = row[1].ToString(),
                                Amount = Convert.ToDecimal(row[12]),
                                Processor = processor,
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
                                Processor = processor,
                                ResponseCode = row[5].ToString(),
                                SessionId = row[2].ToString()

                            });

                            break;
                        case "CIP":

                            ProcessorTran.Add(new ProcessorReconData
                            {
                                Amount = Convert.ToDecimal(row[12]),
                                PaymentRef = row[3].ToString(),
                                Processor = processor,
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
            return ProcessorTran;

        }

        /// <summary>
        /// Get transaction from the excel file in a setup location
        /// </summary>
        /// <param name="processor">Processor prifix</param>
        /// <returns></returns>

        public List<ProcessorReconData> GetExcelTransactions(string processor, bool isMultipleFiles)
        {
            List<ProcessorReconData> ProcessorTran = new();

            if (isMultipleFiles)
            {
                var streamList = GetFileStreamList(processor);
                foreach (var stream in streamList)
                {
                    var thisTran = ExtractExcelData(stream, processor);
                    ProcessorTran = ProcessorTran.Concat(thisTran).ToList();
                }
            }
            else
            {
                var stream = GetFileStream(processor);
                ProcessorTran = ExtractExcelData(stream, processor);
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


        private FtpClient ClientFtp()
        {
            string host = Config.GetSection("Host").Value;
            int port = int.Parse(Config.GetSection("Port").Value);
            string username = Config.GetSection("Username1").Value;
            string password = Config.GetSection("Password").Value;
              
            var client = new FtpClient(host, port)
            {
                Config = {
                    EncryptionMode = FtpEncryptionMode.Implicit,
                    ValidateAnyCertificate = true,
                    SslProtocols = SslProtocols.Tls12,
                    DataConnectionType= FtpDataConnectionType.EPSV
                    },
                Credentials = new NetworkCredential(username, password)
            };

            return client;
        }

        public MemoryStream GetFileStream(string processorName)
        {
            try
            {              
                string directoryPath = Config.GetSection("TransationFilePath").Value;                
                using (var ftp = ClientFtp())
                {                 
                    ftp.Connect();
                    ftp.SetWorkingDirectory(directoryPath);
                    var files = ftp.GetListing(directoryPath);
                    var selectedFile = files.FirstOrDefault(x => Path.GetFileNameWithoutExtension(x.Name).StartsWith(processorName));
                    if (selectedFile == null) throw new Exception("No file available for reconcilation");

                    string excelFilePath = Path.Combine(directoryPath, $"{selectedFile.Name}");

                    System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
                    MemoryStream memoryStream = new MemoryStream();
                    ftp.DownloadStream(memoryStream, $"{directoryPath}/{selectedFile.Name}");
             
                    return memoryStream;
                }

            }
            catch (Exception Ex)
            {
                Log.Write("GetFileStream", $"Err: {Ex.Message}");
                return null;
            }
        }
        public List<MemoryStream> GetFileStreamList(string processorName)
        {
            try
            {           
                string directoryPath = Config.GetSection("TransationFilePath").Value;               
                using (var ftp = ClientFtp())               
                {
                    ftp.Connect();
                    ftp.SetWorkingDirectory(directoryPath);
                    var files = ftp.GetListing(directoryPath);
                    var fileList = files.Where(x => Path.GetFileNameWithoutExtension(x.Name).StartsWith(processorName)).ToList();
                    if (fileList.Count < 1) throw new Exception("No file available for reconcilation");

                    List<MemoryStream> memoryStreamList = new();
                    foreach (var file in fileList)
                    {
                        string excelFilePath = Path.Combine(directoryPath, $"{file.Name}");
                        System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
                        MemoryStream memoryStream = new MemoryStream();
                        ftp.DownloadStream(memoryStream, excelFilePath);
                        memoryStreamList.Add(memoryStream);
                    }
                    return memoryStreamList;
                }
            }
            catch (Exception Ex)
            {
                Log.Write("GetFileStream", $"Err: {Ex.Message}");
                return null;
            }
        }

        public void DeleteUsedFiles()
        {
            string password = Config.GetSection("Password").Value;
            string directoryPath = Config.GetSection("TransationFilePath").Value;
             

            using (var client = ClientFtp())
           
            {
                client.Connect();
                client.SetWorkingDirectory(directoryPath);
                var files = client.GetListing(directoryPath);
                foreach (var file in files)
                {
                    string excelFilePath = Path.Combine(directoryPath, $"{file.Name}");
                    client.DeleteFile(excelFilePath);
                }
            }
        }

        public void UploadReconciledFile(Stream fileStream)
        {           
            string resultPath = Config.GetSection("ReconciliationPath").Value;
            using (var client = ClientFtp())
            { 
                client.Connect();
                client.SetWorkingDirectory(resultPath);


                System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
                //MemoryStream memoryStream = new MemoryStream(fileStream);

                var fileName = $"Reconciled-{DateTime.Now.ToString("ddMMMyyHHmmss")}.xlsx";
                client.UploadStream(fileStream, $"{resultPath}/{fileName}");
            }
        }




        static void DownloadFile(FtpClient ftpClient, string remoteDirectory, string fileName)
        {
            string remoteFilePath = $"{remoteDirectory}/{fileName}";
            string localFilePath = Path.Combine(Environment.CurrentDirectory, fileName);

            try
            {
                // Download the file from the FTP server
                ftpClient.DownloadFile(localFilePath, remoteFilePath);
                Console.WriteLine($"Downloaded file: {fileName}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while downloading {fileName}: {ex.Message}");
            }
        }

    }

    public enum Week
    {
        Monday = 0,
        Weekday = 1,
        Weekend = 2
    }
}