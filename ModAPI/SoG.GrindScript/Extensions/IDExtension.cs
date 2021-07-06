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

        public static bool IsFromMod(this ItemCodex.ItemTypes id) => id >= APIGlobals.API.Allocator.ItemID.Start && id < APIGlobals.API.Allocator.ItemID.End;

        public static bool IsFromMod(this Level.WorldRegion id) => APIGlobals.API.Allocator.WorldRegionID.Start <= id && id < APIGlobals.API.Allocator.WorldRegionID.End;

        public static bool IsFromMod(this Level.ZoneEnum id) => APIGlobals.API.Allocator.LevelID.Start <= id && id < APIGlobals.API.Allocator.LevelID.End;

        public static bool IsFromMod(this RogueLikeMode.TreatsCurses id) => id >= APIGlobals.API.Allocator.TreatCurseID.Start && id < APIGlobals.API.Allocator.TreatCurseID.End;

        public static bool IsFromMod(this RogueLikeMode.Perks id) => id >= APIGlobals.API.Allocator.PerkID.Start && id < APIGlobals.API.Allocator.PerkID.End;
    }
}
