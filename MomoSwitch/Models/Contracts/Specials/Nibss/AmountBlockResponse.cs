using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MomoSwitch.Models.Contracts.Specials.Nibss
{
    public class AmountBlockResponse : AccountBlockResponse
    {
        public new string ResponseCode { get; set; }
        public decimal Amount { get; set; }
    }
}