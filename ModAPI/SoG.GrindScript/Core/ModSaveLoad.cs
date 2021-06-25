using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SoG.Modding
{
    internal static class ModSaveLoad
    {
        private static readonly int GrindScriptVersion = 1;
        public static readonly string ModExt = ".gs";

        // Identifier methods

        /// <summary>
        /// Converts items from one type to another. <para/>
        /// This affects inventory, crafted items, discovered items, etc. <para/>
        /// This does not
        /// </summary>

        private static void ShuffleItem(ItemCodex.ItemTypes from, ItemCodex.ItemTypes to)
        {
            if (from.IsSoGItem())
                GrindScript.Logger.Warn($"Item {from} is a SoG item! Shuffling may cause issues.", source: "ShuffleItem");

            if (to.IsSoGItem())
                GrindScript.Logger.Warn($"Item {to} is a SoG item! Shuffling may cause issues.", source: "ShuffleItem");

            Game1 Game = GrindScript.Game;

            PlayerView player = Game.xLocalPlayer;
            Inventory inventory = player.xInventory;
            Journal journal = player.xJournalInfo;

            // Shuffle inventory
            if (inventory.denxInventory.ContainsKey(from))
            {
                inventory.denxInventory[to] = new Inventory.DisplayItem(inventory.denxInventory[from].iAmount, inventory.denxInventory[from].iPickupNumber, ItemCodex.GetItemDescription(to));
                inventory.denxInventory.Remove(from);
            }

            // Shuffle discovered items
            if (journal.henUniqueDiscoveredItems.Contains(from))
            {
                journal.henUniqueDiscoveredItems.Remove(from);
                journal.henUniqueDiscoveredItems.Add(to);
            }

            // Shuffle crafted items
            if (journal.henUniqueCraftedItems.Contains(from))
            {
                journal.henUniqueCraftedItems.Remove(from);
                journal.henUniqueCraftedItems.Add(to);
            }

            // Shuffle fishes
            if (journal.henUniqueFishies.Contains(from))
            {
                journal.henUniqueFishies.Remove(from);
                journal.henUniqueFishies.Add(to);
            }
        }

        public static void IdentifyItems(BaseScript target, Dictionary<int, string> targetSavedItems, ref ItemCodex.ItemTypes shuffleIndex)
        {
            var savedItems = new Dictionary<int, string>(targetSavedItems);

            // Shuffle items into their correct ItemTypes IDs
            while (savedItems.Count > 0)
            {
                var saveItem = savedItems.First();

                sbyte shuffleStatus = -1;

                foreach (var modItem in target.ModLib.ModItems.Values)
                {
                    if (saveItem.Value == modItem.uniqueID)
                    {
                        if (saveItem.Key != (int)modItem.type)
                        {
                            // If there is a conflict, shuffle it to a temporary ID

                            if (savedItems.ContainsKey((int)modItem.type))
                            {
                                GrindScript.Logger.Debug($"{savedItems[(int)modItem.type]}: {(int)modItem.type} -> {(int)shuffleIndex} (conflict)", source: "IdentifyItems");
                                ShuffleItem(modItem.type, shuffleIndex);

                                savedItems.Add((int)shuffleIndex, savedItems[(int)modItem.type]);
                                savedItems.Remove((int)modItem.type);
                                shuffleIndex++;
                            }

                            // Shuffle save ID to mod ID

                            GrindScript.Logger.Debug($"{saveItem.Value}: {saveItem.Key} -> {(int)modItem.type}", source: "IdentifyItems");
                            ShuffleItem((ItemCodex.ItemTypes)saveItem.Key, modItem.type);

                            shuffleStatus = 1;
                        }
                        else shuffleStatus = 0;
                        break;
                    }
                }

                if (shuffleStatus == -1)
                    GrindScript.Logger.Warn($"{saveItem.Value} couldn't be identified!", source: "IdentifyItems");

                savedItems.Remove(saveItem.Key);
            }
        }

        // Save methods - these need to save additonal info such as mod list used

        public static void SaveGrindScriptInfo(BinaryWriter file, bool items = true)
        {
            file.Write(GrindScriptVersion);

            // Write information for each mod
            file.Write(GrindScript._loadedScripts.Count);
            foreach (var mod in GrindScript._loadedScripts)
            {
                file.Write(mod.GetType().FullName);

                if (items)
                {
                    // Items which were defined by this mod during this session
                    file.Write(mod.ModLib.ModItems.Count);
                    foreach (var item in mod.ModLib.ModItems.Values)
                    {
                        file.Write((int)item.type);
                        file.Write(item.uniqueID);
                    }
                }
            }
        }

        public static void LoadGrindScriptInfo(BinaryReader file, bool items = true)
        {
            var itemShuffleIndex = IDAllocator.ItemTypes_ShuffleStart;

            int GSVersion = file.ReadInt32();

            if (GSVersion != GrindScriptVersion)
            {
                GrindScript.Logger.Warn($"Version mismatch! Using version {GrindScriptVersion} while save has {GSVersion}.");
            }

            // Read information for each mod

            int scriptCount = file.ReadInt32();
            while (scriptCount-- > 0)
            {
                string fullName = file.ReadString();

                BaseScript target = GrindScript._loadedScripts.FirstOrDefault(mod => mod.GetType().FullName == fullName);
                if (target == null)
                {
                    GrindScript.Logger.Warn($"This save used mod {fullName}, which is not loaded right now!");
                    GrindScript.Logger.Warn($"Game objects can not be identified as a result...");
                }

                if (items)
                {
                    Dictionary<int, string> savedItems = new Dictionary<int, string>();

                    for (int saveItems = file.ReadInt32(); saveItems > 0; saveItems--)
                    {
                        savedItems.Add(file.ReadInt32(), file.ReadString());
                    }

                    if (target != null) IdentifyItems(target, savedItems, ref itemShuffleIndex);
                }
            }
        }

        public static void SaveModCharacter(BinaryWriter file)
        {
            SaveGrindScriptInfo(file, items: true);
        }

        public static void LoadModCharacter(BinaryReader file)
        {
            LoadGrindScriptInfo(file, items: true);
        }

        public static void SaveModWorld(BinaryWriter file)
        {
            // TODO
        }

        public static void LoadModWorld(BinaryReader file)
        {
            // TODO
        }

        public static void SaveModArcade(BinaryWriter file)
        {
            SaveGrindScriptInfo(file, items: true);
        }

        public static void LoadModArcade(BinaryReader file)
        {
            LoadGrindScriptInfo(file, items: true);
        }
    }
}
