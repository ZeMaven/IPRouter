using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace NipInwardProxy.Models.Nibss
{
    public class FTAdviceDebitResponse: FTAdviceCreditResponse
    {
        public string MandateReferenceNumber { get; set; }
        public decimal TransactionFee { get; set; }       
    }
}