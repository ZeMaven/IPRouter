using Azure.Core;
using Momo.Common.Models;
using MomoSwitch.Models;
using MomoSwitch.Models.Contracts.Momo;
using MomoSwitch.Models.DataBase;
using MomoSwitch.Models.Internals;

namespace MomoSwitch.Actions
{
    public interface IDataExpress
    {
        Task<TranQueryResponse> GetTransaction(string SessionId);
        ResponseHeader SubmitTransaction(FundTransferRequest Request, string Proccesor);
        ResponseHeader UpdateTransaction(string SessionId, string ResponseCode);
    }

    public class DataExpress : IDataExpress
    {
        private readonly ILog Log;
        private readonly IOutward Outward;
        public DataExpress(ILog log, IOutward outward)
        {
            Log = log;
            Outward = outward;
        }

        public ResponseHeader SubmitTransaction(FundTransferRequest Request, string Proccesor)
        {
            try
            {
                var Db = new MomoSwitchDbContext();

                Db.TransactionTb.Add(new Models.DataBase.Tables.TransactionTb
                {
                    Date = DateTime.Now,
                    Processor = Proccesor,
                    Amount = Request.amount,
                    BenefAccountName = Request.beneficiaryAccountName,
                    BenefAccountNumber = Request.beneficiaryAccountNumber,
                    BenefBankCode = Request.destinationInstitutionCode,
                    BenefBvn = Request.beneficiaryBankVerificationNumber,
                    BenefKycLevel = Request.beneficiaryKYCLevel.ToString(),
                    ChannelCode = Request.channelCode.ToString(),
                    Fee = 0,
                    ManadateRef = Request.mandateReferenceNumber,
                    Narration = Request.originatorNarration,
                    PaymentReference = Request.paymentReference,
                    ResponseCode = "09",
                    ResponseMessage = "Pending",
                    SessionId = Request.transactionId,
                    SourceAccountName = Request.originatorAccountName,
                    SourceAccountNumber = Request.originatorAccountNumber,
                    SourceBankCode = Request.sourceInstitutionCode,
                    SourceBvn = Request.originatorBankVerificationNumber,
                    SourceKycLevel = Request.originatorKYCLevel.ToString()
                });

                Db.SaveChanges();

                Log.Write("DataExpress.SubmitTransaction", $"Transaction saved Ok: Session:{Request.transactionId}");
                return new ResponseHeader
                {
                    ResponseCode = "00",
                    ResponseMessage = "Saved well"
                };

            }
            catch (Exception Ex)
            {
                Log.Write("DataExpress.SubmitTransaction", $"Failed tosave Transaction: Session:{Request.transactionId}");
                Log.Write("DataExpress.SubmitTransaction", $"Err {Ex.Message} Session:{Request.transactionId}");
                return new ResponseHeader
                {
                    ResponseCode = "01",
                    ResponseMessage = "Failed saving transation"
                };
            }
        }



        public ResponseHeader UpdateTransaction(string SessionId, string ResponseCode)
        {
            try
            {
                var Db = new MomoSwitchDbContext();
                var Tran = Db.TransactionTb.SingleOrDefault(x => x.SessionId == SessionId);
                if (Tran != null)
                {
                    Tran.PaymentDate = DateTime.Now;
                    Tran.ResponseCode = ResponseCode == "00" ? "16" : ResponseCode;
                    Tran.ResponseMessage = "";
                    Db.SaveChanges();
                    return new ResponseHeader
                    {
                        ResponseCode = "00",
                        ResponseMessage = "Saved well"
                    };
                }
                else
                {
                    Log.Write("DataExpress.UpdateTransaction", $"Critical Issue. Transaction not found Session:{SessionId}");
                    return new ResponseHeader
                    {
                        ResponseCode = "01",
                        ResponseMessage = "Transaction not found"
                    };
                }
            }
            catch (Exception Ex)
            {
                Log.Write("DataExpress.UpdateTransaction", $"Failed to update Transaction: Session:{SessionId}");
                Log.Write("DataExpress.UpdateTransaction", $"Err {Ex.Message} Session:{SessionId}");
                return new ResponseHeader
                {
                    ResponseCode = "01",
                    ResponseMessage = "Failed saving transation"
                };
            }
        }


        public async Task<TranQueryResponse> GetTransaction(string SessionId)
        {
            try
            {

                var Db = new MomoSwitchDbContext();
                var Tran = Db.TransactionTb.SingleOrDefault(x => x.SessionId == SessionId);
                if (Tran != null)
                {
                    if (Tran.ResponseCode != "00")
                    {
                        //Get from processor
                        var QueryTran = await Outward.TranQuery(new TranQueryRequest { transactionId = Tran.SessionId }, Tran.Processor);

                        //UpdateDb
                        Tran.ResponseCode = QueryTran.responseCode;
                        Tran.ResponseMessage = QueryTran.responseMessage;
                        Tran.ValidateDate = DateTime.Now;
                        Db.SaveChanges();


                        return new TranQueryResponse
                        {
                            code = QueryTran.responseCode,
                            sourceInstitutionCode = Tran.SourceBankCode,
                            message = Tran.ResponseMessage,
                            responseMessage = Tran.ResponseMessage,
                            sessionID = Tran.SessionId,
                            transactionId = Tran.SessionId,
                            responseCode = QueryTran.responseCode,
                        };

                    }
                    else
                    {
                        return new TranQueryResponse
                        {
                            code = Tran.ResponseCode,
                            sourceInstitutionCode = Tran.SourceBankCode,
                            message = Tran.ResponseMessage,
                            responseMessage = Tran.ResponseMessage,
                            sessionID = Tran.SessionId,
                            transactionId = Tran.SessionId,
                            responseCode = Tran.ResponseCode
                        };

                    }
                }
                else
                {
                    Log.Write("DataExpress.UpdateTransaction", $"Critical Issue. Transaction not found Session:{SessionId}");
                    return new TranQueryResponse
                    {

                        sessionID = SessionId,
                        transactionId = SessionId,
                        responseCode = "25"
                    };
                }
            }
            catch (Exception Ex)
            {
                Log.Write("DataExpress.GetTransaction", $"Err {Ex.Message} Session:{SessionId}");
                return new TranQueryResponse
                {
                    sessionID = SessionId,
                    transactionId = SessionId,
                    responseCode = "06"
                };
            }
        }
    }
}