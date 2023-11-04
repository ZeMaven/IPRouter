namespace MomoSwitch.Models
{
    public class RouterRequest
    { 
        /// <summary>
        /// Current datetime
        /// </summary>
        public DateTime Date { get; set; }
        /// <summary>
        /// Destination bank code
        /// </summary>
         public string BankCode { get; set; }
        public decimal Amount { get; set;}

        public string Processor { get; set; }  //If it has a value, then use. useful for requery,
    }
}
