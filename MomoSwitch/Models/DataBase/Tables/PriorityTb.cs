using System.ComponentModel.DataAnnotations;

namespace MomoSwitch.Models.DataBase.Tables
{
    public class PriorityTb
    {
        public int Id { get; set; }
        [StringLength(50)]
        public string Rule { get; set; }
        public int Priority { get; set; }
    }
}
