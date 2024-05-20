using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Momo.Common.Models
{
    public class ReconDetails
    {
        public string Date { get; set; }
        public string PaymentRef { get; set; }
        public string Processor { get; set; }
        public decimal Amount { get; set; }

        public string EwpSessionId { get; set; }
        public string MsrSessionId { get; set; }
        public string ProcessorSessionId { get; set; }

        public string EwpResponseCode { get; set; }
        public string MsrResponseCode { get; set; }
        public string ProcessorResponseCode { get; set; }
        public string Remarks { get; set; }
    }
}
