﻿using System.ComponentModel.DataAnnotations;

namespace MomoSwitchPortal.Models.ViewModels.Transaction
{
    public class TransactionTableViewModel
    {
        public int Id { get; set; }
        public string Date { get; set; }     
        public string Processor { get; set; }  
        public string TransactionId { get; set; }             
        public string BenefBankCode { get; set; }     
        public string SourceBankCode { get; set; }       
        public decimal Amount { get; set; }       
        public string ResponseCode { get; set; }
        public string ResponseMessage { get; set; }
    }
}