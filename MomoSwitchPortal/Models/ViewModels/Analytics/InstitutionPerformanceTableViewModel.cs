namespace MomoSwitchPortal.Models.ViewModels.Analytics
{
    public class InstitutionPerformanceTableViewModel
    {
        public string BankCode { get; set; }
        public string BankName { get; set; }
        public decimal SuccessRate { get; set; }
        public DateTime Time { get; set; }
        public string Remark { get; set; }
        public int StartSerialNumber { get; set; }
    }
}
