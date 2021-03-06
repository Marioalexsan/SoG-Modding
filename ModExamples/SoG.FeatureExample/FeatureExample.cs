﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using SoG.Modding.Core;
using SoG.Modding.Content;
using SoG.Modding.Extensions;
using SoG.Modding.Utils;
using SoG.Modding.Content.Configs;

namespace SoG.FeatureExample
{
    public class FeatureExample: BaseScript
    {
        bool drewStuff = false;
        int levelUpsSoFar = 0;

        ItemCodex.ItemTypes modOneHandedWeapon = ItemCodex.ItemTypes.Null;
        ItemCodex.ItemTypes modTwoHandedWeapon = ItemCodex.ItemTypes.Null;
        ItemCodex.ItemTypes modShield = ItemCodex.ItemTypes.Null;
        ItemCodex.ItemTypes modHat = ItemCodex.ItemTypes.Null;
        ItemCodex.ItemTypes modFacegear = ItemCodex.ItemTypes.Null;
        ItemCodex.ItemTypes modAccessory = ItemCodex.ItemTypes.Null;

        ItemCodex.ItemTypes modMisc1 = ItemCodex.ItemTypes.Null;
        ItemCodex.ItemTypes modMisc2 = ItemCodex.ItemTypes.Null;

        Level.ZoneEnum modLevel = Level.ZoneEnum.None;

        string audioIntro = "";
        string audioRipped = "";
        string audioDestiny = "";
        string audioClash = "";
        string audioDeafSilence = "";

        public override void LoadContent()
        {
            Logger.Info("Loading FeatureExample mod....");
            try
            {
                SetupItems();

                SetupAudio();

                SetupCommands();

                SetupLevels();

                SetupRoguelike();
            }
            catch(Exception e)
            {
                Logger.Error($"Failed to load! Exception message: {e.Message}");
                return;
            }
            Logger.Info("Loaded Successfully!");
        }

        public override void OnDraw()
        {
            if (drewStuff) return;
            drewStuff = true;
            Logger.Info("OnDraw() called!");
            Logger.Info("Won't bother outputting any extra draws due to 60 draws / second!");
        }

        public override void OnPlayerDamaged(PlayerView view, ref int damage, ref byte type)
        {
            Logger.Info("OnPlayerDamaged() called!");
        }

        public override void OnPlayerKilled(PlayerView player)
        {
            Logger.Info("OnPlayerKilled() called!");
        }

        public override void PostPlayerLevelUp(PlayerView player)
        {
            levelUpsSoFar++;
            if (levelUpsSoFar == 50 || levelUpsSoFar < 50 && (levelUpsSoFar + 7) % 8 == 0)
            {
                Logger.Info("PostPlayerLevelUp() called! Cumulative count: " + levelUpsSoFar);
            }
            if (levelUpsSoFar == 50)
            {
                Logger.Info("Won't bother outputting any more levelups since there's too many!");
            }
        }

        public override void OnEnemyDamaged(Enemy enemy, ref int damage, ref byte type)
        {
            Logger.Info("OnEnemyDamaged() called!");
        }

        public override void OnNPCDamaged(NPC enemy, ref int damage, ref byte type)
        {
            Logger.Info("OnNPCDamaged() called!");
        }

        public override void OnNPCInteraction(NPC npc)
        {
            Logger.Info("OnNPCInteraction() called!");
        }

        public override void OnArcadiaLoad()
        {
            Logger.Info("OnArcadiaLoad() called!");
        }

        public override void OnItemUse(ItemCodex.ItemTypes enItem, PlayerView xView, ref bool bSend)
        {
            Logger.Info("OnItemUse() called!");
        }

