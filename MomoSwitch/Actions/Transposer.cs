using Momo.Common.Models;
using MomoSwitch.Models.Contracts.Momo;
 


namespace MomoSwitch.Actions
{
    public interface ITransposer
    {
        FundTransferResponse ToMomoFundTransferResponse(FundTransferPxResponse proxyName);
        NameEnquiryResponse ToMomoNameEnquiryResponse(NameEnquiryPxResponse proxyName);
        TranQueryResponse ToMomoTranQueryResponse(TranQueryPxResponse proxyName);
        FundTransferPxRequest ToProxyFundTransferRequest(FundTransferRequest Request);
        NameEnquiryPxRequest ToProxyNameEnquiryRequest(NameEnquiryRequest Request);
        TranQueryPxRequest ToProxyTranQueryyRequest(TranQueryRequest Request);
    }

    public class Transposer : ITransposer
    {


        #region ProxyToMomo

        public NameEnquiryResponse ToMomoNameEnquiryResponse(NameEnquiryPxResponse proxyName) => new NameEnquiryResponse
        {
            accountName = proxyName.AccountName,
            accountNumber = proxyName.AccountNumber,
            bankVerificationNumber = proxyName.Bvn,
            channelCode = 0,
            kycLevel = proxyName.KycLevel,
            responseCode = proxyName.ResponseCode,
            transactionId = proxyName.TransactionId,
            sessionID = proxyName.SessionId,
            destinationInstitutionCode = proxyName.DestinationBankCode
        };

        public FundTransferResponse ToMomoFundTransferResponse(FundTransferPxResponse proxyName) => new FundTransferResponse
        {
            transactionId = proxyName.TransactionId,
            narration = proxyName.Narration,
            sessionID = proxyName.SessionId,
            nameEnquiryRef = proxyName.NameEnquiryRef,
            amount = proxyName.Amount,
            beneficiaryAccountName = proxyName.BenefAccountName,
            beneficiaryAccountNumber = proxyName.BenefAccountNumber,
            beneficiaryBankVerificationNumber = proxyName.BenefBvn,
            beneficiaryKYCLevel = proxyName.BenefKycLevel,
            channelCode = proxyName.ChannelCode,
            debitAccountName = proxyName.SourceAccountName,
            debitAccountNumber = proxyName.SourceAccountNumber,
            destinationInstitutionCode = proxyName.BenfBankCode,
            responseCode = proxyName.ResponseCode,

            paymentReference = null,
            mandateReferenceNumber = null,
            debitBankVerificationNumber = null,
            debitKYCLevel = 0,
            transactionFee = 0,
            transactionLocation = 0,
        };

        public TranQueryResponse ToMomoTranQueryResponse(TranQueryPxResponse proxyName) => new TranQueryResponse
        {
            sessionID = proxyName.SessionId,
            transactionId = proxyName.TransactionId,
            channelCode = proxyName.ChannelCode,
            sourceInstitutionCode = proxyName.SourceBankCode,
            responseCode = proxyName.ResponseCode,
            message = proxyName.ResponseMessage,
        };

        #endregion


        #region MomoToProxy

        public NameEnquiryPxRequest ToProxyNameEnquiryRequest(NameEnquiryRequest Request) => new NameEnquiryPxRequest
        {
            AccountId = Request.accountNumber,
            DestinationBankCode = Request.destinationInstitutionCode,
            TransactionId = Request.transactionId,
        };


        public FundTransferPxRequest ToProxyFundTransferRequest(FundTransferRequest Request) => new FundTransferPxRequest
        {
            TransactionId = Request.transactionId,
            BenefBvn = Request.beneficiaryBankVerificationNumber,
            DestinationBankCode = Request.destinationInstitutionCode,
            Amount = Request.amount,
            BenefAccountName = Request.beneficiaryAccountName,
            BenefAccountNumber = Request.beneficiaryAccountNumber,
            Narration = Request.beneficiaryNarration,
            NameEnquiryRef = Request.nameEnquiryRef,
            SourceAccountName = Request.originatorAccountName,
            SourceAccountNumber = Request.originatorAccountNumber,
            SourceBankCode = "MomoCode"//Put in config
        };

        public TranQueryPxRequest ToProxyTranQueryyRequest(TranQueryRequest Request) => new TranQueryPxRequest
        {
            TransactionId = Request.transactionId,
        };

        #endregion


    }
}
