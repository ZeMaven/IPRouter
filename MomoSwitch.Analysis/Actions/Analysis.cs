using Momo.Common.Actions;
using MomoSwitch.Analysis.Models;
using System.Text.Json;
using MomoSwitch.Analysis.Models.DataBase;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace MomoSwitch.Analysis.Actions
{
    public interface IAnalysis
    {
        List<Perfomance> GetPerfomance();
    }

    [Authorize]
    public class Analysis : IAnalysis
    {
        private readonly ILog _log;

        public Analysis(ILog log)
        {
            _log = log;
        }

        public List<Perfomance> GetPerfomance()
        {
            try
            {
                _log.Write($"Outward.GetPerfomance", $"Request Performance");

                var Db = new MomoSwitchDbContext();
                var Data = Db.PerformanceTb.AsNoTracking().Where(x => x.BankCode != "120003").Select(x => new Perfomance
                {
                    BankCode = x.BankCode,
                    BankName = x.BankName,
                    Rate = x.Rate,
                    Remark = x.Remark,
                    Time = x.Time,
                }).ToList();                
                _log.Write($"GetPerfomance", $"Response: {JsonSerializer.Serialize(Data)}");
                return Data;

            }
            catch (Exception Ex)
            {
                _log.Write($"Outward.GetPerfomance", $"Error: {Ex.Message}");
                return new List<Perfomance> { new Perfomance { } };
            }
        }
    }
}