namespace MomoSwitchPortal.Models.ViewModels.Analytics
{
    public class SummaryReportFilterRequest
    {
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string TranType { get; set; }

    }
}
