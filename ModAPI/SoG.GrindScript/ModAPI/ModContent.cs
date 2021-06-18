using System;
using System.Collections.Generic;
using System.Reflection;

namespace SoG.Modding
{
    public static class ModContent
    {
        /// <summary> 
        /// Creates a new item using the provided builder. Shorthand for CreateItem(owner, item, null). 
        /// </summary>

        public static ItemCodex.ItemTypes CreateItem(BaseScript owner, ModItemBuilder item)
        {
            return CreateItem(owner, item, null);
        }

        /// <summary> 
        /// <para> Creates a new item using the provided builders. </para>
        /// <para> A null equipment builder can be provided if the new item isn't an equipment. </para>
        /// </summary>
        /// <returns>The new item's ID, or ItemCodex.ItemTypes.Null if creation failed.</returns>

        public static ItemCodex.ItemTypes CreateItem(BaseScript owner, ModItemBuilder item, ModEquipBuilder equip)
        {
            if (item == null || owner == null)
            {
                GrindScript.Logger.Warn("Can't create item due to owner or item being null.");
                return ItemCodex.ItemTypes.Null;
            }

            ItemCodex.ItemTypes allocatedType = ModAllocator.AllocateItemType();

            // ModItem entry and ItemDescription need to be created before the respective EquipmentInfo
            ModItemEntry newEntry = ModLibrary.Global.ModItems[allocatedType] = new ModItemEntry()
            {
                owner = owner,
                type = allocatedType,
                itemInfo = item.Build(allocatedType),
            };
            newEntry.equipInfo = equip?.Build(allocatedType);

            ItemDescription itemInfo = newEntry.itemInfo.vanilla;

            var toSanitize = new List<ItemCodex.ItemCategories>
            {
                ItemCodex.ItemCategories.OneHandedWeapon,
                ItemCodex.ItemCategories.TwoHandedWeapon,
                ItemCodex.ItemCategories.Weapon,
                ItemCodex.ItemCategories.Shield,
                ItemCodex.ItemCategories.Armor,
                ItemCodex.ItemCategories.Hat,
                ItemCodex.ItemCategories.Accessory,
                ItemCodex.ItemCategories.Shoes,
                ItemCodex.ItemCategories.Facegear
            };

            foreach (var toRemove in toSanitize)
                itemInfo.lenCategory.Remove(toRemove);

            if (newEntry.equipInfo != null)
            {
                var type = (ItemCodex.ItemCategories)newEntry.equipInfo.equipType;
                itemInfo.lenCategory.Add(type);

                if (type == ItemCodex.ItemCategories.Weapon)
                {
                    switch ((newEntry.equipInfo.vanilla as WeaponInfo).enWeaponCategory)
                    {
                        case WeaponInfo.WeaponCategory.OneHanded:
                            itemInfo.lenCategory.Add(ItemCodex.ItemCategories.OneHandedWeapon); break;
                        case WeaponInfo.WeaponCategory.TwoHanded:
                            itemInfo.lenCategory.Add(ItemCodex.ItemCategories.TwoHandedWeapon); break;
                    }
                }
            }

            Ui.AddMiscText("Items", itemInfo.sNameLibraryHandle, itemInfo.sFullName, MiscTextTypes.GenericItemName);
            Ui.AddMiscText("Items", itemInfo.sDescriptionLibraryHandle, itemInfo.sDescription, MiscTextTypes.GenericItemDescription);

            return allocatedType;
        }

        public static void DefineModAudio(BaseScript owner, ModAudioBuilder audio)
        {
            if (audio == null || owner == null)
            {
                GrindScript.Logger.Warn("Can't create audio due to owner or builder being null!");
                return;
            }
            audio.UpdateExistingEntry(owner.CustomAssets.RootDirectory, owner._AudioID);
        }

        public static void DefineSongRedirect(string vanilla, string redirect)
        {
            var songRegionMapField = (Dictionary<string, string>)typeof(SoundSystem).GetTypeInfo().GetField("dssSongRegionMap", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(GrindScript.Game.xSoundSystem);
            if (!songRegionMapField.ContainsKey(vanilla))
            {
                GrindScript.Logger.Warn($"Redirecting {vanilla} to {redirect} is not possible since {vanilla} is not a vanilla music!");
                return;
            }

            bool isModded = AudioUtils.SplitGSAudioID(redirect, out int entryID, out bool isMusic, out int cueID);
            var entry = ModLibrary.Global.ModAudio.ContainsKey(entryID) ? ModLibrary.Global.ModAudio[entryID] : null;
            string cueName = entry != null && entry.musicIDToMusic.ContainsKey(cueID) ? entry.musicIDToMusic[cueID] : null;

            if ((!isModded || !isMusic || cueName == null) && !(redirect == ""))
            {
                GrindScript.Logger.Warn($"Redirecting {vanilla} to {redirect} is not possible since {redirect} is not a modded music!");
                return;
            }

            var redirectedSongs = ModLibrary.Global.VanillaRedirectedSongs;

            if (redirectedSongs.ContainsKey(vanilla))
            {
                GrindScript.Logger.Info($"Song {vanilla} has a previous redirect {redirectedSongs[vanilla]}, which will be replaced!");
            }

            if (redirect == "")
            {
                redirectedSongs.Remove(vanilla);
                GrindScript.Logger.Info($"Song {vanilla} has been cleared of any redirects.");
            }
            else
            {
                redirectedSongs[vanilla] = redirect;
                GrindScript.Logger.Info($"Song {vanilla} is now redirected to {redirect}, which is cue {cueName}.");
            }
        }

        //
        // Getters and crappers
        //

        public static string GetSoundID(BaseScript owner, string cueName)
        {
            if (owner == null)
            {
                GrindScript.Logger.Warn("Can't get sound ID due to owner being null!");
                return "";
            }
            return GetSoundID(owner._AudioID, cueName);
        }

        public static string GetSoundID(int audioID, string cueName)
        {
            var effects = ModLibrary.Global.ModAudio[audioID].effectIDToEffect;
            foreach (var kvp in effects)
            {
                if (kvp.Value == cueName)
                {
                    return $"GS_{audioID}_S{kvp.Key}";
                }
            }
            return "";
        }

        public static string GetMusicID(BaseScript owner, string cueName)
        {
            if (owner == null)
            {
                GrindScript.Logger.Warn("Can't get sound ID due to owner being null!");
                return "";
            }
            return GetMusicID(owner._AudioID, cueName);
        }

        public static string GetMusicID(int audioID, string cueName)
        {
            var music = ModLibrary.Global.ModAudio[audioID].musicIDToMusic;
            foreach (var kvp in music)
            {
                if (kvp.Value == cueName)
                {
                    return $"GS_{audioID}_M{kvp.Key}";
                }
            }
            return "";
        }

        public static string GetCueName(string GSID)
        {
            string cueName = "";
            if (AudioUtils.SplitGSAudioID(GSID, out int entryID, out bool isMusic, out int cueID))
            {
                if (isMusic)
                    cueName = ModLibrary.Global.ModAudio[entryID].musicIDToMusic[cueID];
                else
                    cueName = ModLibrary.Global.ModAudio[entryID].effectIDToEffect[cueID];
            }
            GrindScript.Logger.Debug("GetCueName: " + cueName);
            return cueName;
        }
    }
}
