﻿using CommandQuery;
using Microsoft.Azure.Functions.Worker;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SCMM.Discord.API.Commands;
using SCMM.Shared.API.Extensions;
using SCMM.Steam.API.Commands;
using SCMM.Steam.Client;
using SCMM.Steam.Data.Models.Extensions;
using SCMM.Steam.Data.Models.WebApi.Models;
using SCMM.Steam.Data.Models.WebApi.Requests.IGameInventory;
using SCMM.Steam.Data.Models.WebApi.Requests.IInventoryService;
using SCMM.Steam.Data.Store;
using System.Globalization;
using System.Text;

namespace SCMM.Steam.Functions.Timer;

public class CheckForNewItemDefinitions
{
    private readonly IConfiguration _configuration;
    private readonly SteamDbContext _db;
    private readonly ICommandProcessor _commandProcessor;
    private readonly IQueryProcessor _queryProcessor;
    private readonly SteamWebApiClient _apiClient;

    public CheckForNewItemDefinitions(IConfiguration configuration, ICommandProcessor commandProcessor, IQueryProcessor queryProcessor, SteamDbContext db, SteamWebApiClient apiClient)
    {
        _configuration = configuration;
        _commandProcessor = commandProcessor;
        _queryProcessor = queryProcessor;
        _db = db;
        _apiClient = apiClient;
    }

    [Function("Check-New-Item-Definitions")]
    public async Task Run([TimerTrigger("30 * * * * *")] /* every minute */ TimerInfo timerInfo, FunctionContext context)
    {
        var logger = context.GetLogger("Check-New-Item-Definitions");

        var steamApps = await _db.SteamApps.Where(x => x.IsActive).ToListAsync();
        if (!steamApps.Any())
        {
            return;
        }

        foreach (var app in steamApps)
        {
            logger.LogTrace($"Checking for new item definitions (appId: {app.SteamId})");

            // Get the latest item definition digest
            var itemDefMetadata = await _apiClient.InventoryServiceGetItemDefMeta(new GetItemDefMetaJsonRequest()
            {
                AppId = UInt64.Parse(app.SteamId)
            });

            // Has the digest actually changed?
            var itemDefsDigest = itemDefMetadata?.Digest;
            if (!String.IsNullOrEmpty(itemDefsDigest) && !String.Equals(itemDefsDigest, app.ItemDefinitionsDigest, StringComparison.OrdinalIgnoreCase))
            {
                var itemDefsLastModified = itemDefMetadata?.Modified.SteamTimestampToDateTimeOffset();
                if (itemDefsLastModified != null && (itemDefsLastModified >= app.TimeUpdated || app.TimeUpdated == null))
                {
                    app.ItemDefinitionsDigest = itemDefsDigest;
                    app.TimeUpdated = itemDefsLastModified;

                    await _db.SaveChangesAsync();

                    // Get the new item definition archive
                    var itemDefinitions = await _apiClient.GameInventoryGetItemDefArchive(new GetItemDefArchiveJsonRequest()
                    {
                        AppId = UInt64.Parse(app.SteamId),
                        Digest = app.ItemDefinitionsDigest,
                    });

                    // Import the item definitions
                    var newAssetDescriptions = new List<SteamAssetDescription>();
                    var updatedAssetDescriptions = new List<SteamAssetDescription>();
                    if (itemDefinitions != null)
                    {
                        var assetDescriptions = await _db.SteamAssetDescriptions
                            .Include(x => x.App)
                            .Where(x => x.AppId == app.Id)
                            .ToListAsync();

                        // TODO: Filter this properly
                        var fileredItemDefinitions = itemDefinitions
                            .Where(x => x.Name != "DELETED" && x.Type != "generator");

                        foreach (var itemDefinition in fileredItemDefinitions)
                        {
                            var assetDescription = assetDescriptions.FirstOrDefault(x =>
                                (x.ItemDefinitionId > 0 && itemDefinition.ItemDefId > 0 && x.ItemDefinitionId == itemDefinition.ItemDefId) ||
                                (x.WorkshopFileId > 0 && itemDefinition.WorkshopId > 0 && x.WorkshopFileId == itemDefinition.WorkshopId) ||
                                (!String.IsNullOrEmpty(x.NameHash) && !String.IsNullOrEmpty(itemDefinition.MarketHashName) && x.NameHash == itemDefinition.MarketHashName) ||
                                (!String.IsNullOrEmpty(x.Name) && !String.IsNullOrEmpty(itemDefinition.MarketName) && x.Name == itemDefinition.MarketName) ||
                                (!String.IsNullOrEmpty(x.Name) && !String.IsNullOrEmpty(itemDefinition.Name) && x.Name == itemDefinition.Name)
                            );
                            if (assetDescription == null)
                            {
                                var newAssetDescription = await _commandProcessor.ProcessWithResultAsync(new ImportSteamItemDefinitionRequest()
                                {
                                    AppId = UInt64.Parse(app.SteamId),
                                    ItemDefinitionId = itemDefinition.ItemDefId,
                                    ItemDefinitionName = itemDefinition.Name,
                                    ItemDefinition = itemDefinition
                                });
                                if (newAssetDescription.AssetDescription != null)
                                {
                                    newAssetDescriptions.Add(newAssetDescription.AssetDescription);
                                }
                            }
                            else
                            {
                                if (assetDescription.ItemDefinitionId == null)
                                {
                                    var updatedAssetDescription = await _commandProcessor.ProcessWithResultAsync(new UpdateSteamAssetDescriptionRequest()
                                    {
                                        AssetDescription = assetDescription,
                                        AssetItemDefinition = itemDefinition
                                    });
                                    if (updatedAssetDescription.AssetDescription != null)
                                    {
                                        updatedAssetDescriptions.Add(updatedAssetDescription.AssetDescription);
                                    }
                                }
                            }
                        }
                    }

                    await _db.SaveChangesAsync();

                    if (newAssetDescriptions.Any() || updatedAssetDescriptions.Any())
                    {
                        await BroadcastUpdatedItemDefinitionsNotification(logger, app, newAssetDescriptions, updatedAssetDescriptions);
                    }
                }
            }
        }
    }

