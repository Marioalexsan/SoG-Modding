using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SoG.Modding;
using System;
using System.Collections.Generic;
using System.IO;

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
                Logger.Info("Creating Items...");

                modShield = ModElements.CreateItem(this, "_Mod_Item0001",
                    new ItemConfig().Texts("Shield Example", "This is a custom shield!").Resources(ModContent, ModPath + "Items/ModShield/Icon"),
                    new EquipConfig(EquipType.Shield).Stats(ShldHP: 1337).Resource(ModContent, ModPath + "Items/ModShield")
                    );

                modAccessory = ModElements.CreateItem(this, "_Mod_Item0002",
                    new ItemConfig().Texts("Accessory Example", "This is a custom accessory that mimics a shield due to lazyness!").Resources(ModContent, ModPath + "Items/Common/Icon"),
                    new EquipConfig(EquipType.Accessory).Stats(ATK: 1337)
                    );

                modHat = ModElements.CreateItem(this, "_Mod_Item0003",
                    new ItemConfig().Texts("Hat Example", "This is a custom hat!").Resources(ModContent, ModPath + "Items/ModHat/Icon"),
                    new EquipConfig(EquipType.Hat).Stats(ATK: 1111).Resource(ModContent, ModPath + "Items/ModHat").HatOffsets(new Vector2(4f, 7f), new Vector2(5f, 5f), new Vector2(5f, 5f), new Vector2(4f, 7f))
                    );

                modFacegear = ModElements.CreateItem(this, "_Mod_Item0004",
                    new ItemConfig().Texts("Facegear Example", "This is a custom facegear!").Resources(ModContent, ModPath + "Items/ModFacegear/Icon"),
                    new EquipConfig(EquipType.Facegear).Stats(ATK: 1234).Resource(ModContent, ModPath + "Items/ModFacegear").FacegearOffsets(new Vector2(2f, -1f), new Vector2(5f, -3f), new Vector2(3f, -5f), new Vector2(2f, -1f))
                    );

                modOneHandedWeapon = ModElements.CreateItem(this, "_Mod_Item0005",
                    new ItemConfig().Texts("OneHandedMelee Example", "This is a custom 1H weapon! It has cool animations for downward attacks.").Resources(ModContent, ModPath + "Items/Mod1H/Icon"),
                    new EquipConfig(EquipType.Weapon).WeaponType(WeaponInfo.WeaponCategory.OneHanded, false).Stats(ATK: 1555).Resource(ModContent, ModPath + "Items/Mod1H")
                    );

                modTwoHandedWeapon = ModElements.CreateItem(this, "_Mod_Item0006",
                    new ItemConfig().Texts("TwoHandedMagic Example", "This is a custom 2H weapon!").Resources(ModContent, ModPath + "Items/Mod2H/Icon"),
                    new EquipConfig(EquipType.Weapon).WeaponType(WeaponInfo.WeaponCategory.TwoHanded, true).Stats(ATK: 776).Resource(ModContent, ModPath + "Items/Mod2H")
                    );

                modMisc1 = ModElements.CreateItem(this, "_Mod_Item0007",
                    new ItemConfig().Texts("Mod Misc 1", "This is a custom miscellaneous item!").Resources(ModContent, ModPath + "Items/Common/Icon").Categories(ItemCodex.ItemCategories.Misc)
                    );

                modMisc2 = ModElements.CreateItem(this, "_Mod_Item0008",
                    new ItemConfig().Texts("Mod Misc 2", "This is another custom misc item!").Resources(ModContent, ModPath + "Items/Common/Icon").Categories(ItemCodex.ItemCategories.Misc)
                    );

                ModElements.AddRecipe(modOneHandedWeapon, new Dictionary<ItemCodex.ItemTypes, ushort>
                {
                    [modMisc1] = 5,
                    [modMisc2] = 10,
                    [modTwoHandedWeapon] = 1
                });

                Logger.Info("Done with Creating Items!");

                Logger.Info("Building sounds...");

                ModElements.ConfigureModAudio(this,
                    new AudioConfig().AddMusicForRegion("FeatureExample", "Intro", "Clash", "DeafSilence").AddMusicForRegion("FeatureExampleStuff", "Ripped", "Destiny")
                    );

                audioIntro = ModElements.GetMusicID(this, "Intro");
                audioDestiny = ModElements.GetMusicID(this, "Destiny");
                audioRipped = ModElements.GetMusicID(this, "Ripped");
                audioClash = ModElements.GetMusicID(this, "Clash");
                audioDeafSilence = ModElements.GetMusicID(this, "DeafSilence");

                // Testing the song redirects in a bit more detail

                Logger.Info("Testing sound redirects...");

                ModElements.RedirectVanillaMusic("BossBattle01", "BishopBattle"); // Redirect is invalid
                ModElements.RedirectVanillaMusic("BossBattle01", "GS_1337_M1337"); // Redirect is invalid
                ModElements.RedirectVanillaMusic("GS_1337_M1337", audioClash); // Vanilla is invalid

                ModElements.RedirectVanillaMusic("BossBattle01", audioClash); // Sets a redirect
                ModElements.RedirectVanillaMusic("BossBattle01", audioRipped); // Overrides the redirect
                ModElements.RedirectVanillaMusic("BossBattle01", ""); // Clears the redirect

                Logger.Info("Redirect tests done!");

                // Doing the actual redirects

                ModElements.RedirectVanillaMusic("BossBattle01", audioClash);
                ModElements.RedirectVanillaMusic("BishopBattle", audioRipped);

                Logger.Info("Done with sounds!");

                Logger.Info("Setting up commands...");

                SetupCommands();

                Logger.Info("Commands set up successfully!");

                Logger.Info("Setting up levels...");

                SetupLevels();

                Logger.Info("Levels set up successfully!");
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

        public override void OnPlayerDamaged(ref int damage, ref byte type)
        {
            Logger.Info("OnPlayerDamaged() called!");
        }

        public override void OnPlayerKilled()
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

        private void SetupCommands()
        {
            var parsers = new Dictionary<string, CommandParser>
            {
                ["GiveItems"] = (argList, _) =>
                {
                    string[] args = argList.Split(' ');
                    if (NetUtils.IsLocalOrServer)
                    {
                        PlayerView localPlayer = GrindScript.Game.xLocalPlayer;
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
                        GrindScript.Game.xSoundSystem.PlaySong(ID, true);
                    else CAS.AddChatMessage("Unknown mod music!");
                },

                ["TellIDs"] = (argList, _) =>
                {
                    Inventory inv = GrindScript.Game.xLocalPlayer.xInventory;
                    CAS.AddChatMessage("Shield:" + (int)modShield + ", count: " + inv.GetAmount(modShield));
                    CAS.AddChatMessage("Accessory:" + (int)modAccessory + ", count: " + inv.GetAmount(modAccessory));
                    CAS.AddChatMessage("Hat:" + (int)modHat + ", count: " + inv.GetAmount(modHat));
                    CAS.AddChatMessage("Facegear:" + (int)modFacegear + ", count: " + inv.GetAmount(modFacegear));
                    CAS.AddChatMessage("One Handed:" + (int)modOneHandedWeapon + ", count: " + inv.GetAmount(modOneHandedWeapon));
                    CAS.AddChatMessage("Two Handed:" + (int)modTwoHandedWeapon + ", count: " + inv.GetAmount(modTwoHandedWeapon));
                },

                ["GibCraft"] = (argList, _) =>
                {
                    PlayerView localPlayer = GrindScript.Game.xLocalPlayer;
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
                    GrindScript.Game._Level_PrepareSwitchAuto(LevelBlueprint.GetBlueprint(modLevel), 0);
                }
            };

            ModElements.ConfigureCommandsFrom(this, parsers);
        }

        private void SetupLevels()
        {
            LevelConfig cfg = new LevelConfig();
            cfg.WorldRegion(Level.WorldRegion.PillarMountains);

            cfg.Builder(CaveLevelStuff.Build);

            modLevel = LevelElements.CreateLevel(this, cfg);
        }

        public override void OnItemUse(ItemCodex.ItemTypes enItem, PlayerView xView, ref bool bSend)
        {
            Logger.Info("OnItemUse() called!");
        }
    }
}
