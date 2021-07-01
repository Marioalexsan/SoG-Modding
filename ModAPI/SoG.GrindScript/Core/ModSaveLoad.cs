using System.Collections.Generic;
using System.IO;
using System.Linq;
using System;

namespace SoG.Modding
{
    internal static class ModSaveLoad
    {
        public enum Saveables
        {
            InventoryItems,
            RoguelikePerks,
            RoguelikeTreatsCurses
        }

        private static readonly int GrindScriptVersion = 2;
        public static readonly string ModExt = ".gs";

        // Identifier methods

        /// <summary>
        /// Converts items from one type to another. <para/>
        /// This affects inventory, crafted items, discovered items, etc. <para/>
        /// This does not
        /// </summary>

        private static void ShuffleItem(ItemCodex.ItemTypes from, ItemCodex.ItemTypes to)
        {
            if (Enum.IsDefined(typeof(ItemCodex.ItemTypes), from))
                GrindScript.Logger.Warn($"Item {from} is a SoG item! Shuffling may cause issues.", source: "ShuffleItem");

            if (Enum.IsDefined(typeof(ItemCodex.ItemTypes), to))
                GrindScript.Logger.Warn($"Item {to} is a SoG item! Shuffling may cause issues.", source: "ShuffleItem");

            Inventory inventory = GrindScript.Game.xLocalPlayer.xInventory;
            Journal journal = GrindScript.Game.xLocalPlayer.xJournalInfo;

            // Shuffle inventory, discovered items, crafted items, and fishes

            if (inventory.denxInventory.ContainsKey(from))
            {
                inventory.denxInventory[to] = new Inventory.DisplayItem(inventory.denxInventory[from].iAmount, inventory.denxInventory[from].iPickupNumber, ItemCodex.GetItemDescription(to));
                inventory.denxInventory.Remove(from);
            }

            if (journal.henUniqueDiscoveredItems.Contains(from))
            {
                journal.henUniqueDiscoveredItems.Remove(from);
                journal.henUniqueDiscoveredItems.Add(to);
            }

            if (journal.henUniqueCraftedItems.Contains(from))
            {
                journal.henUniqueCraftedItems.Remove(from);
                journal.henUniqueCraftedItems.Add(to);
            }

            if (journal.henUniqueFishies.Contains(from))
            {
                journal.henUniqueFishies.Remove(from);
                journal.henUniqueFishies.Add(to);
            }
        }

        private static void ShufflePerk(RogueLikeMode.Perks from, RogueLikeMode.Perks to)
        {
            if (Enum.IsDefined(typeof(RogueLikeMode.Perks), from))
                GrindScript.Logger.Warn($"Perk {from} is from SoG! Shuffling may cause issues.", source: "ShufflePerk");

            if (Enum.IsDefined(typeof(RogueLikeMode.Perks), to))
                GrindScript.Logger.Warn($"Perk {to} is from SoG! Shuffling may cause issues.", source: "ShufflePerk");

            var session = GrindScript.Game.xGlobalData.xLocalRoguelikeData;

            if (session.enPerkSlot01 == from)
                session.enPerkSlot01 = to;

            if (session.enPerkSlot02 == from)
                session.enPerkSlot02 = to;

            if (session.enPerkSlot03 == from)
                session.enPerkSlot03 = to;

            for (int i = 0; i < session.lenPerksOwned.Count; i++)
            {
                if (session.lenPerksOwned[i] == from)
                    session.lenPerksOwned[i] = to;
            }
        }

        private static void ShuffleTreatCurse(RogueLikeMode.TreatsCurses from, RogueLikeMode.TreatsCurses to)
        {
            if (Enum.IsDefined(typeof(RogueLikeMode.TreatsCurses), from))
                GrindScript.Logger.Warn($"TreatCurse {from} is from SoG! Shuffling may cause issues.", source: "ShuffleTreatCurse");

            if (Enum.IsDefined(typeof(RogueLikeMode.TreatsCurses), to))
                GrindScript.Logger.Warn($"TreatCurse {to} is from SoG! Shuffling may cause issues.", source: "ShuffleTreatCurse");

            var session = GrindScript.Game.xGlobalData.xLocalRoguelikeData;

            if (session.enCurseTreatSlot01 == from)
                session.enCurseTreatSlot01 = to;

            if (session.enCurseTreatSlot02 == from)
                session.enCurseTreatSlot02 = to;

            if (session.enCurseTreatSlot03 == from)
                session.enCurseTreatSlot03 = to;
        }

