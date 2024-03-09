using Momo.Common.Actions;
using SwitchPortal.Models;
using SwitchPortal.Models.DataBase;

using SwitchPortal.Models.ViewModels.Rules.BankSwitch;
using Momo.Common.Models.Tables;
using System.Text.Json;

namespace SwitchPortal.Actions.Rules
{
    public interface IBankSwitch
    {
        Task<ResponseHeader> Create(BankSwitchDetails Request);
        Task<ResponseHeader> Delete(int Id);
        BankSwitchResponse Get();
        BankSwitchResponse Get(int Id);
    }

    public class BankSwitch : IBankSwitch
    {
        private readonly ILog Log;
        public BankSwitch(ILog log)
        {
            Log = log;
        }
        public BankSwitchResponse Get()
        {
            try
            {
                Log.Write("BankSwitch.Get", $"Request: Getting all BankSwitch");
                var Db = new MomoSwitchDbContext();
                var Data = Db.BankSwitchTb.ToList();

                var Resp = new BankSwitchResponse()
                {
                    ResponseHeader = new Models.ResponseHeader
                    {
                        ResponseCode = "00",
                        ResponseMessage = "Successful"
                    },
                    BankSwitchDetails = Data.Select(x => new BankSwitchDetails
                    {
                        Id = x.Id,
                        Processor = x.Processor,
                        BankCode = x.BankCode,
                    }).ToList()
                };

                string JsonStr = JsonSerializer.Serialize(Resp);
                Log.Write("BankSwitch.Get", $"Response: {JsonStr}");
                return Resp;
            }
            catch (Exception Ex)
            {
                Log.Write("BankSwitch.Get", $"Err: {Ex.Message}");
                var Resp = new BankSwitchResponse()
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

        public BankSwitchResponse Get(int Id)
        {
            try
            {
                Log.Write("BankSwitch.Get", $"Request: Getting for {Id}");
                var Db = new MomoSwitchDbContext();
                var Data = Db.BankSwitchTb.Where(x => x.Id == Id).ToList();

                var Resp = new BankSwitchResponse()
                {
                    ResponseHeader = new ResponseHeader
                    {
                        ResponseCode = "00",
                        ResponseMessage = "Successful"
                    },
                    BankSwitchDetails = Data.Select(x => new BankSwitchDetails
                    {
                        Id = x.Id,
                        Processor = x.Processor,
                        BankCode = x.BankCode,
                    }).ToList()
                };

                string JsonStr = JsonSerializer.Serialize(Resp);
                Log.Write("BankSwitch.Get", $"Response: {JsonStr}");
                return Resp;
            }
            catch (Exception Ex)
            {
                Log.Write("BankSwitch.Get", $"Err: {Ex.Message}");
                var Resp = new BankSwitchResponse()
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

        public async Task<ResponseHeader> Create(BankSwitchDetails Request)
        {
            try
            {
                string JsonStr = JsonSerializer.Serialize(Request);
                Log.Write("BankSwitch.Create", $"Request: {JsonStr}");
                var Db = new MomoSwitchDbContext();
                Db.BankSwitchTb.Add(new BankSwitchTb
                {
                    Processor = Request.Processor,
                    BankCode = Request.BankCode,
                });
                await Db.SaveChangesAsync();
                var Resp = new ResponseHeader()
                {
                    ResponseCode = "00",
                    ResponseMessage = "Successful"
                };
                JsonStr = JsonSerializer.Serialize(Resp);
                Log.Write("BankSwitch.Create", $"Response: {JsonStr}");
                return Resp;
            }
            catch (Exception Ex)
            {
                Log.Write("BankSwitch.Create", $"Err: {Ex.Message}");
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
                Log.Write("BankSwitch.Delete", $"Request: {Id}");
                var Db = new MomoSwitchDbContext();
                var Data = Db.BankSwitchTb.Where(x => x.Id == Id).ToList();
                Db.Remove(Data);
                await Db.SaveChangesAsync();
                var Resp = new ResponseHeader()
                {
                    ResponseCode = "00",
                    ResponseMessage = "Successful"
                };
                var JsonStr = JsonSerializer.Serialize(Resp);
                Log.Write("BankSwitch.Delete", $"Response: Id:{Id} {JsonStr}");
                return Resp;
            }
            catch (Exception Ex)
            {
                Log.Write("BankSwitch.Delete", $"Err: {Ex.Message}");
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
