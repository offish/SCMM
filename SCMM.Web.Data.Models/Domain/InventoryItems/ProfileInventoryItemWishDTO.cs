﻿using SCMM.Steam.Data.Models.Enums;
using SCMM.Web.Data.Models.Domain.Currencies;
using SCMM.Web.Data.Models.UI;
using System.Text.Json.Serialization;

namespace SCMM.Web.Data.Models.Domain.InventoryItems
{
    public class ProfileInventoryItemWishDTO : ISearchable
    {
        public string SteamId { get; set; }

        public string SteamAppId { get; set; }

        public string Name { get; set; }

        public string BackgroundColour { get; set; }

        public string ForegroundColour { get; set; }

        public string IconUrl { get; set; }

        public CurrencyDTO Currency { get; set; }

        public int Supply { get; set; }

        public int Demand { get; set; }

        public long? BuyAskingPrice { get; set; }

        public long? BuyNowPrice { get; set; }

        public long? Last24hrSales { get; set; }

        public SteamProfileMarketItemFlags Flags { get; set; }

        [JsonIgnore]
        public object[] SearchData => new object[] { SteamId, Name };
    }
}
