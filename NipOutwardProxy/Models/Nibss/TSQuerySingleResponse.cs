using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace NipOutwardProxy.Models.Nibss
{
    public class TSQuerySingleResponse
    {
        public string SourceInstitutionCode { get; set; }
        public int ChannelCode { get; set; }
        public string SessionID { get; set; }
        public string ResponseCode { get; set; }
    }
}