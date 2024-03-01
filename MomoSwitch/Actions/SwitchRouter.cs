using Momo.Common.Actions;
using MomoSwitch.Models;
using System.Text.Json;

namespace MomoSwitch.Actions
{
    public interface ISwitchRouter
    {
        RuleMatchResponse Route(RouterRequest Request);
    }

    public class SwitchRouter : ISwitchRouter
    {
        //Logic to get the appropraite switch base on rules

        private readonly IUtilities Utilities;
        private readonly ILog Log;


        public SwitchRouter(IUtilities utilities, ILog log)
        {
            Utilities = utilities;
            Log = log;
        }

        public RuleMatchResponse Route(RouterRequest Request)
        {
            try
            {
                var Settings = Utilities.GetSettings();
                //if settings is null, get the default switch

                if (!string.IsNullOrEmpty(Request.Processor))
                    return GetDefaultSwitch(Request.Processor);



                RuleMatchResponse Resp = new();
                foreach (var Item in Settings.PriorityRuleSettingList)// Expected to be  order in asc
                {
                    switch (Item.Rule.ToUpper())
                    {
                        case "TIME":
                            return MatchTimeRule(Request.Date);
                        case "AMOUNT":
                            return MatchAmountRule(Request.Amount);
                        case "BANKSWITCH":
                            return MatchBankSwitchRule(Request.BankCode);
                    }
                }


                var JsonStr = JsonSerializer.Serialize(Request);
                Log.Write("SwitchRouter.Route", $"Request: {JsonStr}");
                Log.Write("SwitchRouter.Route", $"Failed to get Settles");

                return new RuleMatchResponse
                {
                    ResponseHeader = new ResponseHeader
                    {
                        ResponseCode = "01",
                        ResponseMessage = "Failed to get switch"
                    }
                };
            }
            catch (Exception Ex)
            {
                Log.Write("SwitchRouter.Route", $"Err {Ex.Message}");
                return new RuleMatchResponse
                {
                    ResponseHeader = new ResponseHeader
                    {
                        ResponseCode = "01",
                        ResponseMessage = "Failed to get switch"
                    }
                };
            }
        }



        private RuleMatchResponse GetDefaultSwitch(string Switch)
        {
            try
            {
                var Settings = Utilities.GetSettings();
                var Resp = new RuleMatchResponse();
                string RespJson;
                if (string.IsNullOrEmpty(Switch))
                {
                    var SwitchDetail = Settings.SwitchSettingList.Where(x => x.IsActive && x.IsDefault).FirstOrDefault();
                    if (SwitchDetail == null)
                    {
                        Log.Write("SwitchRouter.GetDefaultSwitch", $"Default Switch not found");
                        return new RuleMatchResponse
                        {
                            ResponseHeader = new ResponseHeader
                            {
                                ResponseCode = "01",
                                ResponseMessage = $"Switch: {Switch} not found"
                            }

                        };
                    }

                    Resp = new RuleMatchResponse
                    {
                        ResponseHeader = new ResponseHeader
                        {
                            ResponseCode = "00",
                            ResponseMessage = "Success"
                        },
                        Switch = SwitchDetail.Switch,
                        NameEnquiryUrl = SwitchDetail.NameEnquiryUrl,
                        TransferUrl = SwitchDetail.TransferUrl,
                        TranQueryUrl = SwitchDetail.TranQueryUrl,
                    };
                    RespJson = JsonSerializer.Serialize(Resp);
                    Log.Write("Router.GetDefaultSwitch", $"Response: {RespJson}");

                    return Resp;
                }
                else
                {
                    var SwitchDetail = Settings.SwitchSettingList.Where(x => x.IsActive && x.Switch == Switch).FirstOrDefault();
                    if (SwitchDetail == null)
                    {
                        Log.Write("SwitchRouter.GetDefaultSwitch", $"Switch: {Switch} not found");
                        return new RuleMatchResponse
                        {
                            ResponseHeader = new ResponseHeader
                            {
                                ResponseCode = "01",
                                ResponseMessage = $"Switch: {Switch} not found"
                            }

                        };
                    }

                    Resp = new RuleMatchResponse
                    {
                        ResponseHeader = new ResponseHeader
                        {
                            ResponseCode = "00",
                            ResponseMessage = "Success"
                        },
                        Switch = SwitchDetail.Switch,
                        NameEnquiryUrl = SwitchDetail.NameEnquiryUrl,
                        TransferUrl = SwitchDetail.TransferUrl,
                        TranQueryUrl = SwitchDetail.TranQueryUrl,
                    };
                    RespJson = JsonSerializer.Serialize(Resp);
                    Log.Write("Router.GetDefaultSwitch", $"Response: {RespJson}");
                    return Resp;
                }
            }
            catch (Exception Ex)
            {
                Log.Write("SwitchRouter.GetDefaultSwitch", $"Err: {Ex.Message}");
                return new RuleMatchResponse
                {
                    ResponseHeader = new ResponseHeader
                    {
                        ResponseCode = "01",
                        ResponseMessage = $"System challenge"
                    }
                };
            }
        }