        void SetupItems()
        {
            Logger.Info("Creating Items...");

            Dictionary<string, ItemConfig> ItemLibrary = new Dictionary<string, ItemConfig>
            {
                ["_Mod_Item0001"] = new ItemConfig("_Mod_Item0001")
                {
                    Name = "Shield Example",
                    Description = "This is a custom shield!",
                    EquipType = EquipmentType.Shield,
                    IconPath = ModPath + "Items/ModShield/Icon",
                    EquipResourcePath = ModPath + "Items/ModShield",
                    ShldHP = 1337
                },

                ["_Mod_Item0002"] = new ItemConfig("_Mod_Item0002")
                {
                    Name = "Accessory Example",
                    Description = "This is a custom accessory that mimics a shield due to lazyness!",
                    IconPath = ModPath + "Items/Common/Icon",
                    EquipType = EquipmentType.Accessory,
                    ATK = 1337
                },

                ["_Mod_Item0003"] = new ItemConfig("_Mod_Item0003")
                {
                    Name = "Hat Example",
                    Description = "This is a custom hat!",
                    IconPath = ModPath + "Items/ModHat/Icon",
                    EquipType = EquipmentType.Hat,
                    EquipResourcePath = ModPath + "Items/ModHat",
                    ATK = 1111
                }
                    .SetHatOffsets(new Vector2(4f, 7f), new Vector2(5f, 5f), new Vector2(5f, 5f), new Vector2(4f, 7f)),

                ["_Mod_Item0004"] = new ItemConfig("_Mod_Item0004")
                {
                    Name = "Facegear Example",
                    Description = "This is a custom facegear!",
                    IconPath = ModPath + "Items/ModFacegear/Icon",
                    EquipType = EquipmentType.Facegear,
                    EquipResourcePath = ModPath + "Items/ModFacegear",
                    ATK = 1234
                }
                    .SetFacegearOffsets(new Vector2(2f, -1f), new Vector2(5f, -3f), new Vector2(3f, -5f), new Vector2(2f, -1f)),

                ["_Mod_Item0005"] = new ItemConfig("_Mod_Item0005")
                {
                    Name = "OneHandedMelee Example",
                    Description = "This is a custom 1H weapon! It has cool animations for downward attacks.",
                    IconPath = ModPath + "Items/Mod1H/Icon",
                    EquipType = EquipmentType.Weapon,
                    WeaponType = WeaponInfo.WeaponCategory.OneHanded,
                    MagicWeapon = false,
                    EquipResourcePath = ModPath + "Items/Mod1H",
                    ATK = 1555
                },

                ["_Mod_Item0006"] = new ItemConfig("_Mod_Item0006")
                {
                    Name = "TwoHandedMagic Example",
                    Description = "This is a custom 2H weapon!",
                    IconPath = ModPath + "Items/Mod2H/Icon",
                    EquipType = EquipmentType.Weapon,
                    WeaponType = WeaponInfo.WeaponCategory.TwoHanded,
                    MagicWeapon = true,
                    EquipResourcePath = ModPath + "Items/Mod2H",
                    ATK = 776
                },

                ["_Mod_Item0007"] = new ItemConfig("_Mod_Item0007")
                {
                    Name = "Mod Misc 1",
                    Description = "This is a custom miscellaneous item!",
                    IconPath = ModPath + "Items/Common/Icon",
                    Categories = { ItemCodex.ItemCategories.Misc }
                },

                ["_Mod_Item0008"] = new ItemConfig("_Mod_Item0008")
                {
                    Name = "Mod Misc 2",
                    Description = "This is another custom miscellaneous item!",
                    IconPath = ModPath + "Items/Common/Icon",
                    Categories = { ItemCodex.ItemCategories.Misc }
                }
            };

            ModAPI.ItemAPI.CreateItems(ItemLibrary.Values);

            modShield = ModAPI.ItemAPI.GetItemType(this, "_Mod_Item0001");
            modAccessory = ModAPI.ItemAPI.GetItemType(this, "_Mod_Item0002");
            modHat = ModAPI.ItemAPI.GetItemType(this, "_Mod_Item0003");
            modFacegear = ModAPI.ItemAPI.GetItemType(this, "_Mod_Item0004");
            modOneHandedWeapon = ModAPI.ItemAPI.GetItemType(this, "_Mod_Item0005");
            modTwoHandedWeapon = ModAPI.ItemAPI.GetItemType(this, "_Mod_Item0006");
            modMisc1 = ModAPI.ItemAPI.GetItemType(this, "_Mod_Item0007");
            modMisc2 = ModAPI.ItemAPI.GetItemType(this, "_Mod_Item0008");

            ModAPI.ItemAPI.AddRecipe(modOneHandedWeapon, new Dictionary<ItemCodex.ItemTypes, ushort>
            {
                [modMisc1] = 5,
                [modMisc2] = 10,
                [modTwoHandedWeapon] = 1
            });

            Logger.Info("Done with Creating Items!");
        }

