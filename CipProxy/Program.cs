
using CipProxy.Actions;
using Momo.Common.Actions;

namespace CipProxy
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();
            builder.Services.AddTransient<ILog, Log>();
            builder.Services.AddTransient<ICommonUtilities, CommonUtilities>();
            builder.Services.AddTransient<ICipOutward, CipOutward>();
            builder.Services.AddTransient<ICipInward, CipInward>();            
            builder.Services.AddTransient<IHttpService, HttpService>();
            builder.Services.AddTransient<IPgp, Pgp>();

            var app = builder.Build();

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
