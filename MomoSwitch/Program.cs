
using Microsoft.EntityFrameworkCore;
using MomoSwitch.Actions;
using MomoSwitch.Models.DataBase;

namespace MomoSwitch
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            builder.Services.AddMemoryCache();
            builder.Services.AddTransient<IUtilities, Utilities>();
            builder.Services.AddTransient<ISwitchRouter, SwitchRouter>();
            builder.Services.AddTransient<ITransposer, Transposer>();
            builder.Services.AddTransient<IOutward, Outward>();
            builder.Services.AddTransient<IHttpService, HttpService>();
            builder.Services.AddTransient<ILog, Log>();
            
            var app = builder.Build();
            using (var client = new MomoSwitchDbContext())
            {
                // client.Database.EnsureCreated(); //No migration | No update
                await client.Database.MigrateAsync(); //Uses Migration, therefore it updates
            }

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}
