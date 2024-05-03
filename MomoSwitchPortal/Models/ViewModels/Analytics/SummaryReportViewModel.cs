using MomoSwitchPortal.Models.Internals;
using MomoSwitchPortal.Models.ViewModels.Transaction;

namespace MomoSwitchPortal.Models.ViewModels.Analytics
{
    public class SummaryReportViewModel
    {
        public List<SummaryReportTableViewModel> Transactions { get; set; }
        public SummaryReportFilterRequest FilterRequest { get; set; }
        public PaginationMetaData PaginationMetaData { get; set; }
        public int StartSerialNumber { get; set; }
    }
}
