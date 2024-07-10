namespace RemitaProxy.Models.NameEnq
{
    public class NameEnqResponse
    {
        public string? status { get; set; }
        public string? message { get; set; }
        public NmeEnqData? data { get; set; }
    }

    public class NmeEnqData
    {
        public string? sourceAccount { get; set; }
        public string? sourceAccountName { get; set; }
        public string? sourceBankCode { get; set; }
    }

}
