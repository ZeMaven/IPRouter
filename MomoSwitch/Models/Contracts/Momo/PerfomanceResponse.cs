using System.ComponentModel.DataAnnotations;

namespace MomoSwitch.Models.Contracts.Momo
{
 

    public class Perfomance
    {
        public string BankCode { get; set; }        
        public string BankName { get; set; }
        public decimal Rate { get; set; }       
        public string Remark { get; set; }
        public DateTime Time { get; set; }

    }

}
