using Microsoft.EntityFrameworkCore.Metadata;

namespace MomoSwitch.Models
{
    public class Staging
    {


        //CIP NameEnq Request




        public class CipNEReq
        {
            public string sessionId { get; set; }
            public string destinationInstitutionId { get; set; }
            public string accountId { get; set; }
        }

        public class ProxyNEReq
        {
            public string TransactionId { get; set; }
            public string DestinationBankCode { get; set; }
            public string SourceBankCode { get; set; }//remove. but momo is to be set up on switch, proxy will handle this
            public string Accountnumber { get; set; }
        }


        public class ProxyNEResp
        {
            public string SessionId { get; set; }
            public string TransactionId { get; set; }
            public string DestinationBankCode { get; set; }
            public string SourceBankCode { get; set; }
            public string AccountNumber { get; set; }
            public string Bvn { get; set; }
            public string AccountName { get; set; }
            public string ResponseCode { get; set; }
            public string KycLevel { get; set; }

        }

        //''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''

        //Tran Query

        class TQ
        {
            public string TransactionId { get; set; }
        }

        class TQResp
        {
            public string TransactionId { get; set; }
            public string DestinationBankCode { get; set; }
            public string SourceBankCode { get; set; }
            public string SessionId { get; set; }
            public string ResponseCode { get; set; }
            public string ResponseMessage { get; set; }
        }


        class CreditRequest
        {
            public string TransactionId { get; set; }
            public decimal Amount { get; set; }
            public string BenefAccountName { get; set; }
            public string BenefAccountNumber { get; set; }
            public string BenefBvn { get; set; }
            public string BenefKycLevel { get; set; }
            public string DestinationBankCode { get; set; }
            public string Narration { get; set; }
            public string SourceBankCode { get; set; }
            public string SourceAccountName { get; set; }
            public string SourceAccountNumber { get; set; }
            public string NameEnquiryRef { get; set; }
        }



        class CreditResponse
        {
            public string TransactionId { get; set; }
            public string SessionId { get; set; }
            public decimal Amount { get; set; }
            public string BenefAccountName { get; set; }
            public string BenefAccountNumber { get; set; }
            public string BenefBvn { get; set; }
            public string BenefKycLevel { get; set; }
            public string DestinationBankCode { get; set; }
            public string Narration { get; set; }
            public string SourceBankCode { get; set; }
            public string SourceAccountName { get; set; }
            public string SourceAccountNumber { get; set; }
            public string NameEnquiryRef { get; set; }
            public string ResponseCode { get; set; }
            public string ResponseMessage { get; set; }
        }
    }
}
