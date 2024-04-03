using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Momo.Common.Models.Tables
{
    [Index(nameof(BankCode), IsUnique = true)]
    public class BanksTb
    {
        public int Id { get; set; }
        [StringLength(50)]
        public string BankCode { get; set; }
        [StringLength(50)]
        public string BankName { get; set; }
    }
}