        void SetupAudio()
        {
            Logger.Info("Building sounds...");

            ModAPI.AudioAPI.CreateAudio(new AudioConfig().AddMusicForRegion("FeatureExample", "Intro", "Clash", "DeafSilence").AddMusicForRegion("FeatureExampleStuff", "Ripped", "Destiny"));

            audioIntro = ModAPI.AudioAPI.GetMusicID(this, "Intro");
            audioDestiny = ModAPI.AudioAPI.GetMusicID(this, "Destiny");
            audioRipped = ModAPI.AudioAPI.GetMusicID(this, "Ripped");
            audioClash = ModAPI.AudioAPI.GetMusicID(this, "Clash");
            audioDeafSilence = ModAPI.AudioAPI.GetMusicID(this, "DeafSilence");

            // Uncomment this if you want to see more redirect behavior
            /*
            Logger.Info("Testing sound redirects...");

            ModAPI.AudioAPI.RedirectVanillaMusic("BossBattle01", "BishopBattle"); // Redirect is invalid
            ModAPI.AudioAPI.RedirectVanillaMusic("BossBattle01", "GS_1337_M1337"); // Redirect is invalid
            ModAPI.AudioAPI.RedirectVanillaMusic("GS_1337_M1337", audioClash); // Vanilla is invalid

            ModAPI.AudioAPI.RedirectVanillaMusic("BossBattle01", audioClash); // Sets a redirect
            ModAPI.AudioAPI.RedirectVanillaMusic("BossBattle01", audioRipped); // Overrides the redirect
            ModAPI.AudioAPI.RedirectVanillaMusic("BossBattle01", ""); // Clears the redirect

            Logger.Info("Redirect tests done!");
            */

            ModAPI.AudioAPI.RedirectVanillaMusic("BossBattle01", audioClash);
            ModAPI.AudioAPI.RedirectVanillaMusic("BishopBattle", audioRipped);

            Logger.Info("Done with sounds!");
        }

        void SetupCommands()
        {
            Logger.Info("Setting up commands...");

            var parsers = new Dictionary<string, CommandParser>
            {
                ["GiveItems"] = (argList, _) =>
                {
                    string[] args = argList.Split(' ');
                    if (NetTools.IsLocalOrServer)
                    {
                        PlayerView localPlayer = APIGlobals.Game.xLocalPlayer;
                        CAS.AddChatMessage("Dropping Items!");
                        modShield.SpawnItem(localPlayer);
                        modAccessory.SpawnItem(localPlayer);
                        modFacegear.SpawnItem(localPlayer);
                        modHat.SpawnItem(localPlayer);
                        modOneHandedWeapon.SpawnItem(localPlayer);
                        modTwoHandedWeapon.SpawnItem(localPlayer);
                    }
                    else CAS.AddChatMessage("You can't do that if you're a client!");
                },

                ["PlayMusic"] = (argList, _) =>
                {
                    string[] args = argList.Split(' ');
                    if (args.Length != 1)
                        CAS.AddChatMessage("Usage: /PlayMusic <Audio>");

                    var music = new Dictionary<string, string>
                    {
                        ["Intro"] = audioIntro,
                        ["Destiny"] = audioDestiny,
                        ["Ripped"] = audioRipped,
                        ["Clash"] = audioClash,
                        ["DeafSilence"] = audioDeafSilence
                    };

                    if (music.TryGetValue(args[0], out string ID))
                        APIGlobals.Game.xSoundSystem.PlaySong(ID, true);
                    else CAS.AddChatMessage("Unknown mod music!");
                },

                ["TellIDs"] = (argList, _) =>
                {
                    Inventory inv = APIGlobals.Game.xLocalPlayer.xInventory;
                    CAS.AddChatMessage("Shield:" + (int)modShield + ", count: " + inv.GetAmount(modShield));
                    CAS.AddChatMessage("Accessory:" + (int)modAccessory + ", count: " + inv.GetAmount(modAccessory));
                    CAS.AddChatMessage("Hat:" + (int)modHat + ", count: " + inv.GetAmount(modHat));
                    CAS.AddChatMessage("Facegear:" + (int)modFacegear + ", count: " + inv.GetAmount(modFacegear));
                    CAS.AddChatMessage("One Handed:" + (int)modOneHandedWeapon + ", count: " + inv.GetAmount(modOneHandedWeapon));
                    CAS.AddChatMessage("Two Handed:" + (int)modTwoHandedWeapon + ", count: " + inv.GetAmount(modTwoHandedWeapon));
                },

                ["GibCraft"] = (argList, _) =>
                {
                    PlayerView localPlayer = APIGlobals.Game.xLocalPlayer;
                    CAS.AddChatMessage("Dropping Items!");

                    int amount = 10;
                    while (amount-- > 0)
                    {
                        modMisc1.SpawnItem(localPlayer);
                        modMisc2.SpawnItem(localPlayer);
                    }
                },

                ["Yeet"] = (argList, _) =>
                {
                    APIGlobals.Game._Level_PrepareSwitchAuto(LevelBlueprint.GetBlueprint(modLevel), 0);
                },
            };

            ModAPI.MiscAPI.CreateCommand(parsers);

            Logger.Info("Commands set up successfully!");
        }

