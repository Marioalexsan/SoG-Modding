using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace SoG.Modding
{
    internal static partial class Patches
    {
        private static bool OnGetItemDescription(ref ItemDescription __result, ItemCodex.ItemTypes enType)
        {
            if (!enType.IsModItem())
                return true;

            ModItem details = ModLibrary.GlobalLib.Items[enType].itemInfo;
            __result = details.vanilla;
            __result.txDisplayImage = Utils.TryLoadTex(details.resourcePath, details.managerToUse);

            return false;
        }

        private static bool OnGetItemInstance(ref Item __result, ItemCodex.ItemTypes enType)
        {
            if (!enType.IsModItem())
                return true;

            ModItem details = ModLibrary.GlobalLib.Items[enType].itemInfo;
            string trueShadowTex = details.shadowPath != "" ? details.shadowPath : "Items/DropAppearance/hartass02";
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

            __result = ModLibrary.GlobalLib.Items[enType].equipInfo.vanilla;

            return false;
        }

        /// <summary>
        /// For modded facegear, allows retrieving FacegearInfo from GrindScript's internals.
        /// </summary>

        private static bool OnGetFacegearInfo(ref FacegearInfo __result, ItemCodex.ItemTypes enType)
        {
            if (!enType.IsModItem())
                return true;

            ModEquip modEquip = ModLibrary.GlobalLib.Items[enType].equipInfo;
            ContentManager manager = modEquip.managerToUse;
            string path = modEquip.resourcePath;

            __result = modEquip.vanilla as FacegearInfo;
            __result.atxTextures[0] = Utils.TryLoadTex(Path.Combine(path, "Up"), manager);
            __result.atxTextures[1] = Utils.TryLoadTex(Path.Combine(path, "Right"), manager);
            __result.atxTextures[2] = Utils.TryLoadTex(Path.Combine(path, "Down"), manager);
            __result.atxTextures[3] = Utils.TryLoadTex(Path.Combine(path, "Left"), manager);

            return false;
        }

        /// <summary>
        /// For modded hats, allows retrieving HatInfo from GrindScript's internals.
        /// </summary>

        private static bool OnGetHatInfo(ref HatInfo __result, ItemCodex.ItemTypes enType)
        {
            if (!enType.IsModItem())
                return true;

            ModEquip modEquip = ModLibrary.GlobalLib.Items[enType].equipInfo;
            ContentManager manager = modEquip.managerToUse;
            string path = modEquip.resourcePath;

            __result = modEquip.vanilla as HatInfo;
            __result.xDefaultSet.atxTextures[0] = Utils.TryLoadTex(Path.Combine(path, "Up"), manager);
            __result.xDefaultSet.atxTextures[1] = Utils.TryLoadTex(Path.Combine(path, "Right"), manager);
            __result.xDefaultSet.atxTextures[2] = Utils.TryLoadTex(Path.Combine(path, "Down"), manager);
            __result.xDefaultSet.atxTextures[3] = Utils.TryLoadTex(Path.Combine(path, "Left"), manager);
            foreach (var kvp in __result.denxAlternateVisualSets)
            {
                string altPath = Path.Combine(path, modEquip.hatAltSetResources[kvp.Key]);
                kvp.Value.atxTextures[0] = Utils.TryLoadTex(Path.Combine(altPath, "Up"), manager);
                kvp.Value.atxTextures[1] = Utils.TryLoadTex(Path.Combine(altPath, "Right"), manager);
                kvp.Value.atxTextures[2] = Utils.TryLoadTex(Path.Combine(altPath, "Down"), manager);
                kvp.Value.atxTextures[3] = Utils.TryLoadTex(Path.Combine(altPath, "Left"), manager);
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

            __result = ModLibrary.GlobalLib.Items[enType].equipInfo.vanilla as WeaponInfo;

            return false;
        }

        /// <summary>
        /// Patches <see cref="WeaponAssets.WeaponContentManager.LoadBatch(Dictionary{ushort, string})"/>.
        /// Allows use of modded assets. Modded weapons have shorter asset paths ("/Weapons" is removed).
        /// </summary>

        private static bool OnLoadBatch(ref Dictionary<ushort, string> dis, WeaponAssets.WeaponContentManager __instance)
        {
            ItemCodex.ItemTypes type = __instance.enType;

            if (!type.IsModItem())
                return true;

            ModEquip equipInfo = ModLibrary.GlobalLib.Items[type].equipInfo;
            ContentManager manager = equipInfo.managerToUse;
            bool oneHanded = (equipInfo.vanilla as WeaponInfo).enWeaponCategory == WeaponInfo.WeaponCategory.OneHanded;

            if (manager != null)
                __instance.contWeaponContent.RootDirectory = manager.RootDirectory;

            foreach (KeyValuePair<ushort, string> kvp in dis)
            {
                string resourcePath = ModLibrary.GlobalLib.Items[type].equipInfo.resourcePath;

                string texPath = kvp.Value;
                texPath = texPath.Replace($"Weapons/{resourcePath}/", "");
                if (oneHanded)
                {
                    texPath = texPath.Replace("Sprites/Heroes/OneHanded/", resourcePath + "/");
                    texPath = texPath.Replace("Sprites/Heroes/Charge/OneHand/", resourcePath + "/1HCharge/");
                }
                else
                {
                    texPath = texPath.Replace("Sprites/Heroes/TwoHanded/", resourcePath + "/");
                    texPath = texPath.Replace("Sprites/Heroes/Charge/TwoHand/", resourcePath + "/2HCharge/");
                }

                __instance.ditxWeaponTextures.Add(kvp.Key, Utils.TryLoadTex(texPath, __instance.contWeaponContent));
            }

            return false;
        }
    }
}
