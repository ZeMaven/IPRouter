using AspNetCoreHero.ToastNotification;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Momo.Common.Actions;
using MomoSwitchPortal.Actions;
using MomoSwitchPortal.Actions.Rules;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddScoped<IAccount, Account>();
builder.Services.AddScoped<ILog, Log>();
builder.Services.AddScoped<IUser, User>();
builder.Services.AddScoped<ISwitch, Switch>();
builder.Services.AddScoped<IAmountRule, AmountRule>();
builder.Services.AddScoped<IBankSwitch, BankSwitch>();
builder.Services.AddScoped<ITimeRule, TimeRule>();
builder.Services.AddScoped<IPriority, Priority>();
builder.Services.AddScoped<IEmail, Email>();
builder.Services.AddScoped<IHome, Home>();
builder.Services.AddScoped<IProfile, Profile>();
builder.Services.AddScoped<ICommonUtilities, CommonUtilities>();

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(option =>
    {
        option.LoginPath = "/account/signin";
        option.ExpireTimeSpan = TimeSpan.FromHours(3);
        option.AccessDeniedPath = "/AccessDenied";        
    });


builder.Services.AddNotyf(config =>
{
    config.DurationInSeconds = 5;
    config.IsDismissable = true;
    config.Position = NotyfPosition.TopRight;
});//Customize: _notyfService.Custom("Custom Notification...", 10, "#B500FF", "fa fa-home");


builder.Services.Configure<IdentityOptions>(options =>
{
    // Default Password settings.
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequireUppercase = true;
    options.Password.RequiredLength = 6;
    options.Password.RequiredUniqueChars = 1;
});

builder.Services.AddHttpContextAccessor();

var app = builder.Build();

app.UsePathBase("/msrportal");

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();

app.Run();
