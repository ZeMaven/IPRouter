using Momo.Common.Actions;
using RemitaProxy.Actions;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddMemoryCache();
builder.Services.AddTransient<ILog, Log>();
builder.Services.AddTransient<ICommonUtilities, CommonUtilities>();
builder.Services.AddTransient<ITransposer, Transposer>();
builder.Services.AddTransient<IRemitaOutward, RemitaOutward>();
builder.Services.AddTransient<IHttpService, HttpService>();
 

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