    private async Task BroadcastUpdatedItemDefinitionsNotification(ILogger logger, SteamApp app, IEnumerable<SteamAssetDescription> newAssetDescriptions, IEnumerable<SteamAssetDescription> updatedAssetDescriptions)
    {
        var guilds = _db.DiscordGuilds.Include(x => x.Configuration).ToList();
        foreach (var guild in guilds)
        {
            try
            {
                if (!bool.Parse(guild.Get(DiscordConfiguration.AlertsItemDefinition, Boolean.FalseString).Value))
                {
                    continue;
                }

                var guildChannels = guild.List(DiscordConfiguration.AlertChannel).Value?.Union(new[] {
                    "announcement", "market", "skin", app.Name, "general", "chat", "bot"
                });

                var description = new StringBuilder();
                var fields = new Dictionary<string, string>();
                description.AppendLine(app.ItemDefinitionsDigest);
                description.AppendLine();
                if (newAssetDescriptions.Any())
                {
                    description.Append($"{newAssetDescriptions.Count()} new item(s) have just appeared in the {app?.Name} item definitions archive.");
                    foreach (var item in newAssetDescriptions.OrderByDescending(x => x.TimeCreated))
                    {
                        fields.Add(
                            $"🆕 {item.ItemDefinitionId}",
                            (String.IsNullOrEmpty(item.IconUrl) ? item.Name : $"[{item.Name}]({item.IconUrl})")
                        );
                    }
                }
                if (updatedAssetDescriptions.Any())
                {
                    description.Append($"{updatedAssetDescriptions.Count()} existing item(s) have been updated.");
                    foreach (var item in updatedAssetDescriptions.OrderByDescending(x => x.ClassId))
                    {
                        fields.Add(
                            $"{item.ClassId}",
                            $"[{item.Name}]({_configuration.GetWebsiteUrl()}/item/{Uri.EscapeDataString(item.Name)})"
                        );
                    }
                }
                if (!newAssetDescriptions.Any() && !updatedAssetDescriptions.Any())
                {
                    description.Append("No significant item additions/removals were detected.");
                }
                if (fields.Count > 24)
                {
                    fields = fields.Take(24).ToDictionary(x => x.Key, x => x.Value);
                    fields.Add(
                        $"+{newAssetDescriptions.Count() + updatedAssetDescriptions.Count() - 24} items",
                        "View full item list for more details"
                    );
                }

                await _commandProcessor.ProcessAsync(new SendDiscordMessageRequest()
                {
                    GuidId = ulong.Parse(guild.DiscordId),
                    ChannelPatterns = guildChannels?.ToArray(),
                    Message = null,
                    Title = $"{app?.Name} Item Definitions Updated",
                    Description = description.ToString(),
                    Fields = fields,
                    FieldsInline = true,
                    Url = $"{_configuration.GetWebsiteUrl()}/items",
                    ThumbnailUrl = app?.IconUrl,
                    Colour = UInt32.Parse(app.PrimaryColor.Replace("#", ""), NumberStyles.HexNumber)
                });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Failed to send new market item notification to guild (id: {guild.Id})");
                continue;
            }
        }
    }
}
