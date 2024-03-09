using System.ComponentModel.DataAnnotations;

namespace Momo.Common.Models.Tables
{
    public class AmountRuleTb
    {
        public int Id { get; set; }
        public decimal AmountA { get; set; }
        public decimal AmountZ { get; set; }
        [StringLength(50)]
        public string Processor { get; set; }
    }
}
