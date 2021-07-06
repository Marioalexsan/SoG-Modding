using System.Collections.Generic;
using System.IO;
using System.Linq;
using System;
using SoG.Modding.Core;

namespace SoG.Modding.Content
{
    // Currently this class only houses internals
    // Later on, we can allow each mod to store some flags or something

    public class SaveModding : ModdingLogic
    {
        public enum Saveables
        {
            InventoryItems,
            RoguelikePerks,
            RoguelikeTreatsCurses
        }

        public const int GrindScriptVersion = 2; // Maybe reset this before first release

        public const string ModExt = ".gs";

        public SaveModding(GrindScript modAPI)
            : base(modAPI) { }

        private void ShuffleItem(ItemCodex.ItemTypes from, ItemCodex.ItemTypes to)
        {
            if (Enum.IsDefined(typeof(ItemCodex.ItemTypes), from) || Enum.IsDefined(typeof(ItemCodex.ItemTypes), to))
                _modAPI.Logger.Warn($"One or both of the items are from SoG! Shuffling may cause issues.", source: nameof(ShuffleItem));

            Inventory inventory = _modAPI.Game.xLocalPlayer.xInventory;
            Journal journal = _modAPI.Game.xLocalPlayer.xJournalInfo;

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

        private void ShufflePerk(RogueLikeMode.Perks from, RogueLikeMode.Perks to)
        {
            if (Enum.IsDefined(typeof(RogueLikeMode.Perks), from) || Enum.IsDefined(typeof(RogueLikeMode.Perks), to))
                _modAPI.Logger.Warn($"One or both of the items are from SoG! Shuffling may cause issues.", nameof(ShufflePerk));

            var session = _modAPI.Game.xGlobalData.xLocalRoguelikeData;

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

        private void ShuffleTreatCurse(RogueLikeMode.TreatsCurses from, RogueLikeMode.TreatsCurses to)
        {
            if (Enum.IsDefined(typeof(RogueLikeMode.TreatsCurses), from) || Enum.IsDefined(typeof(RogueLikeMode.TreatsCurses), to))
                _modAPI.Logger.Warn($"One or both of the items are from SoG! Shuffling may cause issues.", nameof(ShuffleTreatCurse));

            var session = _modAPI.Game.xGlobalData.xLocalRoguelikeData;

            if (session.enCurseTreatSlot01 == from)
                session.enCurseTreatSlot01 = to;

            if (session.enCurseTreatSlot02 == from)
                session.enCurseTreatSlot02 = to;

            if (session.enCurseTreatSlot03 == from)
                session.enCurseTreatSlot03 = to;
        }

        private void IdentifyPersistentID<T>(IEnumerable<PersistentEntry<T>> targetUniques, Dictionary<T, string> targetSavedItems, ref T tempShuffleStart, Action<T, T> typeShuffler) where T : struct, Enum
        {
            var savedItems = new Dictionary<T, string>(targetSavedItems);

            while (savedItems.Count > 0)
            {
                var saveItem = savedItems.First();
                bool found = false;

                _modAPI.Logger.Info($"Checking unique {saveItem.Key}:{saveItem.Value}");

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
                            _modAPI.Logger.Debug($"Solving conflict: {savedItems[modUnique.GameID]} -> {tempShuffleStart}", source: "IdentifyModUnique");
                            typeShuffler(modUnique.GameID, tempShuffleStart);

                            savedItems.Add(tempShuffleStart, savedItems[modUnique.GameID]);
                            savedItems.Remove(modUnique.GameID);
                            tempShuffleStart = (T)Enum.ToObject(typeof(T), Convert.ToInt32(tempShuffleStart) + 1);
                        }

                        // Shuffle save ID to mod ID

                        _modAPI.Logger.Debug($"{saveItem.Value}: {saveItem.Key} -> {modUnique.GameID}", source: "IdentifyModUnique");
                        typeShuffler(modUnique.GameID, tempShuffleStart);
                    }
                }

                if (!found)
                    _modAPI.Logger.Warn($"{saveItem.Value} of type {typeof(T).Name} couldn't be identified!", source: nameof(IdentifyPersistentID));

                savedItems.Remove(saveItem.Key);
            }
        }

        // Save methods - these need to save additonal info such as mod list used

