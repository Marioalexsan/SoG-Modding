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
    /// Stores various methods used in patches by GrindScript.
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
            GrindScript.Logger.DebugInspectCode(newCode, target);
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
            GrindScript.Logger.DebugInspectCode(instructions, target, 3, 10);
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

            ModItem details = ModLibrary.Global.ModItems[enType].itemInfo;
            __result = details.vanilla;
            __result.txDisplayImage = Utils.TryLoadTex("Items/DropAppearance/" + details.resourceToUse, details.managerToUse);

            return false;
        }

        private static bool OnGetItemInstance(ref Item __result, ItemCodex.ItemTypes enType)
        {
            if (!enType.IsModItem()) 
                return true;

            ModItem details = ModLibrary.Global.ModItems[enType].itemInfo;
            string trueShadowTex = details.shadowToUse != "" ? details.shadowToUse : "hartass02";
            ItemDescription xDesc = details.vanilla;

            __result = new Item()
            {
                enType = enType,
                sFullName = xDesc.sFullName,
                bGiveToServer = xDesc.lenCategory.Contains(ItemCodex.ItemCategories.GrantToServer)
            };

            __result.xRenderComponent.txTexture = Utils.TryLoadTex("Items/DropAppearance/" + details.resourceToUse, details.managerToUse);
            __result.xRenderComponent.txShadowTexture = Utils.TryLoadTex("Items/DropAppearance/" + trueShadowTex, details.managerToUse);
            __result.xCollisionComponent.xMovementCollider = new SphereCollider(10f, Vector2.Zero, __result.xTransform, 1f, __result) { bCollideWithFlat = true };

            return false;
        }

        private static bool OnGetEquipmentInfo(ref EquipmentInfo __result, ItemCodex.ItemTypes enType)
        {
            if (!enType.IsModItem())
                return true;

            __result = ModLibrary.Global.ModItems[enType].equipInfo.vanilla;

            return false;
        }

        private static bool OnGetFacegearInfo(ref FacegearInfo __result, ItemCodex.ItemTypes enType)
        {
            if (!enType.IsModItem())
                return true;

            ModEquip modEquip = ModLibrary.Global.ModItems[enType].equipInfo;
            string hatPath = "Sprites/Equipment/Facegear/" + modEquip.resourceToUse + "/";
            ContentManager manager = modEquip.managerToUse;

            __result = modEquip.vanilla as FacegearInfo;
            __result.atxTextures[0] = Utils.TryLoadTex(hatPath + "Up", manager);
            __result.atxTextures[1] = Utils.TryLoadTex(hatPath + "Right", manager);
            __result.atxTextures[2] = Utils.TryLoadTex(hatPath + "Down", manager);
            __result.atxTextures[3] = Utils.TryLoadTex(hatPath + "Left", manager);

            return false;
        }

        private static bool OnGetHatInfo(ref HatInfo __result, ItemCodex.ItemTypes enType)
        {
            if (!enType.IsModItem())
                return true;

            ModEquip modEquip = ModLibrary.Global.ModItems[enType].equipInfo;
            string hatPath = "Sprites/Equipment/Hats/" + modEquip.resourceToUse + "/";
            ContentManager manager = modEquip.managerToUse;

            __result = modEquip.vanilla as HatInfo;
            __result.xDefaultSet.atxTextures[0] = Utils.TryLoadTex(hatPath + "Up", manager);
            __result.xDefaultSet.atxTextures[1] = Utils.TryLoadTex(hatPath + "Right", manager);
            __result.xDefaultSet.atxTextures[2] = Utils.TryLoadTex(hatPath + "Down", manager);
            __result.xDefaultSet.atxTextures[3] = Utils.TryLoadTex(hatPath + "Left", manager);
            foreach (var kvp in __result.denxAlternateVisualSets)
            {
                string altPath = hatPath + modEquip.hatAltSetResources[kvp.Key] + "/";
                kvp.Value.atxTextures[0] = Utils.TryLoadTex(altPath + "Up", manager);
                kvp.Value.atxTextures[1] = Utils.TryLoadTex(altPath + "Right", manager);
                kvp.Value.atxTextures[2] = Utils.TryLoadTex(altPath + "Down", manager);
                kvp.Value.atxTextures[3] = Utils.TryLoadTex(altPath + "Left", manager);
            }

            return false;
        }

        private static bool OnGetWeaponInfo(ref WeaponInfo __result, ItemCodex.ItemTypes enType)
        {
            if (!enType.IsModItem())
                return true;

            __result = ModLibrary.Global.ModItems[enType].equipInfo.vanilla as WeaponInfo;

            return false;
        }

        private static bool OnLoadBatch(ref Dictionary<ushort, string> dis, WeaponAssets.WeaponContentManager __instance)
        {
            // This is more or less a copy of the original, except we redirect the ContentManger's RootDirectory and use shortened asset paths
            if (!__instance.enType.IsModItem())
                return true;


            ContentManager managerToUse = ModLibrary.Global.ModItems[__instance.enType].equipInfo.managerToUse;
            if (managerToUse != null)
                __instance.contWeaponContent.RootDirectory = managerToUse.RootDirectory;

            foreach (KeyValuePair<ushort, string> kvp in dis)
                __instance.ditxWeaponTextures.Add(kvp.Key, Utils.TryLoadTex(kvp.Value.Replace("Weapons/", ""), __instance.contWeaponContent));

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
            __result.txBase = Utils.TryLoadTex("Sprites/Heroes/" + sAttackPath + sAnimation + "/" + sDirection, VanillaContent);

            if (bWithShield && xPlayerView.xEquipment.DisplayShield != null && xPlayerView.xEquipment.DisplayShield.sResourceName != "")
            {
                ItemCodex.ItemTypes enType = xPlayerView.xEquipment.DisplayShield.enItemType;
                ContentManager managerToUse;
                string path;
                if (enType.IsModItem())
                {
                    managerToUse = ModLibrary.Global.ModItems[enType].equipInfo.managerToUse;
                    path = "Sprites/Heroes/" + sAttackPath + sAnimation + "/" + xPlayerView.xEquipment.DisplayShield.sResourceName + "/" + sDirection;
                }
                else
                {
                    managerToUse = VanillaContent;
                    path = "Sprites/Heroes/" + sAttackPath + sAnimation + "/Shields/" + xPlayerView.xEquipment.DisplayShield.sResourceName + "/" + sDirection;
                }
                __result.txShield = Utils.TryLoadTex(path, managerToUse);
            }

            if (bWithWeapon)
                __result.txWeapon = RenderMaster.txNullTex; // Dunno why this is a thing

            return false; // Never executes the original
        }
    }
}
