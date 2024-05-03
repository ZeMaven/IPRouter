using MomoSwitchPortal.Models.Internals;

namespace MomoSwitchPortal.Models.ViewModels.Analytics
{
    public class InstitutionPerformanceViewModel
    {
        public List<InstitutionPerformanceTableViewModel> Institutions { get; set; }
        public InstitutionPerformanceFilterRequest FilterRequest { get; set; }
        public PaginationMetaData PaginationMetaData { get; set; } 
        public int StartSerialNumber { get; set; }

    }
}
