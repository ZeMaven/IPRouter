using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace NipInwardProxy.Models.Nibss
{
    public class TSQuerySingleRequest
    {
        public string SourceInstitutionCode { get; set; }
        public int ChannelCode { get; set; }
        public string SessionID { get; set; }
    }
}