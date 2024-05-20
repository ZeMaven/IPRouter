using Momo.Common.Actions;
using Momo.Common.Models;
using NipOutwardProxy.Models.Nibss;
using System.Text.Json;
using NipService;
using NipServiceTsq;

namespace NipOutwardProxy.Actions
{
    public interface INipOutward
    {
        Task<NameEnquiryPxResponse> NameEnquiry(NameEnquiryPxRequest Request);
        Task<TranQueryPxResponse> TransactionQuery(TranQueryPxRequest Request);
        Task<FundTransferPxResponse> Transfer(FundTransferPxRequest Request);
    }

    public class NipOutward : INipOutward
    {

        private readonly IConfiguration Config;
        private readonly ILog Log;
        private readonly ICommonUtilities Utilities;
        private readonly IHttpService HttpService;
        private readonly IPgp Pgp;
        private readonly IXmlConverter XmlConverter;
        private NIPInterfaceClient Nip;
        private NIPTSQInterfaceClient NipTSQ;

        public NipOutward(IConfiguration config, ILog log, ICommonUtilities utilities, IHttpService httpService, IPgp pgp, IXmlConverter xmlConverter)
        {
            Log = log;
            Config = config;
            Utilities = utilities;
            HttpService = httpService;
            Pgp = pgp;
            XmlConverter = xmlConverter;
            Nip = new NIPInterfaceClient();
            NipTSQ = new NIPTSQInterfaceClient();
        }

        public async Task<NameEnquiryPxResponse> NameEnquiry(NameEnquiryPxRequest Request)
        {
            try
            {
                string SessionId = string.Empty;
                string SourceBank = string.Empty;
                var TranId = Utilities.CreateTransactionId();
                SourceBank = Config.GetSection("SourceBank").Value;
                SessionId = $"{SourceBank}{TranId}";

                Log.Write("NibssOutward.NameEnquiry", $"Request from Router: {JsonSerializer.Serialize(Request)}");
                var NibssRequest = new NESingleRequest
                {
                    AccountNumber = Request.AccountId,
                    ChannelCode = Request.ChannelCode,
                    DestinationInstitutionCode = Request.DestinationBankCode,
                    SessionID = SessionId
                };
                var XmlRequest = XmlConverter.Serialize(NibssRequest);
                Log.Write("NibssOutward.NameEnquiry", $"Request to Nibss xml: {XmlRequest}");
                var EncRequest = Pgp.Ecryption(XmlRequest);
                Log.Write("NibssOutward.NameEnquiry", $"Request to Nibss enc: {EncRequest}");
                var NipResp = await Nip.nameenquirysingleitemAsync(EncRequest);
                Log.Write("NibssOutward.NameEnquiry", $"Response from Nibss: {JsonSerializer.Serialize(NipResp)}");
                var NibssResponseEnc = NipResp.Body.@return;

                Log.Write("NibssOutward.NameEnquiry", $"Response from Nibss enc: {NibssResponseEnc}");

                var NibssResponseXml = Pgp.Decryption(NibssResponseEnc);
                Log.Write("NibssOutward.NameEnquiry", $"Response from Nibss Xml: {NibssResponseXml}");

                NESingleResponse ResponseObj = (NESingleResponse)XmlConverter.DeSerialize(NibssResponseXml, new NESingleResponse());

                var RouterResponse = new NameEnquiryPxResponse
                {
                    AccountName = ResponseObj.AccountName,
                    AccountNumber = ResponseObj.AccountNumber,
                    Bvn = ResponseObj.BankVerificationNumber,
                    ChannelCode = ResponseObj.ChannelCode,
                    DestinationBankCode = ResponseObj.DestinationInstitutionCode,
                    KycLevel = ResponseObj.KYCLevel,
                    ResponseCode = ResponseObj.ResponseCode,
                    ResponseMessage = ResponseObj.ResponseCode == "00" ? "Successful" : null,
                    SessionId = ResponseObj.SessionID,
                    TransactionId = Request.TransactionId,
                    SourceBankCode = Request.SourceBankCode,
                };

                Log.Write("NibssOutward.NameEnquiry", $"Response to Router: {JsonSerializer.Serialize(RouterResponse)}");
                return RouterResponse;
            }
            catch (Exception Ex)
            {
                Log.Write("NibssOutward.NameEnquiry", $"Err: {Ex.Message}");
                return new NameEnquiryPxResponse
                {
                    ResponseCode = "01",
                    ResponseMessage = "Failed",
                    TransactionId = Request.TransactionId,
                    SourceBankCode = Request.SourceBankCode,
                };
            }
        }


