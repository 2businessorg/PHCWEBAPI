using Parameters.Presentation;
using Parameters.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Serilog;
using PHCAPI.Host.Middleware;
using PHCAPI.Host.Filters;
using Audit.Presentation;
using Hangfire;
using Hangfire.SqlServer;
using Auth.Infrastructure;
using Auth.Infrastructure.Data;
using Auth.Infrastructure.Extensions;
using Auth.Presentation;
using Providers.Presentation;
using Clients.Presentation;
using Stocks.Presentation;
using Dossiers.Presentation;
using Invoices.Presentation;
using Receipts.Presentation;
using Advances.Presentation;
using Treasury.Presentation;
using Agent.Presentation;
using Currencies.Presentation;
using VatTaxes.Presentation;
using Recruitment.Presentation;
using Admin.UI;
using Microsoft.AspNetCore.Authorization;
using Shared.Kernel.Authorization;
using Auth.Infrastructure.Persistence;
using System.Threading.RateLimiting;
using PHCAPI.Host.Extensions;
using Shared.Infrastructure.DocumentTextExtraction;
using Shared.Infrastructure.Privacy.Pseudonymization;

var builder = WebApplication.CreateBuilder(args);

var disableAuthForTesting = builder.Environment.IsDevelopment() &&
    builder.Configuration.GetValue<bool>("Security:DisableAuthForTesting");

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.File(
        path: "logs/PHCAPI-.txt",
        rollingInterval: RollingInterval.Day,
        shared: true,
        flushToDiskInterval: TimeSpan.FromSeconds(1))
    .CreateLogger();

builder.Host.UseSerilog();

try
{
    builder.Services.AddProblemDetails();
    builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

    builder.Services.AddHangfire(config =>
    {
        config.SetDataCompatibilityLevel(CompatibilityLevel.Version_180);
        config.UseSimpleAssemblyNameTypeSerializer();
        config.UseRecommendedSerializerSettings();
        config.UseSqlServerStorage(
            builder.Configuration.GetConnectionString("DBconnect"),
            new SqlServerStorageOptions
            {
                CommandBatchMaxTimeout = TimeSpan.FromMinutes(5),
                SlidingInvisibilityTimeout = TimeSpan.FromMinutes(5),
                QueuePollInterval = TimeSpan.Zero,
                UseRecommendedIsolationLevel = true,
                DisableGlobalLocks = true,
                PrepareSchemaIfNecessary = true,
                SchemaName = "PHCHANGFIRE"
            });
    });

    GlobalJobFilters.Filters.Add(new AutomaticRetryAttribute
    {
        Attempts = 3,
        DelaysInSeconds = new[] { 10, 30, 60 },
        LogEvents = false,
        OnAttemptsExceeded = AttemptsExceededAction.Delete
    });

    builder.Services.AddHangfireServer(options =>
    {
        options.WorkerCount = 5;
        options.ServerTimeout = TimeSpan.FromMinutes(5);
        options.ShutdownTimeout = TimeSpan.FromMinutes(1);
    });

    builder.Services.AddAuthPresentation(builder.Configuration);
    builder.Services.AddAdminUI(builder.Configuration);

    builder.Services.AddAuthorization(options =>
    {
        options.AddPolicy(AppPolicies.InternalOnly, policy =>
            policy.RequireRole(AppRoles.Administrator, AppRoles.InternalUser, AppRoles.AuditViewer));
        options.AddPolicy(AppPolicies.AdminOnly, policy =>
            policy.RequireRole(AppRoles.Administrator));
        options.AddPolicy(AppPolicies.ApiAccess, policy =>
            policy.RequireRole(AppRoles.Administrator, AppRoles.ApiUser, AppRoles.InternalUser));
        options.AddPolicy(AppPolicies.Authenticated, policy =>
            policy.RequireAuthenticatedUser());
    });

    if (disableAuthForTesting)
    {
        builder.Services.AddSingleton<IAuthorizationHandler, AllowAllAuthorizationHandler>();
        Log.Warning("Security:DisableAuthForTesting=true. Authorization is temporarily bypassed in Development.");
    }

    builder.Services.AddConfigurableRateLimiting(builder.Configuration);
    builder.Services.AddDocumentTextExtraction(builder.Configuration);
    builder.Services.AddPseudonymization(builder.Configuration);

    builder.Services.AddParametersPresentation(builder.Configuration, enableRest: true, enableGraphQL: false);
    builder.Services.AddProvidersPresentation(builder.Configuration, enableRest: true, enableGraphQL: false);
    builder.Services.AddClientsPresentation(builder.Configuration);
    builder.Services.AddStocksPresentation(builder.Configuration);
    builder.Services.AddDossiersPresentation(builder.Configuration);
    builder.Services.AddInvoicesPresentation(builder.Configuration);
    builder.Services.AddReceiptsPresentation(builder.Configuration);
    builder.Services.AddAdvancesPresentation(builder.Configuration);
    builder.Services.AddTreasuryPresentation(builder.Configuration);
    builder.Services.AddAgentPresentation(builder.Configuration);
    builder.Services.AddCurrenciesPresentation(builder.Configuration);
    builder.Services.AddVatTaxesPresentation(builder.Configuration);
    builder.Services.AddRecruitmentPresentation(builder.Configuration);
    builder.Services.AddAuditPresentation(builder.Configuration, enableRest: true);

    var mvcBuilder = builder.Services.AddControllers(options =>
    {
    }).AddApplicationPart(typeof(Parameters.Presentation.REST.Controllers.ParametersController).Assembly)
        .AddApplicationPart(typeof(Providers.Presentation.REST.Controllers.ProvidersController).Assembly)
        .AddApplicationPart(typeof(Audit.Presentation.REST.Controllers.AuditController).Assembly)
        .AddApplicationPart(typeof(Auth.Presentation.Controllers.AuthenticateController).Assembly)
        .AddApplicationPart(typeof(Clients.Presentation.REST.Controllers.ClientsController).Assembly)
        .AddApplicationPart(typeof(Stocks.Presentation.REST.Controllers.StocksController).Assembly)
        .AddApplicationPart(typeof(Dossiers.Presentation.REST.Controllers.DossiersController).Assembly)
        .AddApplicationPart(typeof(Invoices.Presentation.Controllers.InvoicesController).Assembly)
        .AddApplicationPart(typeof(Receipts.Presentation.REST.Controllers.ReceiptsController).Assembly)
        .AddApplicationPart(typeof(Advances.Presentation.REST.Controllers.AdvancesController).Assembly)
        .AddApplicationPart(typeof(Treasury.Presentation.REST.Controllers.BankReconciliationController).Assembly)
        .AddApplicationPart(typeof(Agent.Presentation.REST.Controllers.AiChatController).Assembly)
        .AddApplicationPart(typeof(Currencies.Presentation.REST.Controllers.CurrenciesController).Assembly)
        .AddApplicationPart(typeof(VatTaxes.Presentation.REST.Controllers.VatTaxesController).Assembly)
        .AddApplicationPart(typeof(Recruitment.Presentation.REST.Controllers.RecruitmentIaController).Assembly);

    builder.Services.AddEndpointsApiExplorer();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Error during service configuration: {Message}", ex.Message);
    if (ex.InnerException != null)
        Log.Fatal(ex.InnerException, "Inner exception: {Message}", ex.InnerException.Message);
    throw;
}

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new()
    {
        Title = "PHCAPI",
        Version = "v1",
        Description = "PHC API",
        Contact = new()
        {
            Name = "2Business Team",
            Email = "suporte.mz@2business-si.com"
        }
    });

    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
        c.IncludeXmlComments(xmlPath);

    foreach (var xml in new[] { "Parameters.Presentation.xml", "Audit.Presentation.xml", "Auth.Presentation.xml", "Dossiers.Presentation.xml", "Stocks.Presentation.xml", "Tables.Presentation.xml", "Currencies.Presentation.xml" })
    {
        var p = Path.Combine(AppContext.BaseDirectory, xml);
        if (File.Exists(p)) c.IncludeXmlComments(p);
    }

    c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Bearer {token}\"",
        Name = "Authorization",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT"
    });

    c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

