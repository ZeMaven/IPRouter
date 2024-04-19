using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using static System.Runtime.InteropServices.JavaScript.JSType;

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
