﻿using SCMM.Steam.Data.Models.Enums;

namespace SCMM.Web.Data.Models.UI.Profile
{
    public class ProfileDetailedDTO
    {
        public Guid Guid { get; set; }

        public string SteamId { get; set; }

        public string ProfileId { get; set; }

        public string Name { get; set; }

        public string AvatarUrl { get; set; }

        public SteamVisibilityType Privacy { get; set; }

        public DateTimeOffset? LastUpdatedInventoryOn { get; set; }

        public string[] Roles { get; set; }
    }
}
