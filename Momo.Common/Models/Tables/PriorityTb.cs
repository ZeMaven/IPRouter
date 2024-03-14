using System.ComponentModel.DataAnnotations;

namespace Momo.Common.Models.Tables
{
    public class PriorityTb
    {
        public int Id { get; set; }
        [StringLength(50)]
        public string Rule { get; set; }
        public int Priority { get; set; }
    }
}
