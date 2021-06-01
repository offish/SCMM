﻿using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SCMM.Steam.API;
using SCMM.Steam.Client;
using SCMM.Steam.Data.Models.Community.Requests.Json;
using SCMM.Steam.Data.Store;
using SCMM.Steam.Job.Server.Jobs.Cron;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SCMM.Steam.Job.Server.Jobs
{
    public class UpdateMarketItemOrdersJob : CronJobService
    {
        private readonly ILogger<UpdateMarketItemOrdersJob> _logger;
        private readonly IServiceScopeFactory _scopeFactory;

        public UpdateMarketItemOrdersJob(IConfiguration configuration, ILogger<UpdateMarketItemOrdersJob> logger, IServiceScopeFactory scopeFactory)
            : base(logger, configuration.GetJobConfiguration<UpdateMarketItemOrdersJob>())
        {
            _logger = logger;
            _scopeFactory = scopeFactory;
        }

        public override async Task DoWork(CancellationToken cancellationToken)
        {
            using var scope = _scopeFactory.CreateScope();
            var commnityClient = scope.ServiceProvider.GetService<SteamCommunityWebClient>();
            var steamService = scope.ServiceProvider.GetRequiredService<SteamService>();
            var db = scope.ServiceProvider.GetRequiredService<SteamDbContext>();

            var cutoff = DateTimeOffset.Now.Subtract(TimeSpan.FromHours(1));
            var items = db.SteamMarketItems
                .Where(x => !String.IsNullOrEmpty(x.SteamId))
                .Where(x => x.LastCheckedOrdersOn <= cutoff)
                .OrderBy(x => x.LastCheckedOrdersOn)
                .Take(100) // batch 100 at a time
                .ToList();

            if (!items.Any())
            {
                return;
            }

            var language = db.SteamLanguages.FirstOrDefault(x => x.IsDefault);
            if (language == null)
            {
                return;
            }

            var currency = db.SteamCurrencies.FirstOrDefault(x => x.IsDefault);
            if (currency == null)
            {
                return;
            }

            var id = Guid.NewGuid();
            _logger.LogInformation($"Updating market item orders information (id: {id}, count: {items.Count()})");
            foreach (var item in items)
            {
                var response = await commnityClient.GetMarketItemOrdersHistogram(
                    new SteamMarketItemOrdersHistogramJsonRequest()
                    {
                        ItemNameId = item.SteamId,
                        Language = language.SteamId,
                        CurrencyId = currency.SteamId,
                        NoRender = true
                    }
                );

                try
                {
                    await steamService.UpdateSteamMarketItemOrders(
                        item,
                        currency.Id,
                        response
                    );
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Failed to update market item order history for '{item.SteamId}'");
                    continue;
                }
            }

            db.SaveChanges();
            _logger.LogInformation($"Updated market item orders information (id: {id})");
        }
    }
}
