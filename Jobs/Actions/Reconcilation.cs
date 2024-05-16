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

//using static System.Net.WebRequestMethods;


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
            Connect3();
            Connect2();
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







        public static void Connect3()
        {
            string host = "ftps.coralpay.com";
            int port = 9192;
            string username = "msrreconuser";
            string password = "P@ssw0rd1234567890";
            string directoryPath = "/To-Reconcile";
            string fileName = "CIP_Tran.xlsx";
            string localFilePath = "c:/logs/peter.xlsx";
        Label:
            try
            {

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

                    if (client.IsConnected)
                    {
                        client.SetWorkingDirectory(directoryPath);

                        // Download specific file (optional)
                        // client.DownloadFile(fileName, localFilePath);

                        // Get directory listing (if needed for other files)
                        var files = client.GetListing(directoryPath);

                        foreach (var file in files)
                        {
                            if (file.Name == fileName) // Check for specific file
                            {
                                System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);

                                using (var stream = client.OpenRead(file.FullName, FtpDataType.Binary, 0,true))
                                {

                                    var strm = stream;

                                    var rd = new StreamReader(strm);
                                    // Assuming you have a reference to ExcelDataReader
                                    using (var reader = ExcelReaderFactory.CreateOpenXmlReader(strm))
                                    {
                                        DataSet excelData = reader.AsDataSet();
                                        DataTable dataTable = excelData.Tables[0];

                                        // Implement your logic to process data from dataTable
                                        foreach (DataRow row in dataTable.Rows)
                                        {
                                            // Process each row as needed
                                        }
                                    }
                                }
                                break; // Exit loop after downloading the desired file
                            }
                        }
                    }


                }

            }
            catch (Exception ex)
            {
                // Handle FTP specific exceptions
                Console.WriteLine($"FTP Error: {ex.Message}");
                goto Label;
            }

        }




        public static void Connect2()
        {
            string host = "ftps.coralpay.com";
            int port = 9192;
            string username = "msrreconuser";
            string password = "P@ssw0rd1234567890";
            string directoryPath = "/To-Reconcile";
            string fileName = "CIP_Tran.xlsx";
            string localFilePath = "c:/logs/peter.xlsx";

            try
            {
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

                    if (client.IsConnected)
                    {



                        //client.DownloadDirectory("c/logs", directoryPath);
                        client.SetWorkingDirectory($"{directoryPath}");

                        client.IsStillConnected();
                        var Prot = client.SslProtocolActive;

                        var files = client.GetListing(directoryPath);



                        foreach (var file in files)
                        {
                            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);

                            using (var stream = client.OpenRead(file.FullName))
                            {
                                var rd = new StreamReader(stream);
                                stream.Position = 0;


                                //using (var reader = ExcelReaderFactory.CreateReader(stream))
                                //{
                                using (var reader = ExcelReaderFactory.CreateOpenXmlReader(stream))
                                {
                                    DataSet excelData = reader.AsDataSet();
                                    DataTable dataTable = excelData.Tables[0];


                                    int i = 0;
                                    foreach (DataRow row in dataTable.Rows)
                                    {


                                    }









                                }

                            }


                            var ff = file.Name;

                        }

                        var dwnd = client.DownloadFile("c:/Logs/peter.xlsx", directoryPath + "/NIP_Tran.xlsx");



                        string fileContents = System.IO.File.ReadAllText(localFilePath);
                        Console.WriteLine("File Contents:");
                        Console.WriteLine(fileContents);

                    }

                    using (var stream = client.OpenRead(Path.Combine(directoryPath, fileName)))
                    {
                        // Process the file stream (read the file, save it, etc.)
                        // For example:
                        using (var reader = new StreamReader(stream))
                        {
                            string line;
                            while ((line = reader.ReadLine()) != null)
                            {
                                Console.WriteLine(line);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
            Console.WriteLine($"Error: ");
        }
















        public void GetFilePath()
        {

            ServicePointManager.ServerCertificateValidationCallback = new RemoteCertificateValidationCallback((AcceptAllCertificates));

            string ftpServer = "ftps.coralpay.com:9192";
            string ftpUsername = "msrreconuser";
            string ftpPassword = "P@ssw0rd1234567890";
            string remoteDirectory = "/To-Reconcile";

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

            request.Credentials = new NetworkCredential(ftpUsername, ftpPassword);

            try
            {
                var jj = request.GetResponse();
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
}
