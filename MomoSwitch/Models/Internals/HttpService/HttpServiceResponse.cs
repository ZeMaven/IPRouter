﻿using System.Net;
using Momo.Common.Models;

namespace MomoSwitch.Models.Internals.HttpService
{
    public class HttpServiceResponse
    {


        public HttpResponseHeader ResponseHeader { get; set; }
        public object Object { get; set; }




    }


    public class HttpResponseHeader : ResponseHeader
    {
        //  [HiddenInput]
        public new HttpServiceStatus ResponseCode { get; set; }
        public HttpStatusCode HttpStatusCode { get; set; }
    }

    public enum HttpServiceStatus { Failed = 0, Error = 3, Success = 1, TimeOut = 2 }

}
;