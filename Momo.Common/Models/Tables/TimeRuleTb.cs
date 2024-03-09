using System.ComponentModel.DataAnnotations;

namespace Momo.Common.Models.Tables
{
    public class TimeRuleTb
    {
        public int Id { get; set; }
        [StringLength(50)]
        public string TimeA { get; set; }//02:30
        [StringLength(50)]
        public string TimeZ { get; set; }//17:00
        [StringLength(50)]
        public string Processor { get; set; }
    }
}
