using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Momo.Common.Models.Tables
{
    public class SwitchTb
    {
        public int Id { get; set; }
        [StringLength(50)]
        public string Processor { get; set; }
        [StringLength(250)]
        public string NameEnquiryUrl { get; set; }
        [StringLength(250)]
        public string TransferUrl { get; set; }
        [StringLength(250)]
        public string TranQueryUrl { get; set; }
        public bool IsDefault { get; set; }
        public bool IsActive { get; set; }

        [DefaultValue(0)]
        public decimal DailyLimit { get; set; }

    }
}
