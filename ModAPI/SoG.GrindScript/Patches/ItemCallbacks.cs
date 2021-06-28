using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoG.Modding
{
    internal static partial class Patches
    {
        private static bool OnGetItemDescription(ref ItemDescription __result, ItemCodex.ItemTypes enType)
        {
            if (!enType.IsModItem())
                return true;

            ModItem details = ModLibrary.Global.Items[enType].itemInfo;
            __result = details.vanilla;
            __result.txDisplayImage = Utils.TryLoadTex(details.resourcePath, details.managerToUse);

            return false;
        }

        private static bool OnGetItemInstance(ref Item __result, ItemCodex.ItemTypes enType)
        {
            if (!enType.IsModItem())
                return true;

            ModItem details = ModLibrary.Global.Items[enType].itemInfo;
            string trueShadowTex = details.shadowPath != "" ? details.shadowPath : BaseScript.SoGPath + "Items/DropAppearance/hartass02";
            ItemDescription xDesc = details.vanilla;

            __result = new Item()
            {
                enType = enType,
                sFullName = xDesc.sFullName,
                bGiveToServer = xDesc.lenCategory.Contains(ItemCodex.ItemCategories.GrantToServer)
            };

            __result.xRenderComponent.txTexture = Utils.TryLoadTex(details.resourcePath, details.managerToUse);
            __result.xRenderComponent.txShadowTexture = Utils.TryLoadTex(trueShadowTex, details.managerToUse);
            __result.xCollisionComponent.xMovementCollider = new SphereCollider(10f, Vector2.Zero, __result.xTransform, 1f, __result) { bCollideWithFlat = true };

            return false;
        }

        /// <summary>
        /// For modded equipment, allows retrieving EquipmentInfo from GrindScript's internals.
        /// </summary>

        private static bool OnGetEquipmentInfo(ref EquipmentInfo __result, ItemCodex.ItemTypes enType)
        {
            if (!enType.IsModItem())
                return true;

            __result = ModLibrary.Global.Items[enType].equipInfo.vanilla;

            return false;
        }

        /// <summary>
        /// For modded facegear, allows retrieving FacegearInfo from GrindScript's internals.
        /// </summary>

        private static bool OnGetFacegearInfo(ref FacegearInfo __result, ItemCodex.ItemTypes enType)
        {
            if (!enType.IsModItem())
                return true;

            ModEquip modEquip = ModLibrary.Global.Items[enType].equipInfo;
            ContentManager manager = modEquip.managerToUse;

            __result = modEquip.vanilla as FacegearInfo;
            __result.atxTextures[0] = Utils.TryLoadTex(modEquip.resourcePath + "/Up", manager);
            __result.atxTextures[1] = Utils.TryLoadTex(modEquip.resourcePath + "/Right", manager);
            __result.atxTextures[2] = Utils.TryLoadTex(modEquip.resourcePath + "/Down", manager);
            __result.atxTextures[3] = Utils.TryLoadTex(modEquip.resourcePath + "/Left", manager);

            return false;
        }

        /// <summary>
        /// For modded hats, allows retrieving HatInfo from GrindScript's internals.
        /// </summary>

        private static bool OnGetHatInfo(ref HatInfo __result, ItemCodex.ItemTypes enType)
        {
            if (!enType.IsModItem())
                return true;

            ModEquip modEquip = ModLibrary.Global.Items[enType].equipInfo;
            ContentManager manager = modEquip.managerToUse;

            __result = modEquip.vanilla as HatInfo;
            __result.xDefaultSet.atxTextures[0] = Utils.TryLoadTex(modEquip.resourcePath + "/Up", manager);
            __result.xDefaultSet.atxTextures[1] = Utils.TryLoadTex(modEquip.resourcePath + "/Right", manager);
            __result.xDefaultSet.atxTextures[2] = Utils.TryLoadTex(modEquip.resourcePath + "/Down", manager);
            __result.xDefaultSet.atxTextures[3] = Utils.TryLoadTex(modEquip.resourcePath + "/Left", manager);
            foreach (var kvp in __result.denxAlternateVisualSets)
            {
                string altPath = modEquip.resourcePath + "/" + modEquip.hatAltSetResources[kvp.Key] + "/";
                kvp.Value.atxTextures[0] = Utils.TryLoadTex(altPath + "/Up", manager);
                kvp.Value.atxTextures[1] = Utils.TryLoadTex(altPath + "/Right", manager);
                kvp.Value.atxTextures[2] = Utils.TryLoadTex(altPath + "/Down", manager);
                kvp.Value.atxTextures[3] = Utils.TryLoadTex(altPath + "/Left", manager);
            }

            return false;
        }

        /// <summary>
        /// For modded weapons, allows retrieving WeaponInfo from GrindScript's internals.
        /// </summary>

        private static bool OnGetWeaponInfo(ref WeaponInfo __result, ItemCodex.ItemTypes enType)
        {
            if (!enType.IsModItem())
                return true;

            __result = ModLibrary.Global.Items[enType].equipInfo.vanilla as WeaponInfo;

            return false;
        }

        /// <summary>
        /// Patches <see cref="WeaponAssets.WeaponContentManager.LoadBatch(Dictionary{ushort, string})"/>.
        /// Allows use of modded assets. Modded weapons have shorter asset paths ("/Weapons" is removed).
        /// </summary>

        private static bool OnLoadBatch(ref Dictionary<ushort, string> dis, WeaponAssets.WeaponContentManager __instance)
        {
            if (!__instance.enType.IsModItem())
                return true;

            ContentManager managerToUse = ModLibrary.Global.Items[__instance.enType].equipInfo.managerToUse;
            if (managerToUse != null)
                __instance.contWeaponContent.RootDirectory = managerToUse.RootDirectory;

            foreach (KeyValuePair<ushort, string> kvp in dis)
            {
                int start = kvp.Value.IndexOf('<') + 1;
                int end = kvp.Value.IndexOf('>');
                string resourcePath = kvp.Value.Substring(start, end - start);

                string texPath = kvp.Value;
                texPath = texPath.Replace($"Weapons/<{resourcePath}>/", "");
                texPath = texPath.Replace("Sprites/Heroes/OneHanded/", resourcePath + "/");
                texPath = texPath.Replace("Sprites/Heroes/Charge/OneHand/", resourcePath + "/1HCharge/");
                texPath = texPath.Replace("Sprites/Heroes/TwoHanded/", resourcePath + "/");
                texPath = texPath.Replace("Sprites/Heroes/Charge/TwoHand/", resourcePath + "/2HCharge/");

                __instance.ditxWeaponTextures.Add(kvp.Key, Utils.TryLoadTex(texPath, __instance.contWeaponContent));
            }

            return false;
        }
    }
}
