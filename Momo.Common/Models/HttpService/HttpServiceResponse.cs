using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Momo.Common.Models.HttpService
{
    public class HttpServiceResponse
    {
        public ResponseHeader ResponseHeader { get; set; }
        public string ResponseContent { get; set; }
    }
    public enum Operation { NameEnqury, Transfer, TranQuery,
        Auth
    }
}
