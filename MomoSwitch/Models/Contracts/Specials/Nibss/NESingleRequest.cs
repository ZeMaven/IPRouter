using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MomoSwitch.Models.Contracts.Specials.Nibss
{
    public class NESingleRequest
    {
        public string SessionID { get; set; }
        public string DestinationInstitutionCode { get; set; }
        public int ChannelCode { get; set; }
        public string AccountNumber { get; set; }

    }
}