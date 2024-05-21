using MomoSwitchPortal.Models.Internals;

namespace MomoSwitchPortal.Models.ViewModels.Analytics
{
    public class DailyReconcilationViewModel
    {
        public List<DailyReconcilationTableViewModel> DailyReconcilations { get; set; }
        public DailyReconcilationFilterRequest FilterRequest { get; set; }
        public PaginationMetaData PaginationMetaData { get; set; }
        public int StartSerialNumber { get; set; }
    }
}
