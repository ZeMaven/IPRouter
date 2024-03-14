using Microsoft.EntityFrameworkCore;
using Momo.Common.Models.Tables;
using System.Collections.Generic;
using System.Transactions;

namespace MomoSwitch.Models.DataBase
{
    public class MomoSwitchDbContext:DbContext
    {
        public MomoSwitchDbContext()
        {
        }

        public MomoSwitchDbContext(DbContextOptions<MomoSwitchDbContext> options) : base(options) { }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                IConfigurationRoot configuration = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory()).AddJsonFile("appsettings.json", optional: true, reloadOnChange: true).AddEnvironmentVariables().Build();
                optionsBuilder.UseSqlServer(configuration.GetConnectionString("ConnStr"));
            }
        }

        public DbSet<AmountRuleTb> AmountRuleTb { get; set; }
        public DbSet<BankSwitchTb> BankSwitchTb { get; set; }
        public DbSet<PriorityTb> PriorityTb { get; set; }
        public DbSet<SwitchTb> SwitchTb { get; set; }
        public DbSet<TimeRuleTb> TimeRuleTb { get; set; }
        public DbSet<TransactionTb> TransactionTb { get; set; }
        public DbSet<PortalUserTb> PortalUserTb { get; set; }
    }
}