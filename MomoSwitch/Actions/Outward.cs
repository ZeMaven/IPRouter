using Microsoft.IdentityModel.Tokens;
using Momo.Common.Actions;
using Momo.Common.Models;
using Momo.Common.Models.Tables;
using MomoSwitch.Models;
using MomoSwitch.Models.Contracts;
using MomoSwitch.Models.Contracts.Momo;
using MomoSwitch.Models.Contracts.Specials.Router;
using MomoSwitch.Models.DataBase;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace MomoSwitch.Actions
{
    public interface IOutward
    {
        Task<NameEnquiryResponse> NameEnquiry(NameEnquiryRequest Req);

        Task<TranQueryResponse> GetTransaction(string SessionId);
        Task<FundTransferResponse> Transfer(FundTransferRequest Req);
        AuthResponse Reset(AuthRequest Req);
    }

    public class Outward : IOutward
    {
        private readonly ISwitchRouter SwitchRouter;
        private readonly ITransposer Transposer;
        private readonly IHttpService HttpService;
        private readonly ILog Log;
        private readonly IConfiguration Config;
        public Outward(ISwitchRouter router, ILog log, ITransposer transposer, IHttpService httpService, IConfiguration config)
        {
            SwitchRouter = router;
            Transposer = transposer;
            Log = log;
            HttpService = httpService;
            Config = config;
        }
        JsonSerializerOptions Options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

        public async Task<NameEnquiryResponse> NameEnquiry(NameEnquiryRequest Req)
        {
            NameEnquiryResponse Resp;
            try
            {  //Review

                string JsonStr = JsonSerializer.Serialize(Req);
                Log.Write("Outward.NameEnqury", $"Request: {JsonStr}");
                var RouterDetail = SwitchRouter.Route(new Models.RouterRequest
                {
                    Amount = 100000,
                    BankCode = Req.destinationInstitutionCode,
                    Date = DateTime.Now,
                });

                var ProcessorRequest = Transposer.ToProxyNameEnquiryRequest(Req);

                var ProcessorResp = await HttpService.Call(new Models.Internals.HttpService.HttpServiceRequest
                {
                    EndPoint = RouterDetail.NameEnquiryUrl,
                    Method = Models.Internals.HttpService.Method.Post,
                    RequestObject = ProcessorRequest
                });

                if (ProcessorResp.ResponseHeader.ResponseCode == Models.Internals.HttpService.HttpServiceStatus.Success)
                {
                    NameEnquiryPxResponse ProcessorRespObj = JsonSerializer.Deserialize<NameEnquiryPxResponse>(ProcessorResp.Object.ToString(), Options);

                    Resp = Transposer.ToMomoNameEnquiryResponse(ProcessorRespObj);

                    Resp.responseCode = Resp.responseCode != "00" ? Resp.responseCode = "A6" : Resp.responseCode = "00";
                    JsonStr = JsonSerializer.Serialize(Resp);
                    Log.Write("Outward.NameEnqury", $"Response: {JsonStr}");
                }
                else
                {
                    Resp = new NameEnquiryResponse
                    {
                        responseCode = "A6",
                        responseMessage = "System challenge",
                        accountNumber = Req.accountNumber,
                        transactionId = Req.transactionId,
                    };
                    JsonStr = JsonSerializer.Serialize(Resp);
                    Log.Write("Outward.NameEnqury", $"Response: {JsonStr}");

                }
                return Resp;
            }
            catch (Exception Ex)
            {
                Log.Write("Outward.NameEnqury", $"Err: {Ex.Message}");
                return new NameEnquiryResponse
                {
                    responseCode = "A6",
                    responseMessage = "System challenge",
                    accountNumber = Req.accountNumber,
                    transactionId = Req.transactionId,
                };
            }
        }

        public async Task<FundTransferResponse> Transfer(FundTransferRequest Req)
        {
            FundTransferResponse Resp;
            try
            {
                string JsonStr = JsonSerializer.Serialize(Req);
                Log.Write("Outward.Transfer", $"Request: {JsonStr}");
                var RouterDetail = SwitchRouter.Route(new Models.RouterRequest
                {
                    Amount = Req.amount,
                    BankCode = Req.destinationInstitutionCode,
                    Date = DateTime.Now,
                });
                var ProcessorRequest = Transposer.ToProxyFundTransferRequest(Req);
                var Submit = SubmitTransaction(Req, RouterDetail.Switch);
                if (Submit.ResponseCode != "00")
                    return new FundTransferResponse
                    {
                        responseCode = Submit.ResponseCode,
                        amount = Req.amount,
                        beneficiaryAccountName = Req.beneficiaryAccountName,
                        beneficiaryAccountNumber = Req.beneficiaryAccountNumber,
                        beneficiaryBankVerificationNumber = Req.beneficiaryBankVerificationNumber,
                        beneficiaryKYCLevel = Req.beneficiaryKYCLevel,
                        channelCode = Req.channelCode,
                        initiatorAccountName = Req.initiatorAccountName,
                        initiatorAccountNumber = Req.initiatorAccountNumber,
                        initiatorBankVerificationNumber = Req.initiatorBankVerificationNumber,
                        originatorKYCLevel = Req.originatorKYCLevel,
                        destinationInstitutionCode = Req.destinationInstitutionCode,
                        mandateReferenceNumber = Req.mandateReferenceNumber,
                        nameEnquiryRef = Req.nameEnquiryRef,
                        narration = Req.originatorNarration,
                        paymentReference = Req.paymentReference,
                        transactionId = Req.transactionId,
                        originatorAccountName = Req.originatorAccountName,
                        originatorAccountNumber = Req.originatorAccountNumber,
                        originatorBankVerificationNumber = Req.originatorBankVerificationNumber,
                    };
                var ProcessorResp = await HttpService.Call(new Models.Internals.HttpService.HttpServiceRequest
                {
                    EndPoint = RouterDetail.TransferUrl,
                    Method = Models.Internals.HttpService.Method.Post,
                    RequestObject = ProcessorRequest
                });


                FundTransferPxResponse ProcessorRespObj = JsonSerializer.Deserialize<FundTransferPxResponse>(ProcessorResp.Object.ToString(), Options);
                if (ProcessorResp.ResponseHeader.ResponseCode == Models.Internals.HttpService.HttpServiceStatus.Success)
                {

                    Resp = Transposer.ToMomoFundTransferResponse(ProcessorRespObj);
                    Resp.paymentReference = Resp.transactionId;
                    Resp.channelCode = Req.channelCode;
                    Resp.transactionLocation = Req.transactionLocation;
                    Resp.beneficiaryKYCLevel = Req.beneficiaryKYCLevel;
                    Resp.initiatorBankVerificationNumber = Req.initiatorBankVerificationNumber;
                    Resp.originatorKYCLevel = Req.originatorKYCLevel;
                    Resp.mandateReferenceNumber = Req.mandateReferenceNumber;
                    Resp.originatorAccountName = Req.originatorAccountName;
                    Resp.originatorAccountNumber = Req.originatorAccountNumber;
                    Resp.originatorBankVerificationNumber = Req.originatorBankVerificationNumber;
                    UpdateTransaction(Req.transactionId, ProcessorRespObj.SessionId, Resp);
                    JsonStr = JsonSerializer.Serialize(Resp);
                    Log.Write("Outward.Transfer", $"Response: {JsonStr}");
                }
                else
                {
                    Resp = new FundTransferResponse
                    {
                        responseCode = "06",// Change appropratly
                        amount = Req.amount,
                        beneficiaryAccountName = Req.beneficiaryAccountName,
                        beneficiaryAccountNumber = Req.beneficiaryAccountNumber,
                        beneficiaryBankVerificationNumber = Req.beneficiaryBankVerificationNumber,
                        beneficiaryKYCLevel = Req.beneficiaryKYCLevel,
                        channelCode = Req.channelCode,
                        initiatorAccountName = Req.initiatorAccountName,
                        initiatorAccountNumber = Req.initiatorAccountNumber,
                        initiatorBankVerificationNumber = Req.initiatorBankVerificationNumber,
                        originatorKYCLevel = Req.originatorKYCLevel,

                        destinationInstitutionCode = Req.destinationInstitutionCode,
                        mandateReferenceNumber = Req.mandateReferenceNumber,
                        nameEnquiryRef = Req.nameEnquiryRef,
                        narration = Req.originatorNarration,
                        paymentReference = Req.paymentReference,
                        transactionId = Req.transactionId,
                        originatorAccountName = Req.originatorAccountName,
                        originatorAccountNumber = Req.originatorAccountNumber,
                        originatorBankVerificationNumber = Req.originatorBankVerificationNumber,
                        //sessionID = Req.transactionId,
                    };
                    UpdateTransaction(Req.transactionId, ProcessorRespObj.SessionId, Resp);
                    JsonStr = JsonSerializer.Serialize(Resp);
                    Log.Write("Outward.Transfer", $"Response: {JsonStr}");

                }
                return Resp;
            }
            catch (Exception Ex)
            {
                Log.Write("Outward.Transfer", $"Err: {Ex.Message}");
                return new FundTransferResponse
                {
                    responseCode = "06",// Change appropratly
                    amount = Req.amount,
                    beneficiaryAccountName = Req.beneficiaryAccountName,
                    beneficiaryAccountNumber = Req.beneficiaryAccountNumber,
                    beneficiaryBankVerificationNumber = Req.beneficiaryBankVerificationNumber,
                    beneficiaryKYCLevel = Req.beneficiaryKYCLevel,
                    channelCode = Req.channelCode,
                    initiatorAccountName = Req.initiatorAccountName,
                    initiatorAccountNumber = Req.initiatorAccountNumber,
                    initiatorBankVerificationNumber = Req.initiatorBankVerificationNumber,
                    originatorKYCLevel = Req.originatorKYCLevel,
                    destinationInstitutionCode = Req.destinationInstitutionCode,
                    mandateReferenceNumber = Req.mandateReferenceNumber,
                    nameEnquiryRef = Req.nameEnquiryRef,
                    narration = Req.originatorNarration,
                    paymentReference = Req.paymentReference,
                    transactionId = Req.transactionId,
                    originatorAccountName = Req.originatorAccountName,
                    originatorAccountNumber = Req.originatorAccountNumber,
                    originatorBankVerificationNumber = Req.originatorBankVerificationNumber,
                    // sessionID = Req.transactionId,
                };
            }
        }

        public async Task<TranQueryResponse> GetTransaction(string TranId)
        {
            try
            {
                Log.Write("Outward.GetTransaction", $"TranId: {TranId}");
                var Db = new MomoSwitchDbContext();
                var Tran = Db.TransactionTb.SingleOrDefault(x => x.TransactionId == TranId);

                TranQueryResponse Resp;
                if (Tran != null)
                {
                    if (Tran.ResponseCode != "00")
                    {
                        //Get from processor
                        var QueryTran = await TranQuery(new TranQueryRequest { transactionId = Tran.SessionId }, Tran.Processor);

                        //UpdateDb
                        Tran.ResponseCode = QueryTran.responseCode;
                        Tran.ResponseMessage = QueryTran.responseMessage;

                        Tran.ValidateDate = DateTime.Now;
                        Db.SaveChanges();


                        Resp = new TranQueryResponse
                        {
                            code = QueryTran.responseCode,
                            sourceInstitutionCode = Tran.SourceBankCode,
                            message = Tran.ResponseMessage,
                            responseMessage = Tran.ResponseMessage,
                            sessionID = Tran.SessionId,
                            transactionId = Tran.TransactionId,
                            responseCode = QueryTran.responseCode,
                        };

                    }
                    else
                    {
                        Resp = new TranQueryResponse
                        {
                            code = Tran.ResponseCode,
                            sourceInstitutionCode = Tran.SourceBankCode,
                            message = Tran.ResponseMessage,
                            responseMessage = Tran.ResponseMessage,
                            sessionID = Tran.SessionId,
                            transactionId = Tran.TransactionId,
                            responseCode = Tran.ResponseCode,
                        };
                    }
                }
                else
                {
                    Log.Write("Outward.GetTransaction", $"Critical Issue. Transaction not found TranId:{TranId}");
                    Resp = new TranQueryResponse
                    {
                        sessionID = null,
                        transactionId = TranId,
                        responseCode = "25"
                    };
                }
                Log.Write("Outward.GetTransaction", $"Response: {JsonSerializer.Serialize(Resp)}");
                return Resp;
            }
            catch (Exception Ex)
            {
                Log.Write("Outward.GetTransaction", $"Err {Ex.Message} TranId:{TranId}");
                return new TranQueryResponse
                {
                    sessionID = null,
                    transactionId = TranId,
                    responseCode = "06"
                };
            }
        }





        public AuthResponse Reset(AuthRequest Req)
        {
            try
            {
                Log.Write($"AuthController", $"Request: {JsonSerializer.Serialize(Req)}");


                var client_id = Config["client_id"];
                var scope = Config["scope"];
                var client_secret = Config["client_secret"];
                var grant_type = Config["grant_type"];



                if (client_id != Req.client_id)
                {
                    Log.Write($"AuthController", $"Error: INVALID CLIENTID");
                    return new AuthResponse { access_token = "", status = "INVALID CLIENTID" };
                }
                else if (scope != Req.scope)
                {
                    Log.Write($"AuthController", $"Error: INVALID SCOPE");
                    return new AuthResponse { access_token = "", status = "INVALID SCOPE" };
                }
                else if (grant_type != Req.grant_type)
                {
                    Log.Write($"AuthController", $"Error: INVALID GRANT TYPE");
                    return new AuthResponse { access_token = "", status = "INVALID GRANT TYPE" };

                }
                else if (client_secret != Req.client_secret)
                {
                    Log.Write($"AuthController", $"Error: INVALID CLIENT SECRET");
                    return new AuthResponse { access_token = "", status = "INVALID CLIENT SECRET" };
                }



                //   string key = "1824a0f8-0b89-4a02-a397-db0123000d26";
                string key = Config["Jwt:Key"];
                var issuer = Config["Jwt:Issuer"];
                var audience = Config["Jwt:Audience"];

                var ClientKey = Guid.NewGuid().ToString();


                var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
                var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha512Signature);


                var Claims = new List<Claim>();
                Claims.Add(new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()));
                Claims.Add(new Claim("ClientId", client_id));
                //  Claims.Add(new Claim("Username", ClientDetails.Username));
                //Claims.Add(new Claim("Password", "9@ssm02dS"));

                //Create Security Token object by giving required parameters    
                var token = new JwtSecurityToken(
                                audience: audience,
                                issuer: issuer,
                                claims: Claims,
                                expires: DateTime.Now.AddDays(365),
                                signingCredentials: credentials);
                var jwt_token = new JwtSecurityTokenHandler().WriteToken(token);
                Log.Write($"AuthController", $"Auth: OK");
                return new AuthResponse { access_token = jwt_token, token_type = "Bearer", status = "SUCCESS", expires_in = 10292929, ext_expires_in = 993939339 };

            }
            catch (Exception Ex)
            {
                Log.Write($"AuthController", $"Error: {Ex.Message}");
                return new AuthResponse { access_token = "", status = "FAILED" };
            }
        }



        #region Pri 
        private ResponseHeader SubmitTransaction(FundTransferRequest Request, string Proccesor)
        {
            try
            {
                var Db = new MomoSwitchDbContext();

                Db.TransactionTb.Add(new TransactionTb
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
                    //SessionId = Request.transactionId,
                    SourceAccountName = Request.initiatorAccountName,
                    SourceAccountNumber = Request.initiatorAccountNumber,
                    SourceBankCode = Request.sourceInstitutionCode,
                    SourceBvn = Request.initiatorBankVerificationNumber,
                    SourceKycLevel = Request.originatorKYCLevel.ToString(),
                    TransactionId = Request.transactionId,
                    NameEnquiryRef = Request.nameEnquiryRef,
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
                if (Ex.InnerException.Message.Contains("IX_TransactionTb_TransactionId"))
                {
                    Log.Write("DataExpress.SubmitTransaction", $"TranId mismatch: TranId:{Request.transactionId}");
                    return new ResponseHeader
                    {
                        ResponseCode = "94",
                    };
                }
                else if (Ex.InnerException.Message.Contains("IX_TransactionTb_SessionId"))
                {
                    Log.Write("DataExpress.SubmitTransaction", $"TranId mismatch");
                    return new ResponseHeader
                    {
                        ResponseCode = "94",
                    };
                }
                Log.Write("DataExpress.SubmitTransaction", $"Err {Ex.InnerException?.Message} Session:{Request.transactionId}");
                Log.Write("DataExpress.SubmitTransaction", $"Failed to save Transaction: Session:{Request.transactionId}");
                Log.Write("DataExpress.SubmitTransaction", $"Err {Ex.Message} Session:{Request.transactionId}");
                return new ResponseHeader
                {
                    ResponseCode = "01",
                    ResponseMessage = "Failed saving transation"
                };
            }
        }
        private ResponseHeader UpdateTransaction(string TranId, string SessionId, FundTransferResponse Response)
        {
            try
            {
                var Db = new MomoSwitchDbContext();
                var Tran = Db.TransactionTb.SingleOrDefault(x => x.TransactionId == TranId);
                if (Tran != null)
                {
                    Tran.SessionId = SessionId;
                    Tran.PaymentDate = DateTime.Now;
                    Tran.ResponseCode = Response.responseCode;
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
        private async Task<TranQueryResponse> TranQuery(TranQueryRequest Req, string Processor)
        {
            TranQueryResponse Resp;
            try
            {
                string JsonStr = JsonSerializer.Serialize(Req);
                //  Log.Write("Outward.TranQuery", $"Request: {JsonStr}");


                var RouterDetail = SwitchRouter.Route(new Models.RouterRequest
                {
                    Processor = Processor// Pass processor
                });
                var ProcessorRequest = Transposer.ToProxyTranQueryyRequest(Req);

                var ProcessorResp = await HttpService.Call(new Models.Internals.HttpService.HttpServiceRequest
                {
                    EndPoint = RouterDetail.TranQueryUrl,
                    Method = Models.Internals.HttpService.Method.Post,
                    RequestObject = ProcessorRequest
                });

                if (ProcessorResp.ResponseHeader.ResponseCode == Models.Internals.HttpService.HttpServiceStatus.Success)
                {
                    TranQueryPxResponse ProcessorRespObj = JsonSerializer.Deserialize<TranQueryPxResponse>(ProcessorResp.Object.ToString(), Options);

                    Resp = Transposer.ToMomoTranQueryResponse(ProcessorRespObj);

                    JsonStr = JsonSerializer.Serialize(Resp);
                    Log.Write("Outward.TranQuery", $"Response: {JsonStr}");
                }
                else
                {
                    Resp = new TranQueryResponse
                    {
                        responseCode = "01",
                        responseMessage = "System challenge",
                        message = "System challenge",
                        transactionId = Req.transactionId,
                    };
                    JsonStr = JsonSerializer.Serialize(Resp);
                    Log.Write("Outward.TranQuery", $"Response: {JsonStr}");

                }
                return Resp;
            }
            catch (Exception Ex)
            {
                Log.Write("Outward.TranQuery", $"Err: {Ex.Message}");
                return new TranQueryResponse
                {
                    responseCode = "09",
                    responseMessage = "System challenge",
                    message = "System challenge",
                    transactionId = Req.transactionId,
                };
            }
        }
        #endregion
    }
}