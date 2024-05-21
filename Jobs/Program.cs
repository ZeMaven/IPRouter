using Jobs.Actions;
using Momo.Common.Actions;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHostedService<RequeryWorker>();
builder.Services.AddHostedService<AnalysisWorker>();
builder.Services.AddHostedService<ReconWorker>();
builder.Services.AddTransient<ILog, Log>();
builder.Services.AddTransient<ITransaction, Transaction>();
builder.Services.AddTransient<IReconcilation, Reconcilation>();
builder.Services.AddTransient<IExcel, Excel>();





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
