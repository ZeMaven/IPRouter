namespace MomoSwitch.Models.DataBase.Tables
{
    public class SwitchTb
    {
        public int Id { get; set; }
        public string Processor { get; set; } 
        public string NameEnquiryUrl { get; set; }
        public string TransferUrl { get; set; }
        public string TranQueryUrl { get; set; }      
        public bool IsDefault { get; set; }
        public bool IsActive { get; set; }
    }
}
