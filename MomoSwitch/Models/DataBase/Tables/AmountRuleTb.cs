using System.ComponentModel.DataAnnotations;

namespace MomoSwitch.Models.DataBase.Tables
{
    public class AmountRuleTb
    {
        public int Id { get; set; }
        public decimal AmountA { get; set; }
        public decimal AmountZ { get; set; }
        [StringLength(50)]
        public string Processor { get; set;}
    }
}
