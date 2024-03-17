using MomoSwitchPortal.Models.Database;
using MomoSwitchPortal.Models;
using MomoSwitchPortal.Models.ViewModels.Rules.Priority;
using System.Text.Json;
using Momo.Common.Actions;
using Momo.Common.Models.Tables;

namespace MomoSwitchPortal.Actions.Rules
{

    public interface IPriority
    {
        Task<ResponseHeader> Create(PriorityDetails Request);
        Task<ResponseHeader> Edit(PriorityDetails Request);
        Task<ResponseHeader> Delete(int Id);
        PriorityResponse Get();
        PriorityResponse Get(int Id);
    }

    public class Priority : IPriority
    {
        private readonly ILog Log;
        public Priority(ILog log)
        {
            Log = log;
        }

        public PriorityResponse Get()
        {
            try
            {
                Log.Write("Priority.Get", $"Request: Getting all BankSwitch");
                var Db = new MomoSwitchDbContext();
                var Data = Db.PriorityTb.ToList();

                var Resp = new PriorityResponse()
                {
                    ResponseHeader = new Models.ResponseHeader
                    {
                        ResponseCode = "00",
                        ResponseMessage = "Successful"
                    },
                    PriorityDetails = Data.Select(x => new PriorityDetails
                    {
                        Id = x.Id,
                        Priority = x.Priority,
                        Rule = x.Rule,
                    }).ToList()
                };

                string JsonStr = JsonSerializer.Serialize(Resp);
                Log.Write("Priority.Get", $"Response: {JsonStr}");
                return Resp;
            }
            catch (Exception Ex)
            {
                Log.Write("Priority.Get", $"Err: {Ex.Message}");
                var Resp = new PriorityResponse()
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

        public PriorityResponse Get(int Id)
        {
            try
            {
                Log.Write("Priority.Get", $"Request: Getting for {Id}");
                var Db = new MomoSwitchDbContext();
                var Data = Db.PriorityTb.Where(x => x.Id == Id).ToList();

                var Resp = new PriorityResponse()
                {
                    ResponseHeader = new ResponseHeader
                    {
                        ResponseCode = "00",
                        ResponseMessage = "Successful"
                    },
                    PriorityDetails = Data.Select(x => new PriorityDetails
                    {
                        Id = x.Id,
                        Priority = x.Priority,
                        Rule = x.Rule,
                    }).ToList()
                };

                string JsonStr = JsonSerializer.Serialize(Resp);
                Log.Write("Priority.Get", $"Response: {JsonStr}");
                return Resp;
            }
            catch (Exception Ex)
            {
                Log.Write("Priority.Get", $"Err: {Ex.Message}");
                var Resp = new PriorityResponse()
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

        public async Task<ResponseHeader> Create(PriorityDetails Request)
        {
            try
            {
                string JsonStr = JsonSerializer.Serialize(Request);
                Log.Write("Priority.Create", $"Request: {JsonStr}");
                var Db = new MomoSwitchDbContext();
                Db.PriorityTb.Add(new PriorityTb
                {
                    Rule = Request.Rule,
                    Priority = Request.Priority,
                });
                await Db.SaveChangesAsync();
                var Resp = new ResponseHeader()
                {
                    ResponseCode = "00",
                    ResponseMessage = "Successful"
                };
                JsonStr = JsonSerializer.Serialize(Resp);
                Log.Write("Priority.Create", $"Response: {JsonStr}");
                return Resp;
            }
            catch (Exception Ex)
            {
                Log.Write("Priority.Create", $"Err: {Ex.Message}");
                var Resp = new ResponseHeader()
                {
                    ResponseCode = "01",
                    ResponseMessage = "System challenge"
                };
                return Resp;
            }
        }

        public async Task<ResponseHeader> Edit(PriorityDetails Request)
        {
            try
            {
                string JsonStr = JsonSerializer.Serialize(Request);
                Log.Write("Priority.Edit", $"Request: {JsonStr}");
                var Db = new MomoSwitchDbContext();
                var Data = Db.PriorityTb.Where(x => x.Id == Request.Id).SingleOrDefault();
                if (Data == null)
                {
                    Log.Write("Priority.Edit", $"Rule not found: Id:{Request.Id}");
                    return new ResponseHeader()
                    {
                        ResponseCode = "01",
                        ResponseMessage = "System challenge"
                    };
                }

                Data.Priority = Request.Priority;
                Data.Rule = Request.Rule;

                await Db.SaveChangesAsync();
                var Resp = new ResponseHeader()
                {
                    ResponseCode = "00",
                    ResponseMessage = "Successful"
                };
                JsonStr = JsonSerializer.Serialize(Resp);
                Log.Write("Priority.Edit", $"Response: {JsonStr}");
                return Resp;
            }
            catch (Exception Ex)
            {
                Log.Write("Priority.Edit", $"Err: {Ex.Message}");
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
                Log.Write("Priority.Delete", $"Request: {Id}");
                var Db = new MomoSwitchDbContext();
                var Data = Db.PriorityTb.SingleOrDefault(x => x.Id == Id);
                Db.PriorityTb.Remove(Data);
                await Db.SaveChangesAsync();
                var Resp = new ResponseHeader()
                {
                    ResponseCode = "00",
                    ResponseMessage = "Successful"
                };
                var JsonStr = JsonSerializer.Serialize(Resp);
                Log.Write("Priority.Delete", $"Response: Id:{Id} {JsonStr}");
                return Resp;
            }
            catch (Exception Ex)
            {
                Log.Write("Priority.Delete", $"Err: {Ex.Message}");
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
