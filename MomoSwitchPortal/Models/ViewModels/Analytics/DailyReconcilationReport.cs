using System.ComponentModel.DataAnnotations;

namespace MomoSwitchPortal.Models.ViewModels.Analytics
{
    public class DailyReconcilationReport
    {
        public string Date { get; set; }
        [Display(Name = "Payment Ref")]
        public string PaymentRef { get; set; }
        public string Processor { get; set; }        
        public string Remarks { get; set; }
        public decimal Amount { get; set; }
        [Display(Name = "EWP Session Id")]
        public string EwpSessionId { get; set; }
        [Display(Name = "MSR Session Id")]
        public string MsrSessionId { get; set; }
        [Display(Name = "Processor Session Id")]
        public string ProcessorSessionId { get; set; }

        [Display(Name = "EWP Response Code")]
        public string EwpResponseCode { get; set; }
        [Display(Name = "MSR Response Code")]
        public string MsrResponseCode { get; set; }
        [Display(Name = "Processor Response Code")]
        public string ProcessorResponseCode { get; set; }
    }
}
