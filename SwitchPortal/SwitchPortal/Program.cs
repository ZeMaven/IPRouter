using Momo.Common.Actions;
using SwitchPortal.Actions.Rules;
using SwitchPortal.Client.Pages;
using SwitchPortal.Components;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddInteractiveWebAssemblyComponents();


builder.Services.AddSingleton<ILog, Log>();
builder.Services.AddSingleton<IAmountRule, AmountRule>();
builder.Services.AddSingleton<IBankSwitch, BankSwitch>();
builder.Services.AddSingleton<IPriority, Priority>();
builder.Services.AddSingleton<ISwitch, Switch>();
builder.Services.AddSingleton<ITimeRule, TimeRule>();



var app = builder.Build();


 

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(SwitchPortal.Client._Imports).Assembly);

app.Run();
