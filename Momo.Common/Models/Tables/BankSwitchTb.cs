using System.ComponentModel.DataAnnotations;

namespace Momo.Common.Models.Tables
{
    public class BankSwitchTb
    {

        public int Id { get; set; }
        [StringLength(50)]
        public string BankCode { get; set; }
        [StringLength(50)]
        public string Processor { get; set; }

    }
}
