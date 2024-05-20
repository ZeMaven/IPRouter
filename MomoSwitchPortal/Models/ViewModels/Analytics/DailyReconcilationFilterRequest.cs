namespace MomoSwitchPortal.Models.ViewModels.Analytics
{
    public class DailyReconcilationFilterRequest
    {
        public string PaymentRef { get; set; }

        public string Processor { get; set; }
        public string Remarks { get; set; }
        public string ProcessorSessionId { get; set; }
        public string MsrSessionId { get; set; }
        public string EwpSessionId { get; set; }

    }
}
