﻿using System;

namespace SCMM.Web.Shared.Data.Models.Steam
{
    [Flags]
    public enum SteamAssetDescriptionFlags : byte
    {
        None = 0x00,
        Banned = 0x01,
        Tradable = 0x02,
        Marketable = 0x04,
        Commodity = 0x10
    }
}
