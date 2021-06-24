using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoG.Modding
{
    internal static class ModSaveLoad
    {
        private static readonly int VanillaVersion = 109;
        private static readonly int GrindScriptVersion = 1;

        // Identifier methods

        private static void ShuffleItem(ItemCodex.ItemTypes from, ItemCodex.ItemTypes to)
        {
            Game1 Game = GrindScript.Game;

            PlayerView player = Game.xLocalPlayer;
            Inventory inv = player.xInventory;
            Equipment equip = player.xEquipment;

            if (inv.denxInventory.ContainsKey(from))
            {
                inv.denxInventory[to] = new Inventory.DisplayItem(inv.denxInventory[from].iAmount, inv.denxInventory[from].iPickupNumber, ItemCodex.GetItemDescription(to));
                inv.denxInventory.Remove(from);
            }
        }

        public static void IdentifyItems(BaseScript target, Dictionary<int, string> targetSavedItems, ref ItemCodex.ItemTypes shuffleIndex)
        {
            var savedItems = new Dictionary<int, string>(targetSavedItems);

            // Shuffle items into their correct ItemTypes IDs
            while (savedItems.Count > 0)
            {
                var saveItem = savedItems.First();

                bool shuffled = false;

                foreach (var modItem in target.ModLib.ModItems.Values)
                {
                    if (saveItem.Value == modItem.uniqueID && saveItem.Key != (int)modItem.type)
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

                        shuffled = true;
                        break;
                    }
                }

                if (!shuffled)
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
            var itemShuffleIndex = ModAllocator.ItemTypes_ShuffleStart;

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
            SaveGrindScriptInfo(file);
        }

        public static void LoadModCharacter(BinaryReader file)
        {
            LoadGrindScriptInfo(file);
        }

        public static void SaveModWorld(BinaryWriter file)
        {
        }

        public static void LoadModWorld(BinaryReader file)
        {
        }

        public static void SaveModArcade(BinaryWriter file)
        {
            SaveGrindScriptInfo(file);
        }

        public static void LoadModArcade(BinaryReader file)
        {
            LoadGrindScriptInfo(file);
        }
    }
}
