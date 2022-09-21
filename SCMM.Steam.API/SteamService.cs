﻿using CommandQuery;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using SCMM.Shared.Data.Models.Extensions;
using SCMM.Steam.API.Commands;
using SCMM.Steam.API.Extensions;
using SCMM.Steam.Client;
using SCMM.Steam.Client.Extensions;
using SCMM.Steam.Data.Models.Community.Responses.Json;
using SCMM.Steam.Data.Models.Extensions;
using SCMM.Steam.Data.Store;
using SCMM.Steam.Data.Store.Types;
using Steam.Models.SteamEconomy;
using System.Globalization;
using System.Reflection;

namespace SCMM.Steam.API
{
    public class SteamService
    {
        private readonly TimeSpan DefaultCachePeriod = TimeSpan.FromHours(6);

        private readonly SteamDbContext _db;
        private readonly SteamConfiguration _cfg;
        private readonly SteamCommunityWebClient _communityClient;
        private readonly ICommandProcessor _commandProcessor;
        private readonly IQueryProcessor _queryProcessor;


        public SteamService(SteamDbContext db, IConfiguration cfg, SteamCommunityWebClient communityClient, ICommandProcessor commandProcessor, IQueryProcessor queryProcessor)
        {
            _db = db;
            _cfg = cfg?.GetSteamConfiguration();
            _communityClient = communityClient;
            _commandProcessor = commandProcessor;
            _queryProcessor = queryProcessor;
        }

        public async Task<SteamStoreItem> AddOrUpdateStoreItemAndMarkAsAvailable(SteamApp app, AssetModel asset, SteamCurrency currency, DateTimeOffset? timeChecked)
        {
            // Find the item by it's store id or asset class id (which ever exists first)
            var dbItem = (
                await _db.SteamStoreItems
                    .Include(x => x.Stores).ThenInclude(x => x.Store)
                    .Include(x => x.Description)
                    .Include(x => x.Description.App)
                    .Include(x => x.Description.CreatorProfile)
                    .Where(x => x.AppId == app.Id)
                    .FirstOrDefaultAsync(x => x.SteamId == asset.Name) ??
                await _db.SteamStoreItems
                    .Include(x => x.Stores).ThenInclude(x => x.Store)
                    .Include(x => x.Description)
                    .Include(x => x.Description.App)
                    .Include(x => x.Description.CreatorProfile)
                    .Where(x => x.AppId == app.Id)
                    .FirstOrDefaultAsync(x => x.Description.ClassId == asset.ClassId)
            );

            // Find the item asset description, or import it if missing
            var assetDescription = (dbItem?.Description ??
                await _db.SteamAssetDescriptions
                    .Include(x => x.App)
                    .Include(x => x.CreatorProfile)
                    .FirstOrDefaultAsync(x => x.AppId == app.Id && x.ClassId == asset.ClassId)
            );
            if (assetDescription == null)
            {
                var importAssetDescription = await _commandProcessor.ProcessWithResultAsync(new ImportSteamAssetDescriptionRequest()
                {
                    AppId = ulong.Parse(app.SteamId),
                    AssetClassId = asset.ClassId
                });
                assetDescription = importAssetDescription.AssetDescription;
                if (assetDescription == null)
                {
                    // The asset description for this item doesn't exist, bail...
                    return null;
                }
            }

            // If the store item doesn't exist yet, create it now
            if (dbItem == null)
            {
                app.StoreItems.Add(dbItem = new SteamStoreItem()
                {
                    App = app,
                    AppId = app.Id,
                    Description = assetDescription,
                    DescriptionId = assetDescription.Id
                });

                var prices = asset.Prices.ToDictionary();
                dbItem.UpdatePrice(
                    currency,
                    prices.FirstOrDefault(x => x.Key == currency?.Name).Value,
                    new PersistablePriceDictionary(prices)
                );
            }

            // If the asset item is not yet accepted, accept it now
            assetDescription.IsAccepted = true;
            if (assetDescription.TimeAccepted == null)
            {
                if (!String.IsNullOrEmpty(asset.Date))
                {
                    DateTimeOffset storeDate;
                    if (DateTimeOffset.TryParseExact(asset.Date, "yyyy-M-d", CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out storeDate) ||
                        DateTimeOffset.TryParseExact(asset.Date, "yyyy/M/d", CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out storeDate))
                    {
                        assetDescription.TimeAccepted = storeDate;
                    }
                }
                else
                {
                    assetDescription.TimeAccepted = timeChecked;
                }
            }

            // Mark the store item as available
            dbItem.SteamId = asset.Name;
            dbItem.IsAvailable = true;
            return dbItem;
        }

