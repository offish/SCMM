using CommandQuery.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SCMM.Azure.ServiceBus.Extensions;
using SCMM.Steam.API;
using SCMM.Steam.Client;
using SCMM.Steam.Client.Extensions;
using SCMM.Steam.Data.Store;
using System.Reflection;

await new HostBuilder()
    .ConfigureFunctionsWorkerDefaults()
    .ConfigureServices()
    .Build()
    .RunAsync();

public static class HostExtensions
{
    public static IHostBuilder ConfigureServices(this IHostBuilder builder)
    {
        return builder.ConfigureServices(services =>
        {
            // Database
            services.AddDbContext<SteamDbContext>(options =>
            {
                options.UseSqlServer(Environment.GetEnvironmentVariable("SteamDbConnection"), sql =>
                {
                    //sql.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery);
                    sql.EnableRetryOnFailure();
                });
            });

            // 3rd party clients
            services.AddSingleton((services) =>
            {
                var configuration = services.GetService<IConfiguration>();
                return configuration.GetSteamConfiguration();
            });
            services.AddSingleton<SteamSession>();
            services.AddScoped<SteamCommunityWebClient>();
            services.AddScoped<SteamWorkshopDownloaderWebClient>();

            // Command/query/message handlers
            services.AddCommands(Assembly.GetEntryAssembly(), Assembly.Load("SCMM.Steam.API"));
            services.AddQueries(Assembly.GetEntryAssembly(), Assembly.Load("SCMM.Steam.API"));
            services.AddMessages(Assembly.GetEntryAssembly());

            // Services
            services.AddScoped<SteamService>();

        });
    }
}
