using Microsoft.Extensions.Logging.ApplicationInsights;
using SCMM.Shared.Web;
using CommandQuery.DependencyInjection;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Web;
using Microsoft.Identity.Web.UI;
using SCMM.Azure.AI;
using SCMM.Azure.AI.Extensions;
using SCMM.Azure.ServiceBus.Extensions;
using SCMM.Azure.ServiceBus.Middleware;
using SCMM.Discord.Bot.Server.Middleware;
using SCMM.Discord.Client;
using SCMM.Discord.Client.Extensions;
using SCMM.Fixer.Client;
using SCMM.Fixer.Client.Extensions;
using SCMM.Google.Client;
using SCMM.Google.Client.Extensions;
using SCMM.Shared.Web.Extensions;
using SCMM.Shared.Web.Middleware;
using SCMM.Steam.API;
using SCMM.Steam.Client;
using SCMM.Steam.Client.Extensions;
using SCMM.Steam.Data.Store;
using System.Reflection;

await WebApplication.CreateBuilder(args)
    .ConfigureLogging()
    .ConfigureServices()
    .Build()
    .Configure()
    .RunAsync();

public static class WebApplicationExtensions
{
    public static WebApplicationBuilder ConfigureLogging(this WebApplicationBuilder builder)
    {
        builder.Logging.ClearProviders();
        builder.Logging.AddDebug();
        builder.Logging.AddConsole();
        builder.Logging.AddHtmlLogger();
        builder.Logging.AddApplicationInsights();
        builder.Logging.AddFilter<ApplicationInsightsLoggerProvider>(string.Empty, LogLevel.Warning);
        return builder;
    }

    public static WebApplicationBuilder ConfigureServices(this WebApplicationBuilder builder)
    {
        // Logging
        builder.Services.AddDatabaseDeveloperPageExceptionFilter();
        builder.Services.AddApplicationInsightsTelemetry(options =>
        {
            options.InstrumentationKey = builder.Configuration["APPINSIGHTS_INSTRUMENTATIONKEY"];
        });

        // Authentication
        builder.Services.AddAuthentication(OpenIdConnectDefaults.AuthenticationScheme)
            .AddMicrosoftIdentityWebApp(builder.Configuration.GetSection("AzureAd"));

        // Database
        builder.Services.AddDbContext<SteamDbContext>(options =>
        {
            options.UseSqlServer(builder.Configuration.GetConnectionString("SteamDbConnection"), sql =>
            {
                //sql.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery);
                sql.EnableRetryOnFailure();
            });
            options.EnableSensitiveDataLogging(AppDomain.CurrentDomain.IsDebugBuild());
            options.EnableDetailedErrors(AppDomain.CurrentDomain.IsDebugBuild());
        });

        // Service bus
        builder.Services.AddAzureServiceBus(
            builder.Configuration.GetConnectionString("ServiceBusConnection")
        );

        // 3rd party clients
        builder.Services.AddSingleton(x => builder.Configuration.GetDiscordConfiguration());
        builder.Services.AddSingleton(x => builder.Configuration.GetGoogleConfiguration());
        builder.Services.AddSingleton(x => builder.Configuration.GetSteamConfiguration());
        builder.Services.AddSingleton(x => builder.Configuration.GetFixerConfiguration());
        builder.Services.AddSingleton(x => builder.Configuration.GetAzureAiConfiguration());
        builder.Services.AddSingleton<DiscordClient>();
        builder.Services.AddSingleton<GoogleClient>();
        builder.Services.AddSingleton<SteamSession>();
        builder.Services.AddSingleton<FixerWebClient>();
        builder.Services.AddSingleton<AzureAiClient>();
        builder.Services.AddScoped<SteamWebClient>();
        builder.Services.AddScoped<SteamCommunityWebClient>();

        // Command/query/message handlers
        builder.Services.AddCommands(Assembly.GetEntryAssembly(), Assembly.Load("SCMM.Discord.API"), Assembly.Load("SCMM.Steam.API"));
        builder.Services.AddQueries(Assembly.GetEntryAssembly(), Assembly.Load("SCMM.Discord.API"), Assembly.Load("SCMM.Steam.API"));
        builder.Services.AddMessages(Assembly.GetEntryAssembly());

        // Services
        builder.Services.AddScoped<SteamService>();

        // Controllers
        builder.Services.AddControllersWithViews(options =>
        {
            var policy = new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .Build();

            options.Filters.Add(new AuthorizeFilter(policy));
        });

        // Views
        builder.Services.AddRazorPages()
             .AddMicrosoftIdentityUI();

        return builder;
    }

    public static WebApplication Configure(this WebApplication app)
    {
        if (app.Environment.IsDevelopment())
        {
            app.UseDevelopmentExceptionHandler();
            // Enable automatic DB migrations
            app.UseMigrationsEndPoint();
        }
        else
        {
            app.UseProductionExceptionHandler();
            // Force HTTPS using HSTS
            app.UseHsts();
        }

        app.UseHttpsRedirection();
        app.UseStaticFiles();

        app.UseRouting();

        app.UseAuthentication();
        app.UseAuthorization();

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}"
            );
            endpoints.MapRazorPages();
        });

        app.UseAzureServiceBusProcessor();

        app.UseDiscordClient();

        return app;
    }
}
