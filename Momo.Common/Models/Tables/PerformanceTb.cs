using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Momo.Common.Models.Tables
{
    [Index(nameof(BankCode))]
    public class PerformanceTb
    {
        public int Id { get; set; }
        [StringLength(50)]
        public string BankCode { get; set; }
        [StringLength(50)]
        public string BankName { get; set; }
        public decimal Rate { get; set;}
        [StringLength(50)]
        public string    Remark { get; set;}
        public DateTime Time { get; set;}

    }
}
