using MomoSwitchPortal.Models.Internals;
using System.ComponentModel.DataAnnotations;

namespace MomoSwitchPortal.Models.ViewModels.Transaction
{
    public class TransactionViewModel
    {
       
        public List<TransactionTableViewModel> Transactions { get; set; }
        public TransactionFilterRequest FilterRequest { get; set; }
        public PaginationMetaData PaginationMetaData { get; set; }

    }
}
