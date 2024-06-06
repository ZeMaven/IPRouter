using CsvHelper.Configuration.Attributes;

namespace Jobs.Models
{
    public class EwpTranDetails
    {
        [Name("External id")]
        public string ExternalId { get; set; }

        [Name("Id")]
        public string SessionId { get; set; }
        [Optional]
        [Default(0)]
        [Name("Amount")]
        public decimal SuccAmount { get; set; }

        [Optional]
        [Default(0)]
        [Name("Amount")]
        public decimal PendingAmount { get; set; }

        [Optional]
        [Default(0)]

        [Name("Original amount")]
        public decimal? FailedAmount { get; set; }

        [Name("Date")]
        public string Date { get; set; }
    }
}
