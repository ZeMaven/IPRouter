using Microsoft.Extensions.Caching.Memory;
using MomoSwitch.Models.DataBase;
using MomoSwitch.Models.Internals;
using System.Text.Json;

namespace MomoSwitch.Actions
{
    public interface IUtilities
    {
        Settings GetSettings();
    }

    public class Utilities : IUtilities
    {
        private readonly IMemoryCache SettingsCache;
        private readonly ILog Log;


        public Utilities(IMemoryCache settingsCache, ILog log)
        {
            SettingsCache = settingsCache;
            Log = log;
        }


        public Settings GetSettings()
        {
            try
            {
                var CacheString = SettingsCache.Get<string>($"SettingsCache");
                if (!string.IsNullOrEmpty(CacheString))
                {
                    var Setting = JsonSerializer.Deserialize<Settings>(CacheString);
                    return Setting;
                }
                else
                {
                    var Db = new MomoSwitchDbContext();
                    var TimeRule = Db.TimeRuleTb.ToList();
                    if (TimeRule.Count > 0) Log.Write("Utilities.GettSettings", "TimeRule setting does not exist");
                    var AmountRule = Db.AmountRuleTb.ToList();
                    if (AmountRule.Count > 0) Log.Write("Utilities.GettSettings", "AmountRule setting does not exist");
                    var BankSwitch = Db.BankSwitchTb.ToList();
                    if (BankSwitch.Count > 0) Log.Write("Utilities.GettSettings", "BankSwitch setting does not exist");
                    var Switch = Db.SwitchTb.ToList();
                    if (Switch.Count > 0) Log.Write("Utilities.GettSettings", "Switch setting does not exist");
                    var Priority = Db.PriorityTb.ToList();
                    if (Priority.Count > 0) Log.Write("Utilities.GettSettings", "Priority setting does not exist");


                    var Setting = new Settings
                    {
                        AmountRuleList = AmountRule.Select(t => new Models.Internals.AmountRule
                        {
                            AmountA = t.AmountA,
                            AmountZ = t.AmountZ,
                            Switch = t.Processor
                        }).ToList(),
                        TimeRuleList = TimeRule.Select(t => new Models.Internals.TimeRule
                        {
                            TimeA = t.TimeA,
                            TimeZ = t.TimeZ,
                            Switch = t.Processor

                        }).ToList(),
                        BankSwitchRuleList = BankSwitch.Select(t => new BankSwitchRule
                        {
                            BankCode = t.BankCode,
                            Switch = t.Processor
                        }).ToList(),
                        SwitchSettingList = Switch.Select(t => new SwitchSetting
                        {
                            Switch = t.Processor,
                            IsActive = t.IsActive,
                            IsDefault = t.IsDefault,
                            TranQueryUrl = t.TranQueryUrl,
                            TransferUrl = t.TransferUrl,
                            NameEnquiryUrl = t.NameEnquiryUrl,
                        }).ToList(),
                        PriorityRuleSettingList = Priority.Select(t => new PriorityRuleSetting
                        {
                            Priority = t.Priority,
                            Rule = t.Rule
                        }).ToList()
                    };

                    var settingStr = JsonSerializer.Serialize(Setting);
                    SettingsCache.Set($"SettingsCache", settingStr, TimeSpan.FromDays(7));
                    return Setting;
                }
            }
            catch (Exception Ex)
            {
                Log.Write("Utilities.GettSettings", $"Err: Error getting settings | {Ex.Message}");
                return null;
            }
        }










    }
}