        void SetupLevels()
        {
            Logger.Info("Setting up levels...");

            LevelConfig cfg = new LevelConfig()
            {
                WorldRegion = Level.WorldRegion.PillarMountains,
                Builder = CaveLevelStuff.Build,
                Loader = null
            };

            modLevel = ModAPI.LevelAPI.CreateLevel(cfg);

            Logger.Info("Levels set up successfully!");
        }

        void SetupRoguelike()
        {
            Logger.Info("Doing Roguelike stuff...");

            PerkConfig perk01 = new PerkConfig("_Mod_Perk001")
            {
                Name = "Soul Booster",
                Description = "Gain 10 extra EP.",
                EssenceCost = 15,
                RunStartActivator = (player) =>
                {
                    player.xEntity.xBaseStats.iMaxEP += 10;
                    player.xEntity.xBaseStats._ichkBaseMaxEP += 10 * 2;
                },
                TexturePath = ModPath + "RogueLike/SoulBooster"
            };

            ModAPI.RoguelikeAPI.CreatePerk(perk01);

            TreatCurseConfig curse01 = new TreatCurseConfig("_Mod_Curse001")
            {
                Name = "Placeholder 01",
                Description = "Placeholder-y stuff!",
                IsCurse = true,
                ScoreModifier = 5,
                TexturePath = ""
            };

            TreatCurseConfig curse02 = new TreatCurseConfig("_Mod_Curse002")
            {
                Name = "Placeholder 02",
                Description = "Placeholder-y stuff!",
                IsCurse = true,
                ScoreModifier = 5,
                TexturePath = ""
            };

            TreatCurseConfig curse03 = new TreatCurseConfig("_Mod_Curse003")
            {
                Name = "Placeholder 03",
                Description = "Placeholder-y stuff!",
                IsCurse = true,
                ScoreModifier = 5,
                TexturePath = ""
            };

            TreatCurseConfig treat01 = new TreatCurseConfig("_Mod_Treat001")
            {
                Name = "Placeholder 01",
                Description = "Placeholder-y stuff!",
                IsCurse = false,
                ScoreModifier = -5,
                TexturePath = ""
            };

            ModAPI.RoguelikeAPI.CreateTreatOrCurse(treat01);
            ModAPI.RoguelikeAPI.CreateTreatOrCurse(curse01);
            ModAPI.RoguelikeAPI.CreateTreatOrCurse(curse02);
            ModAPI.RoguelikeAPI.CreateTreatOrCurse(curse03);

            Logger.Info("Done with Roguelike stuff!");
        }
    }
}
