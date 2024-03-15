using MomoSwitchPortal.Models.Database;
using MomoSwitchPortal.Models;
using MomoSwitchPortal.Models.ViewModels.Rules.Time;
using System.Text.Json;
using Momo.Common.Actions;
using Momo.Common.Models.Tables;

namespace MomoSwitchPortal.Actions.Rules
{
    public interface ITimeRule
    {
        Task<ResponseHeader> Create(TimeDetails Request);
        Task<ResponseHeader> Delete(int Id);
        TimeRuleResponse Get();
        TimeRuleResponse Get(int Id);
    }

    public class TimeRule : ITimeRule
    {
        private readonly ILog Log;
        public TimeRule(ILog log)
        {
            Log = log;
        }
        public TimeRuleResponse Get()
        {
            try
            {
                Log.Write("TimeRule.Get", $"Request: Getting all timerule");
                var Db = new MomoSwitchDbContext();
                var Data = Db.TimeRuleTb.ToList();

                var Resp = new TimeRuleResponse()
                {
                    ResponseHeader = new ResponseHeader
                    {
                        ResponseCode = "00",
                        ResponseMessage = "Successful"
                    },
                    TimeDetails = Data.Select(x => new TimeDetails
                    {
                        Id = x.Id,
                        Processor = x.Processor,
                        TimeA = x.TimeA,
                        TimeZ = x.TimeZ,
                    }).ToList()
                };

                string JsonStr = JsonSerializer.Serialize(Resp);
                Log.Write("TimeRule.Get", $"Response: {JsonStr}");
                return Resp;
            }
            catch (Exception Ex)
            {
                Log.Write("TimeRule.Get", $"Err: {Ex.Message}");
                var Resp = new TimeRuleResponse()
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

        public TimeRuleResponse Get(int Id)
        {
            try
            {
                Log.Write("TimeRule.Get", $"Request: Getting for {Id}");
                var Db = new MomoSwitchDbContext();
                var Data = Db.TimeRuleTb.Where(x => x.Id == Id).ToList();

                var Resp = new TimeRuleResponse()
                {
                    ResponseHeader = new ResponseHeader
                    {
                        ResponseCode = "00",
                        ResponseMessage = "Successful"
                    },
                    TimeDetails = Data.Select(x => new TimeDetails
                    {
                        Id = x.Id,
                        Processor = x.Processor,
                        TimeA = x.TimeA,
                        TimeZ = x.TimeZ,
                    }).ToList()
                };

                string JsonStr = JsonSerializer.Serialize(Resp);
                Log.Write("TimeRule.Get", $"Response: {JsonStr}");
                return Resp;
            }
            catch (Exception Ex)
            {
                Log.Write("TimeRule.Get", $"Err: {Ex.Message}");
                var Resp = new TimeRuleResponse()
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

        public async Task<ResponseHeader> Create(TimeDetails Request)
        {
            try
            {
                string JsonStr = JsonSerializer.Serialize(Request);
                Log.Write("TimeRule.Create", $"Request: {JsonStr}");
                var Db = new MomoSwitchDbContext();
                Db.TimeRuleTb.Add(new Momo.Common.Models.Tables.TimeRuleTb
                {
                    Processor = Request.Processor,
                    TimeA = Request.TimeA,
                    TimeZ = Request.TimeZ,
                });
                await Db.SaveChangesAsync();
                var Resp = new ResponseHeader()
                {
                    ResponseCode = "00",
                    ResponseMessage = "Successful"
                };
                JsonStr = JsonSerializer.Serialize(Resp);
                Log.Write("TimeRule.Create", $"Response: {JsonStr}");
                return Resp;
            }
            catch (Exception Ex)
            {
                Log.Write("TimeRule.Create", $"Err: {Ex.Message}");
                var Resp = new ResponseHeader()
                {
                    ResponseCode = "01",
                    ResponseMessage = "System challenge"
                };
                return Resp;
            }
        }

        public async Task<ResponseHeader> Edit(TimeDetails Request)
        {
            try
            {
                string JsonStr = JsonSerializer.Serialize(Request);
                Log.Write("TimeRule.Edit", $"Request: {JsonStr}");
                var Db = new MomoSwitchDbContext();
                var Data = Db.TimeRuleTb.Where(x => x.Id == Request.Id).SingleOrDefault();
                if (Data != null)
                {
                    Log.Write("TimeRule.Edit", $"Rule not found: Id:{Request.Id}");
                    return new ResponseHeader()
                    {
                        ResponseCode = "01",
                        ResponseMessage = "System challenge"
                    };
                }

                Data.Processor = Request.Processor;
                Data.TimeZ = Request.TimeZ;
                Data.TimeA = Request.TimeA;

                await Db.SaveChangesAsync();
                var Resp = new ResponseHeader()
                {
                    ResponseCode = "00",
                    ResponseMessage = "Successful"
                };
                JsonStr = JsonSerializer.Serialize(Resp);
                Log.Write("TimeRule.Edit", $"Response: {JsonStr}");
                return Resp;
            }
            catch (Exception Ex)
            {
                Log.Write("TimeRule.Edit", $"Err: {Ex.Message}");
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
                Log.Write("TimeRule.Delete", $"Request: {Id}");
                var Db = new MomoSwitchDbContext();
                var Data = Db.TimeRuleTb.Where(x => x.Id == Id).ToList();
                Db.Remove(Data);
                await Db.SaveChangesAsync();
                var Resp = new ResponseHeader()
                {
                    ResponseCode = "00",
                    ResponseMessage = "Successful"
                };
                var JsonStr = JsonSerializer.Serialize(Resp);
                Log.Write("TimeRule.Delete", $"Response: Id:{Id} {JsonStr}");
                return Resp;
            }
            catch (Exception Ex)
            {
                Log.Write("TimeRule.Delete", $"Err: {Ex.Message}");
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
