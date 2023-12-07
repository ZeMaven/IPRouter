using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace NipInwardProxy.Models.Nibss
{
    public class AmountBlockResponse: AccountBlockResponse
    {
        public string ResponseCode { get; set; }
public decimal Amount { get; set; }
    }
}