using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml.Serialization;

namespace NipInwardProxy.Models.Nibss
{
    public class FinancialInstitutionListRequest
    {
        public Header Header { get; set; }      
        public List<Record> Record { get; set; }
    }


    public class Header
    {
        public string BatchNumber { get; set; }
        public int NumberOfRecords { get; set; }
        public int ChannelCode { get; set; }
        public string TransactionLocation { get; set; }
    }

    public class Record
    {
        public string InstitutionCode { get; set; }
        public string InstitutionName { get; set; }
        public string Category { get; set; }
    }


}