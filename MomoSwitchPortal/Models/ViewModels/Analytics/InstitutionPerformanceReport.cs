using System.ComponentModel.DataAnnotations;

namespace MomoSwitchPortal.Models.ViewModels.Analytics
{
    public class InstitutionPerformanceReport
    {
        [Display(Name = "Bank Code")]
        public string BankCode { get; set; }
        [Display(Name = "Bank Name")]
        public string BankName { get; set; }
        [Display(Name = "Success Rate")]
        public string SuccessRate { get; set; }
        public string Remark { get; set; }
        public string Time { get; set; }
    }
}
