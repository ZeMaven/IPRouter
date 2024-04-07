using System.ComponentModel.DataAnnotations;

namespace MomoSwitchPortal.Models.ViewModels.Transaction
{
    public class TransactionTableViewModel
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }     
        public string Processor { get; set; }  
        public string TransactionId { get; set; }             
        public string BenefBankCode { get; set; }   
        public string BenefAccountNumber { get; set; }           
        public string SourceBankCode { get; set; }
        public string SourceBankName { get; set; }
        public string BenefBankName { get; set; }
        public string SourceAccountNumber { get; set; }       
        public decimal Amount { get; set; }       
        public string ResponseCode { get; set; }
        public string ResponseMessage { get; set; }


    }
}
