﻿using MomoSwitchPortal.Models.Database;
using MomoSwitchPortal.Models;
using MomoSwitchPortal.Models.ViewModels.Rules.Switch;
using System.Text.Json;
using Momo.Common.Actions;
using Momo.Common.Models.Tables;
using Microsoft.EntityFrameworkCore;

namespace MomoSwitchPortal.Actions.Rules
{
    public interface ISwitch
    {
        Task<ResponseHeader> Create(SwitchDetails Request);
        Task<ResponseHeader> Edit(SwitchDetails Request);
        Task<ResponseHeader> Delete(int Id);
        SwitchResponse Get();
        SwitchResponse Get(int Id);
    }
    public class Switch : ISwitch
    {
        private readonly ILog Log;
        public Switch(ILog log)
        {
            Log = log;
        }

        public SwitchResponse Get()
        {
            try
            {
                Log.Write("Switch.Get", $"Request: Getting all BankSwitch");
                var Db = new MomoSwitchDbContext();
                var Data = Db.SwitchTb.ToList();

                var Resp = new SwitchResponse()
                {
                    ResponseHeader = new Models.ResponseHeader
                    {
                        ResponseCode = "00",
                        ResponseMessage = "Successful"
                    },
                    SwitchDetails = Data.Select(x => new SwitchDetails
                    {
                        Id = x.Id,
                        IsActive = x.IsActive,
                        TranQueryUrl = x.TranQueryUrl,
                        TransferUrl = x.TransferUrl,
                        NameEnquiryUrl = x.NameEnquiryUrl,
                        IsDefault = x.IsDefault,
                        Processor = x.Processor,
                        DailyLimit = x.DailyLimit
                    }).ToList()
                };

                string JsonStr = JsonSerializer.Serialize(Resp);
                Log.Write("Switch.Get", $"Response: {JsonStr}");
                return Resp;
            }
            catch (Exception Ex)
            {
                Log.Write("Switch.Get", $"Err: {Ex.Message}");
                var Resp = new SwitchResponse()
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

        public SwitchResponse Get(int Id)
        {
            try
            {
                Log.Write("Switch.Get", $"Request: Getting for {Id}");
                var Db = new MomoSwitchDbContext();
                var Data = Db.SwitchTb.Where(x => x.Id == Id).ToList();

                var Resp = new SwitchResponse()
                {
                    ResponseHeader = new ResponseHeader
                    {
                        ResponseCode = "00",
                        ResponseMessage = "Successful"
                    },
                    SwitchDetails = Data.Select(x => new SwitchDetails
                    {
                        Id = x.Id,
                        IsActive = x.IsActive,
                        TranQueryUrl = x.TranQueryUrl,
                        TransferUrl = x.TransferUrl,
                        NameEnquiryUrl = x.NameEnquiryUrl,
                        IsDefault = x.IsDefault,
                        Processor = x.Processor
                    }).ToList()
                };

                string JsonStr = JsonSerializer.Serialize(Resp);
                Log.Write("Switch.Get", $"Response: {JsonStr}");
                return Resp;
            }
            catch (Exception Ex)
            {
                Log.Write("Switch.Get", $"Err: {Ex.Message}");
                var Resp = new SwitchResponse()
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

        public async Task<ResponseHeader> Create(SwitchDetails Request)
        {
            try
            {
                string JsonStr = JsonSerializer.Serialize(Request);
                Log.Write("Switch.Create", $"Request: {JsonStr}");
                var Db = new MomoSwitchDbContext();
                var processorExists = await Db.SwitchTb.SingleOrDefaultAsync(x => x.Processor.ToLower() == Request.Processor.ToLower());

                if(processorExists != null)
                {
                    return new ResponseHeader()
                    {
                        ResponseCode = "01",
                        ResponseMessage = $"Processor with name {Request.Processor} already exists."
                    };
                }
                Db.SwitchTb.Add(new SwitchTb
                {
                    IsActive = Request.IsActive,
                    IsDefault = Request.IsDefault,
                    Processor = Request.Processor,
                    NameEnquiryUrl = Request.NameEnquiryUrl,
                    TransferUrl = Request.TransferUrl,
                    TranQueryUrl = Request.TranQueryUrl,
                    DailyLimit = Request.DailyLimit
                });
                await Db.SaveChangesAsync();
                var Resp = new ResponseHeader()
                {
                    ResponseCode = "00",
                    ResponseMessage = "Successful"
                };
                JsonStr = JsonSerializer.Serialize(Resp);
                Log.Write("Switch.Create", $"Response: {JsonStr}");
                return Resp;
            }
            catch (Exception Ex)
            {
                Log.Write("Switch.Create", $"Err: {Ex.Message}");
                var Resp = new ResponseHeader()
                {
                    ResponseCode = "01",
                    ResponseMessage = "System challenge"
                };
                return Resp;
            }
        }

        public async Task<ResponseHeader> Edit(SwitchDetails Request)
        {
            try
            {
                string JsonStr = JsonSerializer.Serialize(Request);
                Log.Write("Switch.Edit", $"Request: {JsonStr}");
                var Db = new MomoSwitchDbContext();
                var Data = Db.SwitchTb.Where(x => x.Id == Request.Id).SingleOrDefault();
                if (Data == null)
                {
                    Log.Write("Switch.Edit", $"Rule not found: Id:{Request.Id}");
                    return new ResponseHeader()
                    {
                        ResponseCode = "01",
                        ResponseMessage = "System challenge"
                    };
                }

                var processorExists = await Db.SwitchTb.SingleOrDefaultAsync(x => x.Processor.ToLower() == Request.Processor.ToLower().Trim());

                if (processorExists != null && processorExists.Id != Request.Id)
                {
                    return new ResponseHeader()
                    {
                        ResponseCode = "01",
                        ResponseMessage = $"Processor with name {Request.Processor} already exists."
                    };
                }

                var currentDefault = await Db.SwitchTb.SingleOrDefaultAsync(x => x.IsDefault);

                if (Request.IsDefault && currentDefault != null)
                {
                    if(currentDefault.Id != Request.Id)
                    {
                        currentDefault.IsDefault = false;
                    }
                }
                Data.TranQueryUrl = Request.TranQueryUrl;
                Data.TransferUrl = Request.TransferUrl;
                Data.NameEnquiryUrl = Request.NameEnquiryUrl;
                Data.Processor = Request.Processor;
                Data.DailyLimit = Request.DailyLimit;
                Data.IsDefault = Request.IsDefault;
                Data.IsActive = Request.IsActive;

                await Db.SaveChangesAsync();
                var Resp = new ResponseHeader()
                {
                    ResponseCode = "00",
                    ResponseMessage = "Successful"
                };
                JsonStr = JsonSerializer.Serialize(Resp);
                Log.Write("Switch.Edit", $"Response: {JsonStr}");
                return Resp;
            }
            catch (Exception Ex)
            {
                Log.Write("Switch.Edit", $"Err: {Ex.Message}");
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
                Log.Write("Switch.Delete", $"Request: {Id}");
                var Db = new MomoSwitchDbContext();
                var Data = await Db.SwitchTb.SingleOrDefaultAsync(x => x.Id == Id);
                Db.SwitchTb.Remove(Data);
                await Db.SaveChangesAsync();
                var Resp = new ResponseHeader()
                {
                    ResponseCode = "00",
                    ResponseMessage = "Successful"
                };
                var JsonStr = JsonSerializer.Serialize(Resp);
                Log.Write("Switch.Delete", $"Response: Id:{Id} {JsonStr}");
                return Resp;
            }
            catch (Exception Ex)
            {
                Log.Write("Switch.Delete", $"Err: {Ex.Message}");
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
