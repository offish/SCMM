﻿using SCMM.Web.Data.Models.UI;
using System;

namespace SCMM.Web.Data.Models.Domain.InventoryItems
{
    public class ProfileSummaryDTO
    {
        public string SteamId { get; set; }

        public string ProfileId { get; set; }

        public string Name { get; set; }

        public string AvatarUrl { get; set; }

        public string AvatarLargeUrl { get; set; }

        public DateTimeOffset? LastViewedInventoryOn { get; set; }

        public DateTimeOffset? LastUpdatedInventoryOn { get; set; }
    }
}
