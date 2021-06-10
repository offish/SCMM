﻿using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SCMM.Google.Client;
using SCMM.Shared.Data.Store.Types;
using SCMM.Steam.Data.Store;
using SCMM.Steam.Job.Server.Jobs.Cron;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SCMM.Steam.Job.Server.Jobs
{
    public class CheckYouTubeForNewStoreVideosJobConfiguration : CronJobConfiguration
    {
        public ChannelExpression[] Channels { get; set; }

        public class ChannelExpression
        {
            public string ChannelId { get; set; }

            public string Query { get; set; }
        }
    }

    public class CheckYouTubeForNewStoreVideosJobs : CronJobService<CheckYouTubeForNewStoreVideosJobConfiguration>
    {
        private readonly ILogger<CheckYouTubeForNewStoreVideosJobs> _logger;
        private readonly IServiceScopeFactory _scopeFactory;

        public CheckYouTubeForNewStoreVideosJobs(IConfiguration configuration, ILogger<CheckYouTubeForNewStoreVideosJobs> logger, IServiceScopeFactory scopeFactory)
            : base(logger, configuration.GetJobConfiguration<CheckYouTubeForNewStoreVideosJobs, CheckYouTubeForNewStoreVideosJobConfiguration>())
        {
            _logger = logger;
            _scopeFactory = scopeFactory;
        }

        public override async Task DoWork(CancellationToken cancellationToken)
        {
            using var scope = _scopeFactory.CreateScope();
            var googleClient = scope.ServiceProvider.GetService<GoogleClient>();
            var db = scope.ServiceProvider.GetRequiredService<SteamDbContext>();

            var steamApps = await db.SteamApps.ToListAsync();
            if (!steamApps.Any())
            {
                return;
            }

            foreach (var app in steamApps)
            {
                var media = new Dictionary<DateTime, string>();
                var itemStore = db.SteamItemStores
                    .Where(x => x.End == null)
                    .OrderByDescending(x => x.Start)
                    .FirstOrDefault();

                if (itemStore == null)
                {
                    continue;
                }

                foreach (var channel in Configuration.Channels)
                {
                    var videos = await googleClient.SearchVideosAsync(
                        query: channel.Query,
                        channelId: channel.ChannelId,
                        publishedBefore: itemStore.End?.UtcDateTime,
                        publishedAfter: itemStore.Start.UtcDateTime
                    );
                    if (videos?.Any() == true)
                    {
                        foreach (var video in videos.Where(x => x.PublishedAt != null))
                        {
                            if (video.Title.Contains(channel.Query.Trim('\"'), StringComparison.InvariantCultureIgnoreCase))
                            {
                                media[video.PublishedAt.Value] = video.Id;
                                /*
                                googleClient.CommentVideoAsync(
                                    video.ChannelId,
                                    video.Id,
                                    $"thank you for showcasing this weeks Rust skins, your video has been featured on https://scmm.app/store/{itemStore.Start.ToString(Constants.SCMMStoreIdDateFormat)}"
                                );
                                */
                            }
                        }
                    }
                }

                var newMedia = media
                    .Where(x => !itemStore.Media.Contains(x.Value))
                    .OrderBy(x => x.Key)
                    .ToList();

                if (newMedia.Any())
                {
                    itemStore.Media = new PersistableStringCollection(
                        itemStore.Media.Union(newMedia.Select(x => x.Value))
                    );
                    db.SaveChanges();
                }
            }
        }
    }
}
