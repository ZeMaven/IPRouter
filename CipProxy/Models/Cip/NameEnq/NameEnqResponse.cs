﻿namespace CipProxy.Models.Cip.NameEnq
{    
    public class NameEnqResponse
    {
        public string sessionId { get; set; }
        public string destinationInstitutionId { get; set; }
        public string accountId { get; set; }
        public string accountName { get; set; }
        public string status { get; set; }
        public string responseCode { get; set; }
        public string responseMessage { get; set; }
        public string bvn { get; set; }
        public string kycLevel { get; set; }
        public string accountType { get; set; }
    }

}
