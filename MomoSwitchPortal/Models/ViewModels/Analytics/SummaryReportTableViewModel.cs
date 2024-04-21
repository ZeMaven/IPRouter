using System.ComponentModel.DataAnnotations;

namespace MomoSwitchPortal.Models.ViewModels.Analytics
{
    public class SummaryReportTableViewModel
    {
        public string Date { get; set; }    
        public double TotalTransactions { get; set; }        
        public double SuccessfulTransactions { get; set; }        
        public double FailedTransactions { get; set; }        
        public string SuccessPercentage { get; set; }        
        public string FailedPercentage { get; set; }
    }
    
    public class SummaryReportTransactionsMiniViewModel
    {
        public DateTime Date { get; set; }
        public string  ResponseCode { get; set; }
    }
}
