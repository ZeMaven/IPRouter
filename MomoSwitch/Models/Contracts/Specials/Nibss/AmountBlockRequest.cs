using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MomoSwitch.Models.Contracts.Specials.Nibss
{
    public class AmountBlockRequest : AccountBlockRequest
    {
        public decimal Amount { get; set; }

    }
}