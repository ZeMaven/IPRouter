using System.Net;

namespace Jobs.Actions
{
    public class Ftp
    {




        static string[] GetTxtFilesFromFTP(string ftpServerUrl, string username, string password, string remoteDirectoryPath, string fileExtenssion)
        {
            try
            {
                // Create a request to list the directory contents
                FtpWebRequest request = (FtpWebRequest)WebRequest.Create($"{ftpServerUrl}/{remoteDirectoryPath}");
                request.Method = WebRequestMethods.Ftp.ListDirectory;
                request.Credentials = new NetworkCredential(username, password);

                // Get the response
                using (FtpWebResponse response = (FtpWebResponse)request.GetResponse())
                {
                    // Read the response stream
                    using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                    {
                        // Parse the directory listing
                        string directoryListing = reader.ReadToEnd();
                        string[] files = directoryListing.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);

                        // Filter for text files
                        string[] txtFiles = Array.FindAll(files, f => f.EndsWith($".{fileExtenssion}"));

                        // Return the list of text files
                        return txtFiles;
                    }
                }
            }
            catch (WebException ex)
            {
                // Handle exception (e.g., directory not found, access denied)
                Console.WriteLine($"Error retrieving directory listing: {ex.Message}");
                return new string[0];
            }
        }






        static Stream DownloadStreamFromFTP(string ftpServerUrl, string username, string password, string remoteFilePath)
        {
            // Create a WebClient instance to download the file
            WebClient ftpClient = new WebClient();

            // Set credentials for FTP access
            ftpClient.Credentials = new NetworkCredential(username, password);

            try
            {
                // Download the file as a stream
                Stream fileStream = ftpClient.OpenRead($"{ftpServerUrl}/{remoteFilePath}");

                // File downloaded successfully
                Console.WriteLine("Excel file downloaded successfully as a stream.");

                // Return the stream
                return fileStream;
            }
            catch (WebException ex)
            {
                // Handle exception (e.g., file not found, access denied)
                Console.WriteLine($"Error downloading Excel file: {ex.Message}");
                return null;
            }
            finally
            {
                // Dispose the WebClient
                ftpClient.Dispose();
            }
        }





        static void DownloadFromFTP(string ftpServerUrl, string username, string password, string remoteFilePath, string localFilePath)
        {
            // Create a WebClient instance to download the file
            using (WebClient ftpClient = new WebClient())
            {
                // Set credentials for FTP access
                ftpClient.Credentials = new NetworkCredential(username, password);

                try
                {
                    // Download the file
                    ftpClient.DownloadFile($"{ftpServerUrl}/{remoteFilePath}", localFilePath);

                    // File downloaded successfully
                    Console.WriteLine("Excel file downloaded successfully.");
                }
                catch (WebException ex)
                {
                    // Handle exception (e.g., file not found, access denied)
                    Console.WriteLine($"Error downloading Excel file: {ex.Message}");
                }
            }
        }
    }
}


