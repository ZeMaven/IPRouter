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
            if (DetermineDayType(DateTime.Now) == Week.Weekend) return;

            var MsrTrans = GetMsrTransactions();

            var EwpTran00 = GetCsvTransactions("EWP00");
            var EwpTran09 = GetCsvTransactions("EWP09");
            var EwpTran01 = GetCsvTransactions("EWP01");
            var EwpTran = EwpTran00.Concat(EwpTran01).Concat(EwpTran09).ToList();

            var NIPTran = GetExcelTransactions("NIP");
            var CIPTran = GetExcelTransactions("CIP");
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


                    Remarks = CompareResponses(tran?.ResponseCode, MsrTran?.ResponseCode, ProcessorTran?.ResponseCode),

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

            var FileByte = Excel.Write(FinalRecon, "ReconReport");

            MemoryStream reportStream = new MemoryStream(FileByte);

            UploadReconciledFile(reportStream);
            DeleteUsedFiles();
            //Excel.Write(FinalRecon, "ReconReport", ((DateTimeOffset)DateTime.UtcNow).ToUnixTimeSeconds().ToString(), "C:/reports");
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
        private List<ProcessorReconData> GetMsrTransactions()
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

        private List<ProcessorReconData> GetCsvTransactions(string Processor)
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



        /// <summary>
        /// Get transaction from the excel file in a setup location
        /// </summary>
        /// <param name="Processor">Processor prifix</param>
        /// <returns></returns>

        private List<ProcessorReconData> GetExcelTransactions(string Processor)
        {
            var stream = GetFileStream(Processor);
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

                    switch (Processor)
                    {
                        case "EWP09":

                            ProcessorTran.Add(new ProcessorReconData
                            {
                                Date = row[2].ToString(),
                                PaymentRef = row[1].ToString(),
                                Amount = Convert.ToDecimal(row[11]),
                                Processor = Processor.Substring(0, 3),
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
                                Processor = Processor.Substring(0, 3),
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
                                Processor = Processor.Substring(0, 3),
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



        private MemoryStream GetFileStream(string processorName)
        {
            try
            {
                string host = Config.GetSection("Host").Value;
                int port = int.Parse(Config.GetSection("Port").Value);
                string username = Config.GetSection("Username1").Value;
                string password = Config.GetSection("Password").Value;
                string directoryPath = Config.GetSection("TransationFilePath").Value;
                string resultPath = Config.GetSection("ReconciliationPath").Value;


                using (var client = new FtpClient(host, port)
                {
                    Config = {
                    EncryptionMode = FtpEncryptionMode.Implicit,
                    ValidateAnyCertificate = true,
                    SslProtocols = System.Security.Authentication.SslProtocols.Tls12,
                    DataConnectionType= FtpDataConnectionType.EPSV
                    },
                    Credentials = new NetworkCredential(username, password)
                })
                {
                    client.Connect();
                    client.SetWorkingDirectory(directoryPath);
                    var files = client.GetListing(directoryPath);
                    var selectedFile = files.FirstOrDefault(x => Path.GetFileNameWithoutExtension(x.Name).StartsWith(processorName));
                    if (selectedFile == null) throw new Exception("No file available for reconcilation");

                    string excelFilePath = Path.Combine(directoryPath, $"{selectedFile.Name}");

                    System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
                    MemoryStream memoryStream = new MemoryStream();
                    client.DownloadStream(memoryStream, $"{directoryPath}/{selectedFile.Name}");

                    client.DeleteFile(excelFilePath);
                    return memoryStream;
                }
            }
            catch (Exception Ex)
            {
                Log.Write("GetFileStream", $"Err: {Ex.Message}");
                return null;
            }
        }





        private void DeleteUsedFiles()
        {
            string host = Config.GetSection("Host").Value;
            int port = int.Parse(Config.GetSection("Port").Value);
            string username = Config.GetSection("Username1").Value;
            string password = Config.GetSection("Password").Value;
            string directoryPath = Config.GetSection("TransationFilePath").Value;
            string resultPath = Config.GetSection("ReconciliationPath").Value;


            using (var client = new FtpClient(host, port)
            {
                Config = {
                    EncryptionMode = FtpEncryptionMode.Implicit,
                    ValidateAnyCertificate = true,
                    SslProtocols = System.Security.Authentication.SslProtocols.Tls12,
                    DataConnectionType= FtpDataConnectionType.EPSV
                    },
                Credentials = new NetworkCredential(username, password)
            })
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






        private void UploadReconciledFile(Stream fileStream)
        {
            string host = Config.GetSection("Host").Value;
            int port = int.Parse(Config.GetSection("Port").Value);
            string username = Config.GetSection("Username1").Value;
            string password = Config.GetSection("Password").Value;
            string directoryPath = Config.GetSection("TransationFilePath").Value;
            string resultPath = Config.GetSection("ReconciliationPath").Value;


            using (var client = new FtpClient(host, port)
            {
                Config = {
                    EncryptionMode = FtpEncryptionMode.Implicit,
                    ValidateAnyCertificate = true,
                    SslProtocols = System.Security.Authentication.SslProtocols.Tls12,
                    DataConnectionType= FtpDataConnectionType.EPSV
                    },
                Credentials = new NetworkCredential(username, password)
            })
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