builder.Services.AddCors(options =>
{
    var allowedOrigins = builder.Configuration.GetSection("AllowedOrigins").Get<string[]>() ?? Array.Empty<string>();
    if (allowedOrigins.Length == 0)
        Log.Warning("No AllowedOrigins configured in appsettings.json. CORS will block all origins.");
    else
        Log.Information("CORS configured with {Count} allowed origins: {Origins}", allowedOrigins.Length, string.Join(", ", allowedOrigins));

    options.AddPolicy("SecureCors", policy =>
    {
        policy.WithOrigins(allowedOrigins)
              .AllowCredentials()
              .WithMethods("GET", "POST", "PUT", "DELETE", "OPTIONS")
              .WithHeaders("Content-Type", "Authorization", "X-Requested-With", "Accept", "Origin")
              .SetIsOriginAllowedToAllowWildcardSubdomains();
    });
});

WebApplication app;
try { app = builder.Build(); }
catch (Exception ex)
{
    Log.Fatal(ex, "Failed to build application: {Message}", ex.Message);
    if (ex is AggregateException aggEx)
        foreach (var innerEx in aggEx.InnerExceptions)
            Log.Fatal(innerEx, "Inner exception: {Message}", innerEx.Message);
    Log.CloseAndFlush();
    throw;
}

await app.UseAuthDatabaseAsync();
app.UseExceptionHandler();
app.UseMiddleware<CorrelationIdMiddleware>();
app.UseMiddleware<ResponseLoggingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "PHCAPI v1");
        c.RoutePrefix = string.Empty;
    });
    try
    {
        app.UseHangfireDashboard("/hangfire", new DashboardOptions
        {
            Authorization = new[] { new HangfireAuthorizationFilter() },
            StatsPollingInterval = 5000,
            DisplayStorageConnectionString = false,
            DashboardTitle = "PHCAPI - Background Jobs"
        });
    }
    catch (Exception ex)
    {
        Log.Warning(ex, "Failed to initialize Hangfire Dashboard - continuing without it");
    }
}

if (!app.Environment.IsDevelopment())
    app.UseHttpsRedirection();

app.UseRouting();
app.UseCors("SecureCors");
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();
app.UseMiddleware<ModuleAuthorizationMiddleware>();
app.UseAdminUI();
app.MapControllers();
app.MapAgentMcp();
app.MapAdminUI();

app.MapGet("/Admin", (HttpContext context) => Results.Redirect("/Admin/Users")).ExcludeFromDescription();
app.MapGet("/", (HttpContext context) =>
{
    if (context.User.Identity?.IsAuthenticated == true)
        return Results.Redirect("/Admin/Users");
    return Results.Redirect("/Admin/Account/Login");
}).ExcludeFromDescription();

try { app.Run(); }
catch (Exception ex) { Log.Fatal(ex, "Application start-up failed"); }
finally { }
