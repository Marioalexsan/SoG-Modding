using Microsoft.Xna.Framework;
using System;
using SoG.Modding.Core;

namespace SoG.Modding.Extensions
{
    /// <summary>
    /// Following extensions check if a certain game ID is currently allocated to a mod
    /// </summary>

    public static class IDExtension
    {
        public static bool IsFromSoG<T>(this T id) where T : Enum => Enum.IsDefined(typeof(T), id);

        public static bool IsFromMod(this ItemCodex.ItemTypes id) => id >= ModGlobals.API.Allocator.ItemID.Start && id < ModGlobals.API.Allocator.ItemID.End;

        public static bool IsFromMod(this Level.WorldRegion id) => ModGlobals.API.Allocator.WorldRegionID.Start <= id && id < ModGlobals.API.Allocator.WorldRegionID.End;

        public static bool IsFromMod(this Level.ZoneEnum id) => ModGlobals.API.Allocator.LevelID.Start <= id && id < ModGlobals.API.Allocator.LevelID.End;

        public static bool IsFromMod(this RogueLikeMode.TreatsCurses id) => id >= ModGlobals.API.Allocator.TreatCurseID.Start && id < ModGlobals.API.Allocator.TreatCurseID.End;

        public static bool IsFromMod(this RogueLikeMode.Perks id) => id >= ModGlobals.API.Allocator.PerkID.Start && id < ModGlobals.API.Allocator.PerkID.End;
    }
}
