using Microsoft.Xna.Framework;
using SoG.Modding;
using System;
using System.Collections.Generic;

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

        string audioIntro = "";
        string audioRipped = "";
        string audioDestiny = "";
        string audioClash = "";
        string audioDeafSilence = "";

        public FeatureExample()
        {
            Logger.Info("FeatureExample is an example mod that tests GrindScript functionality.");
        }

        public override void LoadContent()
        {
            Logger.Info("Loading....");
            try
            {
                Logger.Info("Creating Items...");

                modShield = ModContent.CreateItem(this,
                    new ItemBuilder().Texts("Shield Example", "This is a custom shield!").Resources(CustomAssets, "WoodenShield"),
                    new EquipBuilder(EquipType.Shield).Stats(ShldHP: 1337).Resource(CustomAssets, "Wooden")
                    );

                modAccessory = ModContent.CreateItem(this,
                    new ItemBuilder().Texts("Accessory Example", "This is a custom accessory that mimics a shield due to lazyness!").Resources(CustomAssets, "WoodenShield"),
                    new EquipBuilder(EquipType.Accessory).Stats(ATK: 1337).Resource(CustomAssets, "Wooden")
                    );

                modHat = ModContent.CreateItem(this,
                    new ItemBuilder().Texts("Hat Example", "This is a custom hat!").Resources(CustomAssets, "Slimeus"),
                    new EquipBuilder(EquipType.Hat).Stats(ATK: 1111).Resource(CustomAssets, "Slimeus").HatOffsets(new Vector2(4f, 7f), new Vector2(5f, 5f), new Vector2(5f, 5f), new Vector2(4f, 7f))
                    );

                modFacegear = ModContent.CreateItem(this,
                    new ItemBuilder().Texts("Facegear Example", "This is a custom facegear!").Resources(CustomAssets, "Flybold"),
                    new EquipBuilder(EquipType.Facegear).Stats(ATK: 1234).Resource(CustomAssets, "Flybold").FacegearOffsets(new Vector2(2f, -1f), new Vector2(5f, -3f), new Vector2(3f, -5f), new Vector2(2f, -1f))
                    );

                modOneHandedWeapon = ModContent.CreateItem(this,
                    new ItemBuilder().Texts("OneHandedMelee Example", "This is a custom 1H weapon! It has custom animations for downward basic attacks.").Resources(CustomAssets, "Crowbar"),
                    new EquipBuilder(EquipType.Weapon).WeaponType(WeaponInfo.WeaponCategory.OneHanded, false).Stats(ATK: 1555).Resource(CustomAssets, "IronSword")
                    );

                modTwoHandedWeapon = ModContent.CreateItem(this,
                    new ItemBuilder().Texts("TwoHandedMagic Example", "This is a custom 2H weapon!").Resources(CustomAssets, "Claymore"),
                    new EquipBuilder(EquipType.Weapon).WeaponType(WeaponInfo.WeaponCategory.TwoHanded, true).Stats(ATK: 776).Resource(CustomAssets, "Claymore")
                    );

                Logger.Info("Done with Creating Items!");

                Logger.Info("Building sounds...");

                ModContent.ConfigureModAudio(this,
                    new AudioConfig().AddMusicForRegion("FeatureExample", "Intro", "Clash", "DeafSilence").AddMusicForRegion("FeatureExampleStuff", "Ripped", "Destiny")
                    );

                audioIntro = ModContent.GetMusicID(this, "Intro");
                audioDestiny = ModContent.GetMusicID(this, "Destiny");
                audioRipped = ModContent.GetMusicID(this, "Ripped");
                audioClash = ModContent.GetMusicID(this, "Clash");
                audioDeafSilence = ModContent.GetMusicID(this, "DeafSilence");

                // Testing the song redirects in a bit more detail

                Logger.Info("Testing sound redirects...");

                ModContent.RedirectVanillaMusic("BossBattle01", "BishopBattle"); // Redirect is invalid
                ModContent.RedirectVanillaMusic("BossBattle01", "GS_1337_M1337"); // Redirect is invalid
                ModContent.RedirectVanillaMusic("GS_1337_M1337", audioClash); // Vanilla is invalid

                ModContent.RedirectVanillaMusic("BossBattle01", audioClash); // Sets a redirect
                ModContent.RedirectVanillaMusic("BossBattle01", audioRipped); // Overrides the redirect
                ModContent.RedirectVanillaMusic("BossBattle01", ""); // Clears the redirect

                Logger.Info("Redirect tests done!");

                // Doing the actual redirects

                ModContent.RedirectVanillaMusic("BossBattle01", audioClash);
                ModContent.RedirectVanillaMusic("BishopBattle", audioRipped);

                Logger.Info($"Music IDs: Intro - {audioIntro}, Destiny - {audioDestiny}, Ripped - {audioRipped}");

                Logger.Info("Done with sounds!");

                Logger.Info("Setting up commands...");

                SetupCommands();

                Logger.Info("Commands set up successfully!");
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
                    if (ModNetworking.IsLocalOrServer)
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
                }
            };

            ModContent.ConfigureCommandsFrom(this, parsers);
        }

        public override void OnItemUse(ItemCodex.ItemTypes enItem, PlayerView xView, ref bool bSend)
        {
            Logger.Info("OnItemUse() called!");
        }
    }
}
