using MomoSwitch.Models;
using System.Text.Json;

namespace MomoSwitch.Actions
{
    public interface IRouter
    {
        RuleMatchResponse Route(RouterRequest Request);
    }

    public class Router : IRouter
    {
        //Logic to get the appropraite switch base on rules

        private readonly IUtilities Utilities;
        private readonly ILog Log;


        public Router(IUtilities utilities, ILog log)
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
                Log.Write("Router.Route", $"Request: {JsonStr}");
                Log.Write("Router.Route", $"Failed to get Settles");

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
                Log.Write("Router.Route", $"Err {Ex.Message}");
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







        private RuleMatchResponse MatchTimeRule(DateTime Date)
        {
            try
            {
                var Settings = Utilities.GetSettings();

                foreach (var Time in Settings.TimeRuleList)
                {

                    var TimeA = DateTime.Parse(Date.ToString($"dd-MMM-yyyy {Time.TimeA}"));

                    var TimeZ = DateTime.Parse(Date.ToString($"dd-MMM-yyyy {Time.TimeZ}"));

                    if ((Date > TimeA) && (Date < TimeZ))
                    {
                        var TargetSwitch = Settings.SwitchSettingList.Where(x => x.Switch == Time.Switch).FirstOrDefault();

                        if (TargetSwitch.IsActive)
                        {
                            return new RuleMatchResponse
                            {
                                ResponseHeader = new ResponseHeader
                                {
                                    ResponseCode = "00",
                                    ResponseMessage = "Success"
                                },
                                Switch = Time.Switch,
                                Url = TargetSwitch.Url
                            };
                        }
                        else
                        {
                            Log.Write("Router.MatchTimeRule", $"{TargetSwitch.Switch} is inactive, defaults to Default");
                            break;
                        }
                    }
                }

                var DefaultSwitch = Settings.SwitchSettingList.Where(x => x.IsDefault == true).FirstOrDefault();

                return new RuleMatchResponse
                {
                    ResponseHeader = new ResponseHeader
                    {
                        ResponseCode = "00",
                        ResponseMessage = "Success"
                    },
                    Switch = DefaultSwitch.Switch,
                    Url = DefaultSwitch.Url
                };
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

                foreach (var AmtRule in Settings.AmountRuleList)
                {


                    if ((Amount > AmtRule.AmountA) && (Amount < AmtRule.AmountZ))
                    {
                        var TargetSwitch = Settings.SwitchSettingList.Where(x => x.Switch == AmtRule.Switch).FirstOrDefault();

                        if (TargetSwitch.IsActive)
                        {
                            return new RuleMatchResponse
                            {
                                ResponseHeader = new ResponseHeader
                                {
                                    ResponseCode = "00",
                                    ResponseMessage = "Success"
                                },
                                Switch = TargetSwitch.Switch,
                                Url = TargetSwitch.Url
                            };
                        }
                        else
                        {
                            Log.Write("Router.MatchAmountRule", $"{TargetSwitch.Switch} is inactive, defaults to Default");
                            break;
                        }
                    }
                }

                var DefaultSwitch = Settings.SwitchSettingList.Where(x => x.IsDefault == true).FirstOrDefault();

                return new RuleMatchResponse
                {
                    ResponseHeader = new ResponseHeader
                    {
                        ResponseCode = "00",
                        ResponseMessage = "Success"
                    },
                    Switch = DefaultSwitch.Switch,
                    Url = DefaultSwitch.Url
                };
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

                var One2One = Settings.BankSwitchRuleList.Where(x => x.BankCode == BankCode).FirstOrDefault();

                if (One2One != null)
                {
                    var TargetSwitch = Settings.SwitchSettingList.Where(x => x.Switch == One2One.Switch).FirstOrDefault();

                    if (TargetSwitch.IsActive)
                    {
                        return new RuleMatchResponse
                        {
                            ResponseHeader = new ResponseHeader
                            {
                                ResponseCode = "00",
                                ResponseMessage = "Success"
                            },
                            Switch = TargetSwitch.Switch,
                            Url = TargetSwitch.Url
                        };
                    }
                    else
                    {
                        Log.Write("Router.BankSwitchRule", $"{TargetSwitch.Switch} is inactive, defaults to Default");
                    }
                }


                var DefaultSwitch = Settings.SwitchSettingList.Where(x => x.IsDefault == true).FirstOrDefault();

                return new RuleMatchResponse
                {
                    ResponseHeader = new ResponseHeader
                    {
                        ResponseCode = "00",
                        ResponseMessage = "Success"
                    },
                    Switch = DefaultSwitch.Switch,
                    Url = DefaultSwitch.Url
                };
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
