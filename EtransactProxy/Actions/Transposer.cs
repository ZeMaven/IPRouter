﻿using Momo.Common.Models;

namespace EtransactProxy.Actions
{
    public interface ITransposer
    {
        ResponseHeader Response(string ResponseCode);
    }

    public class Transposer : ITransposer
    {



        public ResponseHeader Response(string ResponseCode)
        {
            switch (ResponseCode)
            {
                case "-1":
                    return new ResponseHeader()
                    {
                        ResponseCode = "97",
                        ResponseMessage = "Transaction timed out."
                    };
                case "0":
                    return new ResponseHeader()
                    {
                        ResponseCode = "00",
                        ResponseMessage = "Transaction Successful"
                    };
                case "1":
                    return new ResponseHeader()
                    {
                        ResponseCode = "01",
                        ResponseMessage = "Destination Card Not Found"
                    };
                case "2":
                    return new ResponseHeader()
                    {
                        ResponseCode = "02",
                        ResponseMessage = "Card Number Not Found"
                    };
                case "3":
                    return new ResponseHeader()
                    {
                        ResponseCode = "03",
                        ResponseMessage = "Invalid Card PIN"
                    };
                case "4":
                    return new ResponseHeader()
                    {
                        ResponseCode = "04",
                        ResponseMessage = "Card Expiration Incorrect"
                    };
                case "5":
                    return new ResponseHeader()
                    {
                        ResponseCode = "05",
                        ResponseMessage = "Insufficient balance"
                    };
                case "6":
                    return new ResponseHeader()
                    {
                        ResponseCode = "06",
                        ResponseMessage = "Spending Limit Exceeded"
                    };
                case "7":
                    return new ResponseHeader()
                    {
                        ResponseCode = "97",
                        ResponseMessage = "Internal System Error Occurred please contact the service provider"
                    };
                case "8":
                    return new ResponseHeader()
                    {
                        ResponseCode = "97",
                        ResponseMessage = "Financial Institution Cannot authorize transaction Please try later"
                    };
                case "9":
                    return new ResponseHeader()
                    {
                        ResponseCode = "97",
                        ResponseMessage = "PIN tries Exceeded"
                    };
                case "10":
                    return new ResponseHeader()
                    {
                        ResponseCode = "10",
                        ResponseMessage = "Card has been locked"
                    };
                case "11":
                    return new ResponseHeader()
                    {
                        ResponseCode = "11",
                        ResponseMessage = "Invalid Terminal Id"
                    };
                case "12":
                    return new ResponseHeader()
                    {
                        ResponseCode = "12",
                        ResponseMessage = "Payment Timeout"
                    };
                case "13":
                    return new ResponseHeader()
                    {
                        ResponseCode = "13",
                        ResponseMessage = "Destination card has been locked"
                    };
                case "14":
                    return new ResponseHeader()
                    {
                        ResponseCode = "14",
                        ResponseMessage = "Card has expired"
                    };
                case "15":
                    return new ResponseHeader()
                    {
                        ResponseCode = "15",
                        ResponseMessage = "PIN change required"
                    };
                case "16":
                    return new ResponseHeader()
                    {
                        ResponseCode = "16",
                        ResponseMessage = "Invalid Amount"
                    };
                case "17":
                    return new ResponseHeader()
                    {
                        ResponseCode = "17",
                        ResponseMessage = "Card has been disabled"
                    };
                case "18":
                    return new ResponseHeader()
                    {
                        ResponseCode = "18",
                        ResponseMessage = "Unable to credit destination account request will be rolled back"
                    };
                case "19":
                    return new ResponseHeader()
                    {
                        ResponseCode = "19",
                        ResponseMessage = "Transaction not permitted on terminal"
                    };
                case "20":
                    return new ResponseHeader()
                    {
                        ResponseCode = "20",
                        ResponseMessage = "Exceeds withdrawal frequency"
                    };
                case "21":
                    return new ResponseHeader()
                    {
                        ResponseCode = "21",
                        ResponseMessage = "Destination Card has Expired"
                    };
                case "22":
                    return new ResponseHeader()
                    {
                        ResponseCode = "22",
                        ResponseMessage = "Destination Card Disabled"
                    };
                case "23":
                    return new ResponseHeader()
                    {
                        ResponseCode = "23",
                        ResponseMessage = "Source Card Disabled"
                    };
                case "24":
                    return new ResponseHeader()
                    {
                        ResponseCode = "24",
                        ResponseMessage = "Invalid Bank Account"
                    };
                case "26":
                    return new ResponseHeader()
                    {
                        ResponseCode = "26",
                        ResponseMessage = "Request/Function not supported Bank TSS not Funded"
                    };
                case "28":
                    return new ResponseHeader()
                    {
                        ResponseCode = "28",
                        ResponseMessage = "Transaction with this amount, destination account has already been approved today."
                    };
                case "29":
                    return new ResponseHeader()
                    {
                        ResponseCode = "29",
                        ResponseMessage = "Bank Account Restricted"
                    };
                case "30":
                    return new ResponseHeader()
                    {
                        ResponseCode = "30",
                        ResponseMessage = "Pending transaction, upon confirmation from bank."
                    };
                case "31":
                    return new ResponseHeader()
                    {
                        ResponseCode = "31",
                        ResponseMessage = "Transaction status unknown, contact eTranzact after T+1 for status."
                    };
                case "92":
                    return new ResponseHeader()
                    {
                        ResponseCode = "92",
                        ResponseMessage = "No Route to Issuer/Bank"
                    };
                case "99":
                    return new ResponseHeader()
                    {
                        ResponseCode = "99",
                        ResponseMessage = "Transaction Failed"
                    };
                case "1000":
                    return new ResponseHeader()
                    {
                        ResponseCode = "97",
                        ResponseMessage = "Invalid Session"
                    };
                case "1001":
                    return new ResponseHeader()
                    {
                        ResponseCode = "97",
                        ResponseMessage = "Invalid Caller"
                    };
                case "1002":
                    return new ResponseHeader()
                    {
                        ResponseCode = "97",
                        ResponseMessage = "Invalid Transaction Reference"
                    };
                case "1003":
                    return new ResponseHeader()
                    {
                        ResponseCode = "97",
                        ResponseMessage = "Duplicate Transaction Reference"
                    };
                case "1004":
                    return new ResponseHeader()
                    {
                        ResponseCode = "97",
                        ResponseMessage = "Invalid Information"
                    };
                case "1005":
                    return new ResponseHeader()
                    {
                        ResponseCode = "97",
                        ResponseMessage = "Invalid Date Format"
                    };
                case "1006":
                    return new ResponseHeader()
                    {
                        ResponseCode = "97",
                        ResponseMessage = "Invalid Source Information"
                    };
                case "1007":
                    return new ResponseHeader()
                    {
                        ResponseCode = "97",
                        ResponseMessage = "Invalid Payout Bank"
                    };
                default:
                    return new ResponseHeader()
                    {
                        ResponseCode = "97",
                        ResponseMessage = "Unknown Response Code"
                    };
            }

        }
    }
}