using Momo.Common.Actions;
using Momo.Common.Models.Tables;
using SwitchPortal.Models;
using SwitchPortal.Models.DataBase;
using SwitchPortal.Models.ViewModels.Rules.Amount;
using SwitchPortal.Models.ViewModels.Rules.Priority;
using System.Text.Json;

namespace SwitchPortal.Actions.Rules
{

    public interface IAmountRule
    {
        Task<ResponseHeader> Create(AmountDetails Request);
        Task<ResponseHeader> Delete(int Id);
        Task<ResponseHeader> Edit(AmountDetails Request);
        AmountResponse Get();
        AmountResponse Get(int Id);
    }
    public class AmountRule : IAmountRule
    {
        private readonly ILog Log;
        public AmountRule(ILog log)
        {
            Log = log;
        }
        public AmountResponse Get()
        {
            try
            {
                Log.Write("AmountRule.Get", $"Request: Getting all BankSwitch");
                var Db = new MomoSwitchDbContext();
                var Data = Db.AmountRuleTb.ToList();

                var Resp = new AmountResponse()
                {
                    ResponseHeader = new Models.ResponseHeader
                    {
                        ResponseCode = "00",
                        ResponseMessage = "Successful"
                    },
                    AmountDetails = Data.Select(x => new AmountDetails
                    {
                        Id = x.Id,
                        Processor = x.Processor,
                        AmountA = x.AmountA,
                        AmountZ = x.AmountZ,
                    }).ToList()
                };

                string JsonStr = JsonSerializer.Serialize(Resp);
                Log.Write("AmountRule.Get", $"Response: {JsonStr}");
                return Resp;
            }
            catch (Exception Ex)
            {
                Log.Write("AmountRule.Get", $"Err: {Ex.Message}");
                var Resp = new AmountResponse
                {
                    ResponseHeader = new Models.ResponseHeader
                    {
                        ResponseCode = "01",
                        ResponseMessage = "System challenge"
                    }
                };
                return Resp;
            }
        }

        public AmountResponse Get(int Id)
        {
            try
            {
                Log.Write("AmountRule.Get", $"Request: Getting for {Id}");
                var Db = new MomoSwitchDbContext();
                var Data = Db.AmountRuleTb.Where(x => x.Id == Id).ToList();

                var Resp = new AmountResponse()
                {
                    ResponseHeader = new ResponseHeader
                    {
                        ResponseCode = "00",
                        ResponseMessage = "Successful"
                    },
                    AmountDetails = Data.Select(x => new AmountDetails
                    {
                        Id = x.Id,
                        Processor = x.Processor,
                        AmountA = x.AmountA,
                        AmountZ = x.AmountZ,
                    }).ToList()
                };

                string JsonStr = JsonSerializer.Serialize(Resp);
                Log.Write("AmountRule.Get", $"Response: {JsonStr}");
                return Resp;
            }
            catch (Exception Ex)
            {
                Log.Write("AmountRule.Get", $"Err: {Ex.Message}");
                var Resp = new AmountResponse()
                {
                    ResponseHeader = new ResponseHeader
                    {
                        ResponseCode = "01",
                        ResponseMessage = "System challenge"
                    }
                };
                return Resp;
            }
        }

        public async Task<ResponseHeader> Create(AmountDetails Request)
        {
            try
            {
                string JsonStr = JsonSerializer.Serialize(Request);
                Log.Write("AmountRule.Create", $"Request: {JsonStr}");
                var Db = new MomoSwitchDbContext();
                Db.AmountRuleTb.Add(new AmountRuleTb
                {
                    Processor = Request.Processor,
                    AmountA = Request.AmountA,
                    AmountZ = Request.AmountZ,
                });
                await Db.SaveChangesAsync();
                var Resp = new ResponseHeader()
                {
                    ResponseCode = "00",
                    ResponseMessage = "Successful"
                };
                JsonStr = JsonSerializer.Serialize(Resp);
                Log.Write("AmountRule.Create", $"Response: {JsonStr}");
                return Resp;
            }
            catch (Exception Ex)
            {
                Log.Write("AmountRule.Create", $"Err: {Ex.Message}");
                var Resp = new ResponseHeader()
                {
                    ResponseCode = "01",
                    ResponseMessage = "System challenge"
                };
                return Resp;
            }
        }


        public async Task<ResponseHeader> Edit(AmountDetails Request)
        {
            try
            {
                string JsonStr = JsonSerializer.Serialize(Request);
                Log.Write("AmountRule.Edit", $"Request: {JsonStr}");
                var Db = new MomoSwitchDbContext();
                var Data = Db.AmountRuleTb.Where(x => x.Id == Request.Id).SingleOrDefault();
                if (Data != null)
                {
                    Log.Write("AmountRule.Edit", $"Rule not found: Id:{Request.Id}");
                    return new ResponseHeader()
                    {
                        ResponseCode = "01",
                        ResponseMessage = "System challenge"
                    };
                }

                Data.Processor = Request.Processor;
                Data.AmountA = Request.AmountA;
                Data.AmountZ = Request.AmountZ;

                await Db.SaveChangesAsync();
                var Resp = new ResponseHeader()
                {
                    ResponseCode = "00",
                    ResponseMessage = "Successful"
                };
                JsonStr = JsonSerializer.Serialize(Resp);
                Log.Write("AmountRule.Edit", $"Response: {JsonStr}");
                return Resp;
            }
            catch (Exception Ex)
            {
                Log.Write("AmountRule.Edit", $"Err: {Ex.Message}");
                var Resp = new ResponseHeader()
                {
                    ResponseCode = "01",
                    ResponseMessage = "System challenge"
                };
                return Resp;
            }
        }

        public async Task<ResponseHeader> Delete(int Id)
        {
            try
            {
                Log.Write("AmountRule.Delete", $"Request: {Id}");
                var Db = new MomoSwitchDbContext();
                var Data = Db.AmountRuleTb.Where(x => x.Id == Id).ToList();
                Db.Remove(Data);
                await Db.SaveChangesAsync();
                var Resp = new ResponseHeader()
                {
                    ResponseCode = "00",
                    ResponseMessage = "Successful"
                };
                var JsonStr = JsonSerializer.Serialize(Resp);
                Log.Write("AmountRule.Delete", $"Response: Id:{Id} {JsonStr}");
                return Resp;
            }
            catch (Exception Ex)
            {
                Log.Write("AmountRule.Delete", $"Err: {Ex.Message}");
                var Resp = new ResponseHeader()
                {
                    ResponseCode = "01",
                    ResponseMessage = "System challenge"
                };
                return Resp;
            }
        }
    }
}
