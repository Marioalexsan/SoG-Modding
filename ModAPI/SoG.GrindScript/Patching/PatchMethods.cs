using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

namespace SoG.Modding
{
    /// <summary>
    /// Stores various methods used in patches by GrindScript
    /// </summary>
    public static class PatchMethods
    {
        private static IEnumerable<CodeInstruction> StartupThreadExecute_Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            MethodInfo target = GrindScript.GetGameType("SoG.DialogueCharacterLoading").GetMethod("Init", BindingFlags.Public | BindingFlags.Static);
            List<CodeInstruction> insertedCode = new List<CodeInstruction>
            {
                new CodeInstruction(OpCodes.Call, typeof(PatchMethods).GetTypeInfo().GetMethod("OnContentLoad", BindingFlags.NonPublic | BindingFlags.Static))
            };

            var newCode = PatchHelper.InsertAfterFirstMethod(instructions, generator, target, insertedCode);
            PatchHelper.LogCodeAroundTarget(newCode, target);
            return newCode;
        }

        private static void OnGame1Initialize()
        {
            typeof(GrindScript).GetTypeInfo().GetPrivateStaticMethod("Initialize").Invoke(null, new object[0]);
        }

        private static void OnContentLoad()
        {
            foreach (BaseScript mod in GrindScript.LoadedScripts)
                mod.OnCustomContentLoad();
        }

        private static void OnFinalDrawPrefix()
        {
            foreach (BaseScript mod in GrindScript.LoadedScripts)
                mod.OnDraw();
        }

        private static void OnPlayerTakeDamagePrefix(ref int iInDamage, ref byte byType)
        {
            foreach (BaseScript mod in GrindScript.LoadedScripts)
                mod.OnPlayerDamaged(ref iInDamage, ref byType);
        }

        private static void OnPlayerKilledPrefix()
        {
            foreach (BaseScript mod in GrindScript.LoadedScripts)
                mod.OnPlayerKilled();
        }

        private static void PostPlayerLevelUp(PlayerView xView)
        {
            foreach (BaseScript mod in GrindScript.LoadedScripts)
                mod.PostPlayerLevelUp(xView);
        }

        private static void OnEnemyTakeDamagePrefix(Enemy xEnemy, ref int iDamage, ref byte byType)
        {
            foreach (BaseScript mod in GrindScript.LoadedScripts)
                mod.OnEnemyDamaged(xEnemy, ref iDamage, ref byType);
        }

        private static void OnNPCTakeDamagePrefix(NPC xEnemy, ref int iDamage, ref byte byType)
        {
            foreach (BaseScript mod in GrindScript.LoadedScripts)
                mod.OnNPCDamaged(xEnemy, ref iDamage, ref byType);
        }

        private static void OnNPCInteractionPrefix(PlayerView xView, NPC xNPC)
        {
            foreach (BaseScript mod in GrindScript.LoadedScripts)
                mod.OnNPCInteraction(xNPC);
        }

        private static void OnArcadiaLoadPrefix()
        {
            foreach (BaseScript mod in GrindScript.LoadedScripts)
                mod.OnArcadiaLoad();
        }

        private static IEnumerable<CodeInstruction> Chat_ParseCommandTranspiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            MethodInfo target = typeof(string).GetMethod("ToLowerInvariant", BindingFlags.Public | BindingFlags.Instance);
            Label afterRet = generator.DefineLabel();
            List<CodeInstruction> insertedCode = new List<CodeInstruction>
            {
                new CodeInstruction(OpCodes.Ldloc_S, 2),
                new CodeInstruction(OpCodes.Ldarg_S, 1),
                new CodeInstruction(OpCodes.Ldarg_S, 2),
                new CodeInstruction(OpCodes.Call, typeof(PatchMethods).GetTypeInfo().GetMethod("OnChatParseCommand", BindingFlags.NonPublic | BindingFlags.Static)),
                new CodeInstruction(OpCodes.Brfalse, afterRet),
                new CodeInstruction(OpCodes.Ret),
                new CodeInstruction(OpCodes.Nop).WithLabels(afterRet)
            };

