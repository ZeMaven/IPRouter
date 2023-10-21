namespace MomoSwitch.Models.DataBase.Tables
{
    public class SwitchTb
    {
        public int Id { get; set; }
        public string Processor { get; set; }
        public string Url { get; set; }
        public bool IsDefault { get; set; }
        public bool IsActive { get; set; }
    }
}
