using System.ComponentModel.DataAnnotations;

namespace MomoSwitch.Models.DataBase.Tables
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
