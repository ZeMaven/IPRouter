using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MomoSwitch.Models.Contracts.Specials.Nibss
{
    public class FinancialInstitutionListResponse
    {
        public string BatchNumber { get; set; }
        public string DestinationInstitutionCode { get; set; }
        public int ChannelCode { get; set; }
        public int NumberOfRecords { get; set; }
        public string ResponseCode { get; set; }

    }
}