        private RuleMatchResponse MatchTimeRule(DateTime Date)
        {
            try
            {
                var Settings = Utilities.GetSettings();
                var Resp = new RuleMatchResponse();
                string RespJson;
                foreach (var Time in Settings.TimeRuleList)
                {

                    var TimeA = DateTime.Parse(Date.ToString($"dd-MMM-yyyy {Time.TimeA}"));

                    var TimeZ = DateTime.Parse(Date.ToString($"dd-MMM-yyyy {Time.TimeZ}"));

                    if ((Date > TimeA) && (Date < TimeZ))
                    {
                        var TargetSwitch = Settings.SwitchSettingList.Where(x => x.Switch == Time.Switch).FirstOrDefault();

                        if (TargetSwitch.IsActive)
                        {
                            Resp = new RuleMatchResponse
                            {
                                ResponseHeader = new ResponseHeader
                                {
                                    ResponseCode = "00",
                                    ResponseMessage = "Success"
                                },
                                Switch = Time.Switch,
                                NameEnquiryUrl = TargetSwitch.NameEnquiryUrl,
                                TransferUrl = TargetSwitch.TransferUrl,
                                TranQueryUrl = TargetSwitch.TranQueryUrl,
                            };

                            RespJson = JsonSerializer.Serialize(Resp);
                            Log.Write("Router.MatchTimeRule", $"Response: {RespJson}");
                            return Resp;
                        }
                        else
                        {
                            Log.Write("Router.MatchTimeRule", $"{TargetSwitch.Switch} is inactive, defaults to Default");
                            break;
                        }
                    }
                }

                var DefaultSwitch = Settings.SwitchSettingList.Where(x => x.IsDefault == true).FirstOrDefault();

                Resp = new RuleMatchResponse
                {
                    ResponseHeader = new ResponseHeader
                    {
                        ResponseCode = "00",
                        ResponseMessage = "Success"
                    },
                    Switch = DefaultSwitch.Switch,
                    NameEnquiryUrl = DefaultSwitch.NameEnquiryUrl,
                    TransferUrl = DefaultSwitch.TransferUrl,
                    TranQueryUrl = DefaultSwitch.TranQueryUrl,
                };
                RespJson = JsonSerializer.Serialize(Resp);
                Log.Write("Router.MatchTimeRule", $"Response: {RespJson}");
                return Resp;

            }
            catch (Exception Ex)
            {
                Log.Write("Router.MatchTimeRule", $"Err {Ex.Message}");

                return new RuleMatchResponse
                {
                    ResponseHeader = new ResponseHeader
                    {
                        ResponseCode = "01",
                        ResponseMessage = "System challenge"
                    },
                };
            }
        }