        public async Task<SteamMarketItem> AddOrUpdateMarketItem(SteamApp app, SteamCurrency currency, SteamMarketPriceOverviewJsonResponse marketPriceOverview, SteamAssetDescription asset)
        {
            var dbItem = await _db.SteamMarketItems
                .Include(x => x.App)
                .Include(x => x.Currency)
                .Include(x => x.Description)
                .Where(x => x.AppId == app.Id)
                .FirstOrDefaultAsync(x => x.Description.ClassId == asset.ClassId);

            if (dbItem != null)
            {
                return dbItem;
            }

            if (asset.ClassId == null)
            {
                return null;
            }

            var importAssetDescription = await _commandProcessor.ProcessWithResultAsync(new ImportSteamAssetDescriptionRequest()
            {
                AppId = ulong.Parse(app.SteamId),
                AssetClassId = asset.ClassId.Value
            });
            var assetDescription = importAssetDescription.AssetDescription;
            if (assetDescription == null || assetDescription.NameId == null)
            {
                return null;
            }

            app.MarketItems.Add(dbItem = new SteamMarketItem()
            {
                SteamId = assetDescription.NameId?.ToString(),
                AppId = app.Id,
                Description = assetDescription,
                Currency = currency,
                SellOrderCount = marketPriceOverview.Volume.SteamQuantityValueAsInt(),
                SellOrderLowestPrice = marketPriceOverview.LowestPrice.SteamPriceAsInt()
            });

            return dbItem;
        }

        public async Task<SteamMarketItem> UpdateMarketItemOrders(SteamMarketItem item, SteamMarketItemOrdersHistogramJsonResponse histogram)
        {
            if (item == null || histogram?.Success != true)
            {
                return item;
            }

            // Lazy-load buy/sell order history if missing, required for recalculation
            if (item.BuyOrders?.Any() != true || item.SellOrders?.Any() != true)// || item.OrdersHistory?.Any() != true)
            {
                item = await _db.SteamMarketItems
                    .Include(x => x.BuyOrders)
                    .Include(x => x.SellOrders)
                    //.Include(x => x.OrdersHistory)
                    .SingleOrDefaultAsync(x => x.Id == item.Id);
            }

            item.LastCheckedOrdersOn = DateTimeOffset.Now;
            item.RecalculateOrders(
                ParseMarketItemOrdersFromGraph<SteamMarketItemBuyOrder>(histogram.BuyOrderGraph),
                histogram.BuyOrderCount.SteamQuantityValueAsInt(),
                ParseMarketItemOrdersFromGraph<SteamMarketItemSellOrder>(histogram.SellOrderGraph),
                histogram.SellOrderCount.SteamQuantityValueAsInt()
            );

            return item;
        }

        public async Task<SteamMarketItem> UpdateMarketItemSalesHistory(SteamMarketItem item, SteamMarketPriceHistoryJsonResponse sales, SteamCurrency salesCurrency = null)
        {
            if (item == null || sales?.Success != true)
            {
                return item;
            }

            // Lazy-load sales history if missing, required for recalculation
            if (item.SalesHistory?.Any() != true)
            {
                item = await _db.SteamMarketItems
                    .Include(x => x.SalesHistory)
                    .SingleOrDefaultAsync(x => x.Id == item.Id);
            }

            // If the sales are not already in our items currency, exchange them now
            var itemSales = ParseMarketItemSalesFromGraph(sales.Prices, item.LastCheckedSalesOn);
            if (itemSales != null && salesCurrency != null && salesCurrency.Id != item.CurrencyId)
            {
                foreach (var sale in itemSales)
                {
                    sale.MedianPrice = item.Currency.CalculateExchange(sale.MedianPrice, salesCurrency);
                }
            }

            item.LastCheckedSalesOn = DateTimeOffset.Now;
            item.RecalculateSales(itemSales);

            return item;
        }

        private T[] ParseMarketItemOrdersFromGraph<T>(string[][] orderGraph)
            where T : Steam.Data.Store.SteamMarketItemOrder, new()
        {
            var orders = new List<T>();
            if (orderGraph == null)
            {
                return orders.ToArray();
            }

            var totalQuantity = 0;
            for (var i = 0; i < orderGraph.Length; i++)
            {
                var price = orderGraph[i][0].SteamPriceAsInt();
                var quantity = (orderGraph[i][1].SteamQuantityValueAsInt() - totalQuantity);
                orders.Add(new T()
                {
                    Price = price,
                    Quantity = quantity,
                });
                totalQuantity += quantity;
            }

            return orders.ToArray();
        }

        private SteamMarketItemSale[] ParseMarketItemSalesFromGraph(string[][] salesGraph, DateTimeOffset? ignoreSalesBefore = null)
        {
            var sales = new List<SteamMarketItemSale>();
            if (salesGraph == null)
            {
                return sales.ToArray();
            }

            var totalQuantity = 0;
            for (var i = 0; i < salesGraph.Length; i++)
            {
                var timeStamp = DateTime.ParseExact(salesGraph[i][0], "MMM dd yyyy HH: z", CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal);
                var medianPrice = salesGraph[i][1].SteamPriceAsInt();
                var quantity = salesGraph[i][2].SteamQuantityValueAsInt();
                sales.Add(new SteamMarketItemSale()
                {
                    Timestamp = timeStamp,
                    MedianPrice = medianPrice,
                    Quantity = quantity,
                });
                totalQuantity += quantity;
            }

            return sales.ToArray();
        }
    }
}
