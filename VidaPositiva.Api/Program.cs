using FluentMigrator.Runner;
using FluentMigrator.Runner.VersionTableInfo;
using Google.Apis.Auth.AspNetCore3;
using VidaPositiva.Api.Configuration.FluentMigrator;
using VidaPositiva.Api.OAuth.Extensions;
using VidaPositiva.Api.Persistence;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using Migrations.Migrations;
using VidaPositiva.Api.Entities;
using VidaPositiva.Api.OAuth.Constants;
using VidaPositiva.Api.Persistence.Repository;
using VidaPositiva.Api.Persistence.UnitOfWork;
using VidaPositiva.Api.Services.CategoryService;
using VidaPositiva.Api.Services.PoteService;
using VidaPositiva.Api.Services.TransactionService;
using VidaPositiva.Api.Services.UserService;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

#region DatabaseConnection
builder.Services.AddScoped<IVersionTableMetaData, VersionTableMetaDataConfiguration>();

var connectionString = builder.Configuration.GetConnectionString("Default");
var migrationsConnectionString = builder.Configuration.GetConnectionString("Migrations");

builder.Services.AddDbContextPool<Context>(options =>
{
    options.UseNpgsql(connectionString);
    options.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
});

builder.Services
    .AddFluentMigratorCore()
    .ConfigureRunner(r => 
        r.AddPostgres()
            .WithGlobalConnectionString(migrationsConnectionString)
            .ScanIn(typeof(AddUserTable).Assembly)
            .For.Migrations()
    );
#endregion

#region OAuth
// Add Google Authentication
builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = GoogleOpenIdConnectDefaults.AuthenticationScheme;
        options.DefaultForbidScheme = GoogleOpenIdConnectDefaults.AuthenticationScheme;
    })
    .AddCookie(options =>
    {
        options.Cookie.HttpOnly = true;
        options.Cookie.Name = CookiesConstants.AccessCookieName;
        options.Cookie.SameSite = SameSiteMode.None;
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    })
    .AddGoogleOAuth();
#endregion

#region Repositories
builder.Services.AddScoped<IRepository<User>, Repository<User>>();
builder.Services.AddScoped<IRepository<Pote>, Repository<Pote>>();
builder.Services.AddScoped<IRepository<Category>, Repository<Category>>();
builder.Services.AddScoped<IRepository<Transaction>, Repository<Transaction>>();
#endregion

#region Services
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IPoteService, PoteService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<ITransactionService, TransactionService>();
#endregion

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}


app.UseCors(policy =>
{
    policy
        .WithOrigins("http://localhost:4200")
        .AllowAnyHeader()
        .WithMethods("GET", "POST", "PUT", "DELETE", "OPTIONS")
        .AllowCredentials();
});

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();


// NOTE: Aplica as migrações ao inicializar a aplicação
using var scope = app.Services.CreateScope();
var runner = scope.ServiceProvider.GetRequiredService<IMigrationRunner>();
runner.MigrateUp();

app.MapControllers();

app.Run();