using System.Collections.Generic;
using System.Reflection;

namespace SoG.Modding
{
    public static class ModElements
    {
        /// <summary> 
        /// Creates a new item using the provided item and equipment builder. <para/>
        /// The uniqueID should not be identical to other IDs inside this mod. <para/>
        /// The equipment builder can be null, in which case the item will act as a non-equippable item.
        /// </summary>
        /// <returns> The new item's ItemTypes ID. If creation failed, ItemCodex.ItemTypes.Null is returned. </returns>

        public static ItemCodex.ItemTypes CreateItem(BaseScript owner, string uniqueID, ItemConfig itemBuilder, EquipConfig equipBuilder = null)
        {
            if (itemBuilder == null || owner == null)
            {
                GrindScript.Logger.Warn("Can't create item because owner or itemBuilder is null.");
                return ItemCodex.ItemTypes.Null;
            }

            ItemCodex.ItemTypes allocatedType = IDAllocator.AllocateItemType();

            // ModItem entry and ItemDescription need to be created before the respective EquipmentInfo
            ModItemEntry newEntry = ModLibrary.Global.ModItems[allocatedType] = new ModItemEntry()
            {
                uniqueID = uniqueID,
                owner = owner,
                type = allocatedType,
                itemInfo = itemBuilder.Build(allocatedType),
            };
            newEntry.equipInfo = equipBuilder?.Build(allocatedType);

            // Also add to the Mod's list, for Load / Save
            owner.ModLib.ModItems[allocatedType] = newEntry;

            foreach (var existing in owner.ModLib.ModItems.Values)
            {
                if (existing != newEntry && existing.uniqueID == uniqueID)
                {
                    GrindScript.Logger.Error($"Mod {owner.GetType().Name} has two or more items with the uniqueID {uniqueID}!");
                    GrindScript.Logger.Error($"This is likely to break loading, saving, and many other things!");
                    break;
                }
            }

            ItemDescription itemInfo = newEntry.itemInfo.vanilla;

            ItemCodex.ItemCategories[] toSanitize = new ItemCodex.ItemCategories[]
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
                ItemCodex.ItemCategories type = (ItemCodex.ItemCategories)newEntry.equipInfo.equipType;
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

            itemInfo.sNameLibraryHandle = uniqueID + "_Name";
            itemInfo.sDescriptionLibraryHandle = uniqueID + "_sDescription";

            TextHelper.AddMiscText("Items", itemInfo.sNameLibraryHandle, itemInfo.sFullName, MiscTextTypes.GenericItemName);
            TextHelper.AddMiscText("Items", itemInfo.sDescriptionLibraryHandle, itemInfo.sDescription, MiscTextTypes.GenericItemDescription);

            return allocatedType;
        }

        /// <summary>
        /// Adds a new crafting recipe.
        /// </summary>

        public static void AddRecipe(ItemCodex.ItemTypes result, Dictionary<ItemCodex.ItemTypes, ushort> ingredients)
        {
            if (ingredients == null)
            {
                GrindScript.Logger.Warn("Can't add recipe because ingredient dictionary is null!");
                return;
            }

            if (!Crafting.CraftSystem.RecipeCollection.ContainsKey(result))
            {
                var kvps = new KeyValuePair<ItemDescription, ushort>[ingredients.Count];

                int index = 0;
                foreach (var kvp in ingredients)
                    kvps[index++] = new KeyValuePair<ItemDescription, ushort>(ItemCodex.GetItemDescription(kvp.Key), kvp.Value);

                ItemDescription description = ItemCodex.GetItemDescription(result);
                Crafting.CraftSystem.RecipeCollection.Add(result, new Crafting.CraftSystem.CraftingRecipe(description, kvps));
            }

            GrindScript.Logger.Info($"Added recipe for item {result}!");
        }

        /// <summary>
        /// Configures custom audio for the mod, using the config provided. <para/>
        /// After configuring the audio, music and effect IDs can be obtained with <see cref="GetEffectID"/> and <see cref="GetMusicID"/>.
        /// </summary>

        public static void ConfigureModAudio(BaseScript owner, AudioConfig audio)
        {
            if (audio == null || owner == null)
            {
                GrindScript.Logger.Warn("Can't create audio due to owner or builder being null!");
                return;
            }
            audio.UpdateExistingEntry(owner.ModPath, owner._audioID);
        }

        /// <summary>
        /// Instructs the SoundSystem to play the target modded music instead of the vanilla music. <para/>
        /// If redirect is the empty string, any existing redirects and cleared.
        /// </summary>