        public async Task<TranQueryPxResponse> TransactionQuery(TranQueryPxRequest Request)
        {
            try
            {
                string SessionId = string.Empty;
                string SourceBank = string.Empty;
                var TranId = Utilities.CreateTransactionId();
                SourceBank = Config.GetSection("SourceBank").Value;
                SessionId = Request.SessionId;

                Log.Write("NibssOutward.TranQuery", $"Request from Router: {JsonSerializer.Serialize(Request)}");
                var NibssRequest = new TSQuerySingleRequest
                {
                    SourceInstitutionCode = SourceBank,
                    ChannelCode = 0,
                    SessionID = SessionId
                };
                var XmlRequest = XmlConverter.Serialize(NibssRequest);
                Log.Write("NibssOutward.TranQuery", $"Request to Nibss xml: {XmlRequest}");
                var EncRequest = Pgp.Ecryption(XmlRequest);
                Log.Write("NibssOutward.TranQuery", $"Request to Nibss enc: {EncRequest}");
                //incorrect
                var NipResp = await NipTSQ.txnstatusquerysingleitemAsync(EncRequest);
                var NibssResponseEnc = NipResp.Body.@return;
                //
                Log.Write("NibssOutward.TranQuery", $"Response from Nibss enc: {NibssResponseEnc}");

                var NibssResponseXml = Pgp.Decryption(NibssResponseEnc);
                Log.Write("NibssOutward.TranQuery", $"Response from Nibss Xml: {NibssResponseXml}");

                TSQuerySingleResponse ResponseObj = (TSQuerySingleResponse)XmlConverter.DeSerialize(NibssResponseXml, new TSQuerySingleResponse());

                var RouterResponse = new TranQueryPxResponse
                {
                    ChannelCode = ResponseObj.ChannelCode,
                    ResponseCode = ResponseObj.ResponseCode,
                    ResponseMessage = ResponseObj.ResponseCode == "00" ? "Successful" : null,
                    SessionId = ResponseObj.SessionID,
                    TransactionId = Request.SessionId,
                    SourceBankCode = SourceBank
                };

                Log.Write("NibssOutward.TranQuery", $"Response to Router: {JsonSerializer.Serialize(RouterResponse)}");
                return RouterResponse;
            }
            catch (Exception Ex)
            {
                Log.Write("NibssOutward.TranQuery", $"Err: {Ex.Message}");
                return new TranQueryPxResponse
                {
                    ResponseCode = "97",
                    ResponseMessage = "Failed",
                    TransactionId = Request.SessionId,
                };
            }
        }




        public async Task<FundTransferPxResponse> Transfer(FundTransferPxRequest Request)
        {
            string SessionId = string.Empty;
            string SourceBank = string.Empty;
            try
            {

                var TranId = Utilities.CreateTransactionId();
                SourceBank = Config.GetSection("SourceBank").Value;
                SessionId = $"{SourceBank}{TranId}";

                Log.Write("NibssOutward.Transfer", $"Request from Router: {JsonSerializer.Serialize(Request)}");
                var NibssRequest = new FTSingleCreditRequest
                {
                    Amount = Request.Amount,
                    BeneficiaryAccountName = Request.BenefAccountName,
                    BeneficiaryAccountNumber = Request.BenefAccountNumber,
                    BeneficiaryBankVerificationNumber = Request.BenefBvn ?? "",
                    BeneficiaryKYCLevel = Request.BenefKycLevel,
                    ChannelCode = Request.ChannelCode,
                    DestinationInstitutionCode = Request.DestinationBankCode,
                    MandateReferenceNumber = "",
                    NameEnquiryRef = Request.NameEnquiryRef,
                    Narration = Request.Narration,
                    OriginatorAccountName = Request.SourceAccountName,
                    OriginatorAccountNumber = Request.SourceAccountNumber,
                    OriginatorBankVerificationNumber = Request.InitiatorBankVerificationNumber ?? "",
                    OriginatorKYCLevel = Request.InitiatorKYCLevel,
                    PaymentReference = Request.PaymentRef,
                    SessionID = SessionId,
                    TransactionFee = 0,
                    TransactionLocation = ""// Request.TransactionLocation,
                };
                var XmlRequest = XmlConverter.Serialize(NibssRequest);
                Log.Write("NibssOutward.Transfer", $"Request to Nibss xml: SessionID:{SessionId} | {XmlRequest}");
                var EncRequest = Pgp.Ecryption(XmlRequest);
                Log.Write("NibssOutward.Transfer", $"Request to Nibss enc : SessionID:{SessionId} | ENC: {EncRequest}");
                var NipResp = await Nip.fundtransfersingleitem_dcAsync(EncRequest);



                var NibssResponseEnc = NipResp.Body.@return;

                Log.Write("NibssOutward.Transfer", $"Response from Nibss enc : SessionID:{SessionId} | ENC: {NibssResponseEnc}");

                var NibssResponseXml = Pgp.Decryption(NibssResponseEnc);
                Log.Write("NibssOutward.Transfer", $"Response from Nibss Xml: SessionID:{SessionId} | {NibssResponseXml}");

                FTSingleCreditResponse ResponseObj = (FTSingleCreditResponse)XmlConverter.DeSerialize(NibssResponseXml, new FTSingleCreditResponse());

                var RouterResponse = new FundTransferPxResponse
                {
                    SourceAccountName = Request.SourceAccountName,
                    Amount = Request.Amount,
                    BenefAccountName = Request.BenefAccountName,
                    BenefAccountNumber = Request.BenefAccountNumber,
                    BenefKycLevel = Request.BenefKycLevel,
                    BenefBvn = ResponseObj.BeneficiaryBankVerificationNumber,
                    BenfBankCode = ResponseObj.DestinationInstitutionCode,
                    NameEnquiryRef = ResponseObj.NameEnquiryRef,
                    Narration = ResponseObj.Narration,
                    SourceAccountNumber = Request.SourceAccountNumber,
                    TransactionDate = DateTime.Now,
                    ChannelCode = ResponseObj.ChannelCode,
                    ResponseCode = ResponseObj.ResponseCode,
                    ResponseMessage = ResponseObj.ResponseCode == "00" ? "Successful" : null,
                    SessionId = ResponseObj.SessionID,
                    TransactionId = Request.TransactionId,
                    SourceBankCode = Request.SourceBankCode,
                };

                Log.Write("NibssOutward.Transfer", $"Response to Router: {JsonSerializer.Serialize(RouterResponse)}");
                return RouterResponse;
            }
            catch (Exception Ex)
            {
                Log.Write("NibssOutward.Transfer", $"Err: {Ex.Message}");
                return new FundTransferPxResponse
                {
                    ResponseCode = "97",
                    ResponseMessage = "Failed",
                    TransactionId = Request.TransactionId,
                    SourceBankCode = Request.SourceBankCode,
                    SessionId = SessionId
                };
            }
        }
    }
}
