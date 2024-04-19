using System.ComponentModel.DataAnnotations;

namespace MomoSwitchPortal.Models.ViewModels.Reports
{
    public class SummaryReport
    {
        public string Date { get; set; }
        [Display(Name = "Total Transactions")]
        public double TotalTransactions { get; set; }
        [Display(Name = "Successful Transactions")]
        public double SuccessfulTransactions { get; set; }
        [Display(Name = "Failed Transactions")]
        public double FailedTransactions { get; set; }
        [Display(Name = "Success %")]
        public string SuccessPercentage { get; set; }
        [Display(Name = "Failed %")]
        public string FailedPercentage { get; set; }
    }
}
