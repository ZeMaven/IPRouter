using Momo.Common.Actions;
using NipOutwardProxy.Actions;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddTransient<IXmlConverter, XmlConverter>();
builder.Services.AddTransient<IHttpService, HttpService>();
builder.Services.AddTransient<INipOutward, NipOutward>();
builder.Services.AddTransient<IPgp, Pgp>();
builder.Services.AddTransient<ILog, Log>();
builder.Services.AddTransient<ICommonUtilities, CommonUtilities>();


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
