using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MomoSwitch.Models.Contracts.Specials.Router.Nibss
{
    public class FTAdviceDebitRequest : FTAdviceCreditRequest
    {
        public string MandateReferenceNumber { get; set; }
        public decimal TransactionFee { get; set; }
    }
}