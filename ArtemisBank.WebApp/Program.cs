using ArtemisBank.Core.Application;
using ArtemisBank.Infrastructure.Identity;
using ArtemisBank.Infrastructure.Identity.Entities;
using ArtemisBank.Infrastructure.Identity.Seeds;
using ArtemisBank.Infrastructure.Persistence;
using ArtemisBank.Infrastructure.Shared;
using ArtemisBank.WebApp.Jobs;
using Hangfire;
using Microsoft.AspNetCore.Identity;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllersWithViews();

// Culture
var cultureInfo = new System.Globalization.CultureInfo("en-US");
System.Globalization.CultureInfo.DefaultThreadCurrentCulture = cultureInfo;
System.Globalization.CultureInfo.DefaultThreadCurrentUICulture = cultureInfo;

// Layers IoC
builder.Services.AddApplicationLayerIoc();
builder.Services.AddPersistenceLayerIoc(builder.Configuration);
builder.Services.AddIdentityLayerIoc(builder.Configuration);
builder.Services.AddSharedLayerIoc(builder.Configuration);

// Hangfire
builder.Services.AddHangfire(config => config
    .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
    .UseSimpleAssemblyNameTypeSerializer()
    .UseRecommendedSerializerSettings()
    .UseSqlServerStorage(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddHangfireServer();
builder.Services.AddScoped<LoanInstallmentJob>();

// Cookie configuration
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Login";
    options.AccessDeniedPath = "/AccessDenied/Index";
    options.Events.OnRedirectToLogin = context =>
    {
        context.Response.Redirect("/Login?message=nopermiso");
        return Task.CompletedTask;
    };
});

var app = builder.Build();

// Seeds
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
    var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
    await DefaultRoles.SeedAsync(roleManager);
    await DefaultAdminUser.SeedAsync(userManager);
    await DefaultCashierUser.SeedAsync(userManager);
    await DefaultClientUser.SeedAsync(userManager);
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

// Hangfire Dashboard
app.UseHangfireDashboard("/hangfire", new DashboardOptions
{
    Authorization = new[] { new HangfireAuthorizationFilter() }
});

// Hangfire Jobs
RecurringJob.AddOrUpdate<LoanInstallmentJob>(
    "check-late-installments",
    job => job.CheckLateInstallments(),
    Cron.Daily);

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Login}/{action=Index}/{id?}");

app.Run();