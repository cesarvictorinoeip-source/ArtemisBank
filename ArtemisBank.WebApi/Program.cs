using ArtemisBank.Core.Application;
using ArtemisBank.Infrastructure.Identity;
using ArtemisBank.Infrastructure.Identity.Entities;
using ArtemisBank.Infrastructure.Identity.Seeds;
using ArtemisBank.Infrastructure.Persistence;
using ArtemisBank.Infrastructure.Shared;
using ArtemisBank.WebApi.Extensions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers()
    .AddJsonOptions(opt =>
    {
        opt.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

// Layers IoC
builder.Services.AddApplicationLayerIoc();
builder.Services.AddPersistenceLayerIoc(builder.Configuration);
builder.Services.AddIdentityLayerIoc(builder.Configuration);
builder.Services.AddSharedLayerIoc(builder.Configuration);

// Swagger y Versioning
builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddHealthChecks();
builder.Services.AddApiVersioningExtension();
builder.Services.AddSwaggerExtension();

// JWT
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero,
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["SecretKey"]!))
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
}

if (app.Environment.IsDevelopment())
{
    app.UseSwaggerExtension(app);
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.UseHealthChecks("/health");
app.MapControllers();
await app.RunAsync();