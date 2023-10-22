namespace MomoSwitch.Models.Internals
{
    public class Settings
    {
        public List<TimeRule> TimeRuleList { get; set; }
        public List<AmountRule> AmountRuleList { get; set; }
        public List<BankSwitchRule> BankSwitchRuleList { get; set; }
        public List<SwitchSetting> SwitchSettingList { get; set; }
        public List<PriorityRuleSetting> PriorityRuleSettingList { get; set; }
    }






    public class TimeRule : _Switch
    {
        public string TimeA { get; set; }
        public string TimeZ { get; set; }

    }
    public class AmountRule : _Switch
    {
        public decimal AmountA { get; set; }
        public decimal AmountZ { get; set; }

    }
    public class BankSwitchRule : _Switch
    {
        public string BankCode { get; set; }

    }

    public class SwitchSetting: _Switch
    {
        public string Url { get; set; }
        public bool IsDefault { get; set; }
        public bool IsActive { get; set; }

    }
    public class PriorityRuleSetting
    {
        public string Rule { get; set; }
        public int Priority { get; set; }
    }


    public abstract class _Switch
    {
        public string Switch { get; set; }

    }
}
