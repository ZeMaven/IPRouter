using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Momo.Common.Models
{
    public class ReconDetails
    {
        public DateTime Date { get; set; }
        [StringLength(50)]
        public string PaymentRef { get; set; }
        [StringLength(50)]
        public string Processor { get; set; }
        public decimal Amount { get; set; }
        [StringLength(50)]

        public string EwpSessionId { get; set; }
        [StringLength(50)]
        public string MsrSessionId { get; set; }
        [StringLength(50)]
        public string ProcessorSessionId { get; set; }
        [StringLength(50)]

        public string EwpResponseCode { get; set; }
        [StringLength(50)]
        public string MsrResponseCode { get; set; }
        [StringLength(50)]
        public string ProcessorResponseCode { get; set; }
        [StringLength(200)]
        public string Remarks { get; set; }
    }
}