        internal void SaveGrindScriptInfo(BinaryWriter file, HashSet<Saveables> targets)
        {
            file.Write(GrindScriptVersion);

            // Write information for each mod
            file.Write(_modAPI.LoadedScripts.Count);
            foreach (var mod in _modAPI.LoadedScripts)
            {
                file.Write(mod.GetType().FullName);

                if (targets.Contains(Saveables.InventoryItems))
                {
                    // Items which were defined by this mod during this session
                    file.Write(mod.Library.Items.Count);
                    foreach (var item in mod.Library.Items.Values)
                    {
                        file.Write((int)item.GameID);
                        file.Write(item.ModID);
                    }
                }

                if (targets.Contains(Saveables.RoguelikePerks))
                {
                    file.Write(mod.Library.Perks.Count);
                    foreach (var item in mod.Library.Perks.Values)
                    {
                        file.Write((int)item.GameID);
                        file.Write(item.ModID);
                    }
                }

                if (targets.Contains(Saveables.RoguelikeTreatsCurses))
                {
                    file.Write(mod.Library.TreatsCurses.Count);
                    foreach (var item in mod.Library.TreatsCurses.Values)
                    {
                        file.Write((int)item.GameID);
                        file.Write(item.ModID);
                    }
                }
            }
        }

        internal void LoadGrindScriptInfo(BinaryReader file, HashSet<Saveables> targets)
        {
            var itemShuffleIndex = _modAPI.Allocator.ItemID.Max + 1;
            var perkShuffleIndex = _modAPI.Allocator.PerkID.Max + 1;
            var curseShuffleIndex = _modAPI.Allocator.TreatCurseID.Max + 1;

            int GSVersion = file.ReadInt32();

            if (GSVersion != GrindScriptVersion)
            {
                _modAPI.Logger.Warn($"Version mismatch! Using version {GrindScriptVersion} while save has {GSVersion}.");
            }

            // Read information for each mod
            // I've accidentally wrote the enums as Int32s. Hopefully it doesn't cause issues for ushort / byte enums

            int scriptCount = file.ReadInt32();
            while (scriptCount-- > 0)
            {
                string fullName = file.ReadString();

                BaseScript target = _modAPI.LoadedScripts.FirstOrDefault(mod => mod.GetType().FullName == fullName);
                if (target == null)
                {
                    _modAPI.Logger.Warn($"This save used mod {fullName}, which is not loaded right now!");
                    _modAPI.Logger.Warn($"Game objects can not be identified as a result...");
                }

                if (targets.Contains(Saveables.InventoryItems))
                {
                    Dictionary<ItemCodex.ItemTypes, string> savedItems = new Dictionary<ItemCodex.ItemTypes, string>();

                    for (int saveItems = file.ReadInt32(); saveItems > 0; saveItems--)
                        savedItems.Add((ItemCodex.ItemTypes)file.ReadInt32(), file.ReadString());

                    if (target != null)
                        IdentifyPersistentID(target.Library.Items.Values, savedItems, ref itemShuffleIndex, ShuffleItem);
                }

                if (GSVersion >= 2)
                {
                    if (targets.Contains(Saveables.RoguelikePerks))
                    {
                        Dictionary<RogueLikeMode.Perks, string> savedPerks = new Dictionary<RogueLikeMode.Perks, string>();

                        for (int savePerk = file.ReadInt32(); savePerk > 0; savePerk--)
                            savedPerks.Add((RogueLikeMode.Perks)file.ReadInt32(), file.ReadString());

                        if (target != null)
                            IdentifyPersistentID(target.Library.Perks.Values, savedPerks, ref perkShuffleIndex, ShufflePerk);
                    }

                    if (targets.Contains(Saveables.RoguelikeTreatsCurses))
                    {
                        Dictionary<RogueLikeMode.TreatsCurses, string> savedCurses = new Dictionary<RogueLikeMode.TreatsCurses, string>();

                        for (int saveCurse = file.ReadInt32(); saveCurse > 0; saveCurse--)
                            savedCurses.Add((RogueLikeMode.TreatsCurses)file.ReadInt32(), file.ReadString());

                        if (target != null)
                            IdentifyPersistentID(target.Library.TreatsCurses.Values, savedCurses, ref curseShuffleIndex, ShuffleTreatCurse);
                    }
                }
            }
        }

        internal void SaveModCharacter(BinaryWriter file)
        {
            SaveGrindScriptInfo(file, new HashSet<Saveables>
            {
                Saveables.InventoryItems
            });
        }

        internal void LoadModCharacter(BinaryReader file)
        {
            LoadGrindScriptInfo(file, new HashSet<Saveables>
            {
                Saveables.InventoryItems
            });
        }

        internal void SaveModWorld(BinaryWriter file)
        {
            // TODO
        }

        internal void LoadModWorld(BinaryReader file)
        {
            // TODO
        }

        internal void SaveModArcade(BinaryWriter file)
        {
            SaveGrindScriptInfo(file, new HashSet<Saveables>
            {
                Saveables.InventoryItems,
                Saveables.RoguelikePerks,
                Saveables.RoguelikeTreatsCurses
            });
        }

        internal void LoadModArcade(BinaryReader file)
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