        private RuleMatchResponse MatchAmountRule(decimal Amount)
        {
            try
            {
                var Settings = Utilities.GetSettings();
                var Resp = new RuleMatchResponse();
                string RespJson;
                foreach (var AmtRule in Settings.AmountRuleList)
                {


                    if ((Amount >= AmtRule.AmountA) && (Amount <= AmtRule.AmountZ))
                    {
                        var TargetSwitch = Settings.SwitchSettingList.Where(x => x.Switch == AmtRule.Switch).FirstOrDefault();

                        if (TargetSwitch.IsActive)
                        {
                            Resp = new RuleMatchResponse
                            {
                                ResponseHeader = new ResponseHeader
                                {
                                    ResponseCode = "00",
                                    ResponseMessage = "Success"
                                },
                                Switch = TargetSwitch.Switch,
                                NameEnquiryUrl = TargetSwitch.NameEnquiryUrl,
                                TransferUrl = TargetSwitch.TransferUrl,
                                TranQueryUrl = TargetSwitch.TranQueryUrl
                            };

                            RespJson = JsonSerializer.Serialize(Resp);
                            Log.Write("Router.MatchAmountRule", $"Response: {RespJson}");
                            return Resp;
                        }
                        else
                        {
                            Log.Write("Router.MatchAmountRule", $"{TargetSwitch.Switch} is inactive, defaults to Default");
                            break;
                        }
                    }
                }

                var DefaultSwitch = Settings.SwitchSettingList.Where(x => x.IsDefault == true).FirstOrDefault();

                Resp = new RuleMatchResponse
                {
                    ResponseHeader = new ResponseHeader
                    {
                        ResponseCode = "00",
                        ResponseMessage = "Success"
                    },
                    Switch = DefaultSwitch.Switch,
                    NameEnquiryUrl = DefaultSwitch.NameEnquiryUrl,
                    TransferUrl = DefaultSwitch.TransferUrl,
                    TranQueryUrl = DefaultSwitch.TranQueryUrl
                };
                RespJson = JsonSerializer.Serialize(Resp);
                Log.Write("Router.MatchAmountRule", $"Response: {RespJson}");
                return Resp;
            }
            catch (Exception Ex)
            {
                Log.Write("Router.MatchAmountRule", $"Err {Ex.Message}");

                return new RuleMatchResponse
                {
                    ResponseHeader = new ResponseHeader
                    {
                        ResponseCode = "01",
                        ResponseMessage = "System challenge"
                    },
                };
            }
        }

        private RuleMatchResponse MatchBankSwitchRule(string BankCode)
        {
            try
            {
                var Settings = Utilities.GetSettings();
                var Resp = new RuleMatchResponse();
                string RespJson;
                var One2One = Settings.BankSwitchRuleList.Where(x => x.BankCode == BankCode).FirstOrDefault();

                if (One2One != null)
                {
                    var TargetSwitch = Settings.SwitchSettingList.Where(x => x.Switch == One2One.Switch).FirstOrDefault();

                    if (TargetSwitch.IsActive)
                    {
                        Resp = new RuleMatchResponse
                        {
                            ResponseHeader = new ResponseHeader
                            {
                                ResponseCode = "00",
                                ResponseMessage = "Success"
                            },
                            Switch = TargetSwitch.Switch,
                            NameEnquiryUrl = TargetSwitch.NameEnquiryUrl,
                            TransferUrl = TargetSwitch.TransferUrl,
                            TranQueryUrl = TargetSwitch.TranQueryUrl
                        };

                        RespJson = JsonSerializer.Serialize(Resp);
                        Log.Write("Router.MatchBankSwitchRule", $"Response: {RespJson}");
                        return Resp;
                    }
                    else
                    {
                        Log.Write("Router.BankSwitchRule", $"{TargetSwitch.Switch} is inactive, defaults to Default");
                    }
                }


                var DefaultSwitch = Settings.SwitchSettingList.Where(x => x.IsDefault == true).FirstOrDefault();

                Resp = new RuleMatchResponse
                {
                    ResponseHeader = new ResponseHeader
                    {
                        ResponseCode = "00",
                        ResponseMessage = "Success"
                    },
                    Switch = DefaultSwitch.Switch,
                    NameEnquiryUrl = DefaultSwitch.NameEnquiryUrl,
                    TransferUrl = DefaultSwitch.TransferUrl,
                    TranQueryUrl = DefaultSwitch.TranQueryUrl
                };
                RespJson = JsonSerializer.Serialize(Resp);
                Log.Write("Router.BankSwitchRule", $"Response: {RespJson}");
                return Resp;
            }
            catch (Exception Ex)
            {
                Log.Write("Router.BankSwitchRule", $"Err {Ex.Message}");

                return new RuleMatchResponse
                {
                    ResponseHeader = new ResponseHeader
                    {
                        ResponseCode = "01",
                        ResponseMessage = "System challenge"
                    },
                };
            }
        }

    }
}