        public static void RedirectVanillaMusic(string vanilla, string redirect)
        {
            var songRegionMapField = (Dictionary<string, string>)typeof(SoundSystem).GetTypeInfo().GetField("dssSongRegionMap", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(GrindScript.Game.xSoundSystem);
            if (!songRegionMapField.ContainsKey(vanilla))
            {
                GrindScript.Logger.Warn($"Redirecting {vanilla} to {redirect} is not possible since {vanilla} is not a vanilla music!");
                return;
            }

            bool isModded = Utils.SplitGSAudioID(redirect, out int entryID, out bool isMusic, out int cueID);
            var entry = ModLibrary.Global.ModAudio.ContainsKey(entryID) ? ModLibrary.Global.ModAudio[entryID] : null;
            string cueName = entry != null && entry.musicIDToName.ContainsKey(cueID) ? entry.musicIDToName[cueID] : null;

            if ((!isModded || !isMusic || cueName == null) && !(redirect == ""))
            {
                GrindScript.Logger.Warn($"Redirecting {vanilla} to {redirect} is not possible since {redirect} is not a modded music!");
                return;
            }

            var redirectedSongs = ModLibrary.Global.VanillaMusicRedirects;
            bool replacing = redirectedSongs.ContainsKey(vanilla);

            if (redirect == "")
            {
                GrindScript.Logger.Info($"Song {vanilla} has been cleared of any redirects.");
                redirectedSongs.Remove(vanilla);
            }
            else
            {
                GrindScript.Logger.Info($"Song {vanilla} is now redirected to {redirect} ({cueName}). {(replacing ? $"Previous redirect was {redirectedSongs[vanilla]}" : "")}");
                redirectedSongs[vanilla] = redirect;
            }
        }

        /// <summary>
        /// Gets the ID of the effect that has the given cue name. <para/>
        /// This ID can be used to play effects with methods such as <see cref="SoundSystem.PlayCue"/>.
        /// </summary>

        public static string GetEffectID(BaseScript owner, string cueName)
        {
            if (owner == null)
            {
                GrindScript.Logger.Warn("Can't get sound ID due to owner being null!");
                return "";
            }
            return GetEffectID(owner._audioID, cueName);
        }

        /// <summary>
        /// Gets the ID of the effect that has the given cue name. <para/>
        /// This ID can be used to play effects with methods such as <see cref="SoundSystem.PlayCue"/>.
        /// </summary>

        public static string GetEffectID(int audioEntryID, string cueName)
        {
            var effects = ModLibrary.Global.ModAudio[audioEntryID].effectIDToName;
            foreach (var kvp in effects)
            {
                if (kvp.Value == cueName)
                {
                    return $"GS_{audioEntryID}_S{kvp.Key}";
                }
            }
            return "";
        }

        /// <summary>
        /// Gets the ID of the music that has the given cue name. <para/>
        /// This ID can be used to play effects with <see cref="SoundSystem.PlaySong"/>.
        /// </summary>

        public static string GetMusicID(BaseScript owner, string cueName)
        {
            if (owner == null)
            {
                GrindScript.Logger.Warn("Can't get sound ID due to owner being null!");
                return "";
            }
            return GetMusicID(owner._audioID, cueName);
        }

        /// <summary>
        /// Gets the ID of the music that has the given cue name. <para/>
        /// This ID can be used to play effects with <see cref="SoundSystem.PlaySong"/>.
        /// </summary>

        public static string GetMusicID(int audioEntryID, string cueName)
        {
            var music = ModLibrary.Global.ModAudio[audioEntryID].musicIDToName;
            foreach (var kvp in music)
            {
                if (kvp.Value == cueName)
                    return $"GS_{audioEntryID}_M{kvp.Key}";
            }
            return "";
        }

        /// <summary>
        /// Gets the cue name based on the modded ID. <para/>
        /// </summary>

        public static string GetCueName(string GSID)
        {
            if (!Utils.SplitGSAudioID(GSID, out int entryID, out bool isMusic, out int cueID))
                return "";
            ModAudioEntry entry = ModLibrary.Global.ModAudio[entryID];
            return isMusic ? entry.musicIDToName[cueID] : entry.effectIDToName[cueID];
        }

        /// <summary>
        /// Adds a new command that can be executed by typing in chat "/(ModName):(command) (argList)" <para/>
        /// The command must not have whitespace in it.
        /// </summary>

        public static void ConfigureCommand(BaseScript owner, string command, CommandParser parser)
        {
            if (owner == null || command == null)
            {
                GrindScript.Logger.Warn("Can't configure command due to owner or command being null!");
                return;
            }

            string name = owner.GetType().Name;

            if (!ModLibrary.Global.ModCommands.TryGetValue(name, out var parsers))
            {
                GrindScript.Logger.Error($"Couldn't retrieve command table for mod {name}!");
                return;
            }

            if (parser == null)
            {
                parsers.Remove(command);
                GrindScript.Logger.Info($"Cleared command /{name}:{command}.");
            }
            else
            {
                parsers[command] = parser;
                GrindScript.Logger.Info($"Updated command /{name}:{command}.");
            }
        }

        /// <summary>
        /// Helper method for setting up multiple commands.
        /// </summary>

        public static void ConfigureCommandsFrom(BaseScript owner, Dictionary<string, CommandParser> parsers)
        {
            if (owner == null || parsers == null)
            {
                GrindScript.Logger.Warn("Can't configure commands due to owner or parsers being null!");
                return;
            }

            foreach (var kvp in parsers)
            {
                ConfigureCommand(owner, kvp.Key, kvp.Value);
            }
        }

        /// <summary>
        /// TODO
        /// </summary>

        public static Level.ZoneEnum CreateLevel()
        {
            return Level.ZoneEnum.None;
        }

        public static Level.WorldRegion CreateWorldRegion()
        {
            return Level.WorldRegion.NotLoaded;
        }
    }
}
