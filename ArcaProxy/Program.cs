using ArcaProxy.Actions;
using Momo.Common.Actions;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddTransient<ILog, Log>();
builder.Services.AddTransient<ICommonUtilities, CommonUtilities>();
builder.Services.AddTransient<ITransposer, Transposer>();
builder.Services.AddTransient<IArcaOutward, ArcaOutward>();
builder.Services.AddTransient<IHttpService, HttpService>();
builder.Services.AddTransient<IBankCodes, BankCodes>();

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