            var newCode = PatchHelper.InsertAfterFirstMethod(instructions, generator, target, insertedCode);
            PatchHelper.LogCodeAroundTarget(newCode, target);
            return newCode;
        }

        private static bool OnChatParseCommand(string command, string message, int connection)
        {
            foreach (BaseScript mod in GrindScript.LoadedScripts)
            {
                if (mod.OnChatParseCommand(command, message, connection)) 
                    return true;
            }
            return false;
        }

        private static void OnItemUsePrefix(ItemCodex.ItemTypes enItem, PlayerView xView, ref bool bSend)
        {
            if (xView.xViewStats.bIsDead) return;
            foreach (BaseScript mod in GrindScript.LoadedScripts)
                mod.OnItemUse(enItem, xView, ref bSend);
        }

        /// ModLibrary related stuff
        /// 

        private static bool OnGetItemDescription(ref ItemDescription __result, ItemCodex.ItemTypes enType)
        {
            if (!enType.IsModItem()) 
                return true;

            ModItemInfo details = GrindScript.GlobalLib.ModItems[enType].itemInfo;
            __result = details.vanilla;
            try
            {
                __result.txDisplayImage = details.managerToUse.Load<Texture2D>("Items/DropAppearance/" + details.resourceToUse);
            }
            catch (Exception e)
            {
                Console.WriteLine("Failed to load a drop appearance for item " + enType + ". Exception:\n" + e);
                __result.txDisplayImage = RenderMaster.txNullTex;
            }

            return false;
        }

        private static bool OnGetItemInstance(ref Item __result, ItemCodex.ItemTypes enType)
        {
            if (!enType.IsModItem()) 
                return true;

            ModItemInfo details = GrindScript.GlobalLib.ModItems[enType].itemInfo;
            ItemDescription xDesc = details.vanilla;

            __result = new Item()
            {
                enType = enType,
                sFullName = xDesc.sFullName,
                bGiveToServer = xDesc.lenCategory.Contains(ItemCodex.ItemCategories.GrantToServer)
            };

            try
            {
                __result.xRenderComponent.txTexture = details.managerToUse.Load<Texture2D>("Items/DropAppearance/" + details.resourceToUse);
            }
            catch (Exception e)
            {
                Console.WriteLine("Failed to load a drop appearance for item instance " + enType + ". Exception:\n" + e);
                __result.xRenderComponent.txTexture = RenderMaster.txNullTex;
            }

            __result.xCollisionComponent.xMovementCollider = new SphereCollider(10f, Vector2.Zero, __result.xTransform, 1f, __result) { bCollideWithFlat = true };

            try
            {
                string trueShadowTex = details.shadowToUse != "" ? details.shadowToUse : "hartass02";
                __result.xRenderComponent.txShadowTexture = GrindScript.Game.xLevelMaster.contRegionContent.Load<Texture2D>("Items/DropAppearance/" + details.shadowToUse);
            }
            catch (Exception e)
            {
                Console.WriteLine("Failed to load a shadow texture for item instance " + enType + ". Exception:\n" + e);
                __result.xRenderComponent.txShadowTexture = RenderMaster.txNullTex;
            }

            return false;
        }

        private static bool OnGetEquipmentInfo(ref EquipmentInfo __result, ItemCodex.ItemTypes enType)
        {
            if (!enType.IsModItem())
                return true;

            __result = GrindScript.GlobalLib.ModItems[enType].equipInfo.vanilla;

            return false;
        }

        private static bool OnGetFacegearInfo(ref FacegearInfo __result, ItemCodex.ItemTypes enType)
        {
            if (!enType.IsModItem())
                return true;

            ModEquipInfo modEquip = GrindScript.GlobalLib.ModItems[enType].equipInfo;
            __result = modEquip.vanilla as FacegearInfo;
            ContentManager manager = modEquip.managerToUse;

            try
            {
                string hatPath = "Sprites/Equipment/Facegear/" + modEquip.resourceToUse + "/";
                __result.atxTextures[0] = manager.Load<Texture2D>(hatPath + "Up");
                __result.atxTextures[1] = manager.Load<Texture2D>(hatPath + "Right");
                __result.atxTextures[2] = manager.Load<Texture2D>(hatPath + "Down");
                __result.atxTextures[3] = manager.Load<Texture2D>(hatPath + "Left");
            }
            catch (Exception e)
            {
                Console.WriteLine("Failed to load facegear textures for item " + enType + ". Exception:\n" + e);
                __result.atxTextures[0] = RenderMaster.txNullTex;
                __result.atxTextures[1] = RenderMaster.txNullTex;
                __result.atxTextures[2] = RenderMaster.txNullTex;
                __result.atxTextures[3] = RenderMaster.txNullTex;
            }

            return false;
        }

        private static bool OnGetHatInfo(ref HatInfo __result, ItemCodex.ItemTypes enType)
        {
            if (!enType.IsModItem())
                return true;

            ModEquipInfo modEquip = GrindScript.GlobalLib.ModItems[enType].equipInfo;
            __result = modEquip.vanilla as HatInfo;
            ContentManager manager = modEquip.managerToUse;

            // This shouldn't be too expensive if the content manager already has the textures
            try
            {
                string hatPath = "Sprites/Equipment/Hats/" + modEquip.resourceToUse + "/";
                __result.xDefaultSet.atxTextures[0] = manager.Load<Texture2D>(hatPath + "Up");
                __result.xDefaultSet.atxTextures[1] = manager.Load<Texture2D>(hatPath + "Right");
                __result.xDefaultSet.atxTextures[2] = manager.Load<Texture2D>(hatPath + "Down");
                __result.xDefaultSet.atxTextures[3] = manager.Load<Texture2D>(hatPath + "Left");
                foreach (var kvp in __result.denxAlternateVisualSets)
                {
                    string altPath = hatPath + modEquip.hatAltSetResources[kvp.Key] + "/";
                    kvp.Value.atxTextures[0] = manager.Load<Texture2D>(altPath + "Up");
                    kvp.Value.atxTextures[1] = manager.Load<Texture2D>(altPath + "Right");
                    kvp.Value.atxTextures[2] = manager.Load<Texture2D>(altPath + "Down");
                    kvp.Value.atxTextures[3] = manager.Load<Texture2D>(altPath + "Left");
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Failed to load hat textures for item " + enType + ". Exception:\n" + e);
                __result.xDefaultSet.atxTextures[0] = RenderMaster.txNullTex;
                __result.xDefaultSet.atxTextures[1] = RenderMaster.txNullTex;
                __result.xDefaultSet.atxTextures[2] = RenderMaster.txNullTex;
                __result.xDefaultSet.atxTextures[3] = RenderMaster.txNullTex;

                foreach (var kvp in __result.denxAlternateVisualSets)
                {
                    kvp.Value.atxTextures[0] = RenderMaster.txNullTex;
                    kvp.Value.atxTextures[1] = RenderMaster.txNullTex;
                    kvp.Value.atxTextures[2] = RenderMaster.txNullTex;
                    kvp.Value.atxTextures[3] = RenderMaster.txNullTex;
                }
            }

            return false;
        }

        private static bool OnGetWeaponInfo(ref WeaponInfo __result, ItemCodex.ItemTypes enType)
        {
            if (!enType.IsModItem())
                return true;

            __result = GrindScript.GlobalLib.ModItems[enType].equipInfo.vanilla as WeaponInfo;

            return false;
        }

        private static bool OnLoadBatch(ref Dictionary<ushort, string> dis, WeaponAssets.WeaponContentManager __instance)
        {
            // This is more or less a copy of the original, except we redirect the ContentManger's RootDirectory and use shortened asset paths
            if (!__instance.enType.IsModItem())
                return true;

            try
            {
                __instance.contWeaponContent.RootDirectory = GrindScript.GlobalLib.ModItems[__instance.enType].equipInfo.managerToUse.RootDirectory;
            }
            catch
            {
                Console.WriteLine("LoadBatch: couldn't set RootDirectory for item " + __instance.enType);
            }

            foreach (KeyValuePair<ushort, string> kvp in dis)
            {
                string sPath = kvp.Value.Replace("Weapons/", "");

                try
                {
                    __instance.ditxWeaponTextures.Add(kvp.Key, __instance.contWeaponContent.Load<Texture2D>(sPath));
                }
                catch
                {
                    GrindScript.Game.Log("Failed to load weapon texture at: " + __instance.contWeaponContent.RootDirectory + "/" + sPath);
                    Console.WriteLine("LoadBatch: failed to load weapon texture for item " + __instance.enType + " at " + __instance.contWeaponContent.RootDirectory + "/" + sPath);
                    __instance.ditxWeaponTextures[kvp.Key] = RenderMaster.txNullTex;
                }
            }

            return false;
        }

        private static bool OnGetAnimationSet(PlayerView xPlayerView, string sAnimation, string sDirection, bool bWithWeapon, bool bCustomHat, bool bWithShield, bool bWeaponOnTop, ref PlayerAnimationTextureSet __result)
        {
            // This is more or less a copy of the original, except for modded items we use shortened asset paths, skip some unused vanilla code, and plug in our own ContentManager

            __result = new PlayerAnimationTextureSet()
            {
                bWeaponOnTop = bWeaponOnTop
            };
            string sAttackPath = "";

            ContentManager VanillaContent = RenderMaster.contPlayerStuff;

            __result.txBase = VanillaContent.Load<Texture2D>("Sprites/Heroes/" + sAttackPath + sAnimation + "/" + sDirection);

            // Skipped code which isn't used in vanilla

            try
            {
                if (bWithShield && xPlayerView.xEquipment.DisplayShield != null && xPlayerView.xEquipment.DisplayShield.sResourceName != "")
                {
                    ItemCodex.ItemTypes enType = xPlayerView.xEquipment.DisplayShield.enItemType;
                    if (enType.IsModItem())
                    {
                        __result.txShield = GrindScript.GlobalLib.ModItems[enType].equipInfo.managerToUse.Load<Texture2D>("Sprites/Heroes/" + sAttackPath + sAnimation + "/" + xPlayerView.xEquipment.DisplayShield.sResourceName + "/" + sDirection);
                    }
                    else
                    {
                        __result.txShield = VanillaContent.Load<Texture2D>("Sprites/Heroes/" + sAttackPath + sAnimation + "/Shields/" + xPlayerView.xEquipment.DisplayShield.sResourceName + "/" + sDirection);
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Shield texture load failed! Exception: " + e.Message);
                __result.txShield = RenderMaster.txNullTex;
            }

            if (bWithWeapon)
                __result.txWeapon = RenderMaster.txNullTex;

            return false; // Never executes the original
        }
    }
}