        public static void IdentifyModUnique<T>(IEnumerable<IPersistentID<T>> targetUniques, Dictionary<T, string> targetSavedItems, ref T tempShuffleStart, Action<T, T> typeShuffler) where T : Enum
        {
            var savedItems = new Dictionary<T, string>(targetSavedItems);

            // Shuffle items into their correct ItemTypes IDs

            while (savedItems.Count > 0)
            {
                var saveItem = savedItems.First();
                bool found = false;

                GrindScript.Logger.Info($"Checking unique {saveItem.Key}:{saveItem.Value}");

                foreach (var modUnique in targetUniques)
                {
                    if (saveItem.Value == modUnique.ModID)
                    {
                        found = true;

                        // If game IDs coincide, quit early
                        if (saveItem.Key.Equals(modUnique.GameID)) 
                            break;

                        if (savedItems.ContainsKey(modUnique.GameID))
                        {
                            GrindScript.Logger.Debug($"Solving conflict: {savedItems[modUnique.GameID]} -> {tempShuffleStart}", source: "IdentifyModUnique");
                            typeShuffler(modUnique.GameID, tempShuffleStart);

                            savedItems.Add(tempShuffleStart, savedItems[modUnique.GameID]);
                            savedItems.Remove(modUnique.GameID);
                            tempShuffleStart = (T)Enum.ToObject(typeof(T), Convert.ToInt32(tempShuffleStart) + 1);
                        }

                        // Shuffle save ID to mod ID

                        GrindScript.Logger.Debug($"{saveItem.Value}: {saveItem.Key} -> {modUnique.GameID}", source: "IdentifyModUnique");
                        typeShuffler(modUnique.GameID, tempShuffleStart);
                    }
                }

                if (!found)
                    GrindScript.Logger.Warn($"{saveItem.Value} of type {typeof(T).Name} couldn't be identified!", source: "IdentifyModUnique");

                savedItems.Remove(saveItem.Key);
            }
        }

        // Save methods - these need to save additonal info such as mod list used

        public static void SaveGrindScriptInfo(BinaryWriter file, HashSet<Saveables> targets)
        {
            file.Write(GrindScriptVersion);

            // Write information for each mod
            file.Write(GrindScript._loadedScripts.Count);
            foreach (var mod in GrindScript._loadedScripts)
            {
                file.Write(mod.GetType().FullName);

                if (targets.Contains(Saveables.InventoryItems))
                {
                    // Items which were defined by this mod during this session
                    file.Write(mod.ModLib.Items.Count);
                    foreach (var item in mod.ModLib.Items.Values)
                    {
                        file.Write((int)item.GameID);
                        file.Write(item.ModID);
                    }
                }

                if (targets.Contains(Saveables.RoguelikePerks))
                {
                    file.Write(mod.ModLib.Perks.Count);
                    foreach (var item in mod.ModLib.Perks.Values)
                    {
                        file.Write((int)item.GameID);
                        file.Write(item.ModID);
                    }
                }

                if (targets.Contains(Saveables.RoguelikeTreatsCurses))
                {
                    file.Write(mod.ModLib.TreatsCurses.Count);
                    foreach (var item in mod.ModLib.TreatsCurses.Values)
                    {
                        file.Write((int)item.GameID);
                        file.Write(item.ModID);
                    }
                }
            }
        }

        public static void LoadGrindScriptInfo(BinaryReader file, HashSet<Saveables> targets)
        {
            var itemShuffleIndex = IDAllocator.ItemTypes_ShuffleStart;
            var perkShuffleIndex = IDAllocator.RoguelikePerk_ShuffleStart;
            var curseShuffleIndex = IDAllocator.TreatsCurses_ShuffleStart; // Not needed, really

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

                if (targets.Contains(Saveables.InventoryItems))
                {
                    Dictionary<ItemCodex.ItemTypes, string> savedItems = new Dictionary<ItemCodex.ItemTypes, string>();

                    for (int saveItems = file.ReadInt32(); saveItems > 0; saveItems--)
                        savedItems.Add((ItemCodex.ItemTypes)file.ReadInt32(), file.ReadString());

                    if (target != null)
                        IdentifyModUnique(target.ModLib.Items.Values, savedItems, ref itemShuffleIndex, ShuffleItem);
                }

                if (GSVersion >= 2)
                {
                    if (targets.Contains(Saveables.RoguelikePerks))
                    {
                        Dictionary<RogueLikeMode.Perks, string> savedPerks = new Dictionary<RogueLikeMode.Perks, string>();

                        for (int savePerk = file.ReadInt32(); savePerk > 0; savePerk--)
                            savedPerks.Add((RogueLikeMode.Perks)file.ReadInt32(), file.ReadString());

                        if (target != null)
                            IdentifyModUnique(target.ModLib.Perks.Values, savedPerks, ref perkShuffleIndex, ShufflePerk);
                    }

                    if (targets.Contains(Saveables.RoguelikeTreatsCurses))
                    {
                        Dictionary<RogueLikeMode.TreatsCurses, string> savedCurses = new Dictionary<RogueLikeMode.TreatsCurses, string>();

                        for (int saveCurse = file.ReadInt32(); saveCurse > 0; saveCurse--)
                            savedCurses.Add((RogueLikeMode.TreatsCurses)file.ReadInt32(), file.ReadString());

                        if (target != null)
                            IdentifyModUnique(target.ModLib.TreatsCurses.Values, savedCurses, ref curseShuffleIndex, ShuffleTreatCurse);
                    }
                }
            }
        }

        public static void SaveModCharacter(BinaryWriter file)
        {
            SaveGrindScriptInfo(file, new HashSet<Saveables>
            {
                Saveables.InventoryItems
            });
        }

        public static void LoadModCharacter(BinaryReader file)
        {
            LoadGrindScriptInfo(file, new HashSet<Saveables>
            {
                Saveables.InventoryItems
            });
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
            SaveGrindScriptInfo(file, new HashSet<Saveables>
            {
                Saveables.InventoryItems,
                Saveables.RoguelikePerks,
                Saveables.RoguelikeTreatsCurses
            });
        }

        public static void LoadModArcade(BinaryReader file)
        {
            LoadGrindScriptInfo(file, new HashSet<Saveables>
            {
                Saveables.InventoryItems,
                Saveables.RoguelikePerks,
                Saveables.RoguelikeTreatsCurses
            });
        }
    }
}
