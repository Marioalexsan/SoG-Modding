using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using WeaponAssets;
using SoG.Modding.Patches;
using SoG.Modding.Extensions;
using HarmonyLib;

namespace SoG.Modding.Core
{
    /// <summary>
    /// Factory class used for retrieving Harmony patches via an enum.
    /// </summary>

    public static class PatchCodex
    {
        /// <summary>
        /// Identifies a specific game patch.
        /// </summary>
        public enum PatchID
        {
            // GrindScript Initialize
            Game1_Initialize, 

            // Mod Content Init / Loading / Whatever
            Game1_StartupThreadExecute,

            // BaseScript callbacks
            Game1_FinalDraw,
            Game1_Player_TakeDamage,
            Game1_Player_KillPlayer,
            Game1_Player_ApplyLvUpBonus,
            Game1_Enemy_TakeDamage,
            Game1_NPC_TakeDamage,
            Game1_NPC_Interact,
            Game1_LevelLoading_DoStuff_Arcadia,
            Game1_Item_Use,

            // RogueLike
            Game1_RogueLike_GetPerkTexture,
            Game1_RogueLike_GetTreatCurseTexture,
            Game1_RogueLike_GetTreatCurseInfo,
            TreatCurseMenu_FillTreatList,
            TreatCurseMenu_FillCurseList,
            Game1_RogueLike_ActivatePerks,
            Game1_LevelLoading_DoStuff_ArcadeModeRoom,
            PerkInfo_Init,

            // Item API callbacks
            Game1_Chat_ParseCommand,
            ItemCodex_GetItemDescription,
            ItemCodex_GetItemInstance,
            EquipmentCodex_GetArmorInfo,
            EquipmentCodex_GetAccessoryInfo,
            EquipmentCodex_GetShieldInfo,
            EquipmentCodex_GetShoesInfo,
            HatCodex_GetHatInfo,
            FacegearCodex_GetHatInfo,
            WeaponCodex_GetWeaponInfo,
            WeaponContentManager_LoadBatch,
            Game1_Animations_GetAnimationSet,

            // SoundSystem patches
            SoundSystem_PlayInterfaceCue,
            SoundSystem_PlayTrackableInterfaceCue,
            SoundSystem_PlayCue1,
            SoundSystem_PlayCue2,
            SoundSystem_PlayCue3,
            SoundSystem_PlayCue4,
            SoundSystem_ReadySongInCue,
            SoundSystem_PlaySong,
            SoundSystem_PlayMixCues,
            SoundSystem_ChangeSongRegionIfNecessary,

            // Saving patches
            Game1_Saving_SaveCharacterToFile,
            Game1_Saving_SaveWorldToFile,
            Game1_Saving_SaveRogueToFile,
            Game1_Loading_LoadCharacterFromFile,
            Game1_Loading_LoadWorldFromFile,
            Game1_Loading_LoadRogueFile,

            // Level patches
            Game1_LevelLoading_DoStuff,
            LevelBlueprint_GetBlueprint,

            // Shop Menu Crap patches
            Game1_ShopMenu_Render_TreatCurseAssign
        }

        /// <summary>
        /// Describes a Harmony patch as a collection of MethodInfo.
        /// </summary>
        public class PatchInfo
        {
            public MethodInfo Target;
            public MethodInfo Prefix;
            public MethodInfo Transpiler;
            public MethodInfo Postfix;

            public PatchInfo() { }

            public PatchInfo(MethodInfo target)
            {
                Target = target;
            }
        }

        private static readonly Dictionary<PatchID, PatchInfo> _patchCodex;

        /// <summary>
        /// Get a patch based on its PatchID. Returns null if no such patch is defined.
        /// </summary>
        public static PatchInfo GetPatch(PatchID which)
        {
            return _patchCodex.TryGetValue(which, out var patch) ? patch : null;
        }

        static PatchCodex()
        {
            // Commonly used types
            var patches = typeof(PatchCollection).GetTypeInfo();
            var game1 = typeof(Game1).GetTypeInfo();
            var soundSystem = typeof(SoundSystem).GetTypeInfo();
            var weaponContentManager = typeof(WeaponContentManager).GetTypeInfo();
            var itemCodex = typeof(ItemCodex).GetTypeInfo();
            var equipmentCodex = typeof(EquipmentCodex).GetTypeInfo();
            var facegearCodex = typeof(FacegearCodex).GetTypeInfo();
            var hatCodex = typeof(HatCodex).GetTypeInfo();
            var weaponCodex = typeof(WeaponCodex).GetTypeInfo();
            var treatCurseMenu = typeof(ShopMenu.TreatCurseMenu).GetTypeInfo();

            _patchCodex = new Dictionary<PatchID, PatchInfo>()
            {
                [PatchID.Game1_Initialize] = new PatchInfo()
                {
                    Target = game1.GetPrivateMethod("Initialize"),
                    Prefix = patches.GetPrivateMethod("OnGame1Initialize")
                },
                [PatchID.Game1_StartupThreadExecute] = new PatchInfo()
                {
                    Target = game1.GetMethod("__StartupThreadExecute"),
                    Transpiler = patches.GetPrivateMethod("StartupTranspiler")
                },

                [PatchID.Game1_FinalDraw] = new PatchInfo()
                {
                    Target = game1.GetMethod("FinalDraw"),
                    Prefix = patches.GetPrivateMethod("OnFinalDraw")
                },
                [PatchID.Game1_Player_TakeDamage] = new PatchInfo()
                {
                    Target = game1.GetMethod("_Player_TakeDamage"),
                    Prefix = patches.GetPrivateMethod("OnPlayerTakeDamage")
                },
                [PatchID.Game1_Player_KillPlayer] = new PatchInfo()
                {
                    Target = game1.GetMethod("_Player_KillPlayer", new Type[] { typeof(PlayerView), typeof(bool), typeof(bool) }),
                    Prefix = patches.GetPrivateMethod("OnPlayerKilled")
                },
                [PatchID.Game1_Player_ApplyLvUpBonus] = new PatchInfo()
                {
                    Target = game1.GetMethod("_Player_ApplyLvUpBonus"),
                    Postfix = patches.GetPrivateMethod("PostPlayerLevelUp")
                },
                [PatchID.Game1_Enemy_TakeDamage] = new PatchInfo()
                {
                    Target = game1.GetMethod("_Enemy_TakeDamage"),
                    Prefix = patches.GetPrivateMethod("OnEnemyTakeDamage")
                },
                [PatchID.Game1_NPC_TakeDamage] = new PatchInfo()
                {
                    Target = game1.GetMethod("_NPC_TakeDamage"),
                    Prefix = patches.GetPrivateMethod("OnNPCTakeDamage")
                },
                [PatchID.Game1_NPC_Interact] = new PatchInfo()
                {
                    Target = game1.GetMethod("_NPC_Interact"),
                    Prefix = patches.GetPrivateMethod("OnNPCInteraction")
                },
                [PatchID.Game1_LevelLoading_DoStuff_Arcadia] = new PatchInfo()
                {
                    Target = game1.GetMethod("_LevelLoading_DoStuff_Arcadia"),
                    Prefix = patches.GetPrivateMethod("OnArcadiaLoad")
                },
                [PatchID.Game1_Item_Use] = new PatchInfo()
                {
                    Target = game1.GetDeclaredMethods("_Item_Use").ElementAt(1),
                    Prefix = patches.GetPrivateMethod("OnItemUse")
                },


                [PatchID.Game1_RogueLike_GetPerkTexture] = new PatchInfo()
                {
                    Target = game1.GetMethod("_RogueLike_GetPerkTexture"),
                    Prefix = patches.GetPrivateMethod("OnGetPerkTexture")
                },
                [PatchID.Game1_RogueLike_GetTreatCurseTexture] = new PatchInfo()
                {
                    Target = game1.GetMethod("_RogueLike_GetTreatCurseTexture"),
                    Prefix = patches.GetPrivateMethod("OnGetTreatCurseTexture")
                },
                [PatchID.Game1_RogueLike_GetTreatCurseInfo] = new PatchInfo()
                {
                    Target = game1.GetMethod("_RogueLike_GetTreatCurseInfo"),
                    Prefix = patches.GetPrivateMethod("OnGetTreatCurseInfo")
                },
                [PatchID.TreatCurseMenu_FillCurseList] = new PatchInfo()
                {
                    Target = treatCurseMenu.GetMethod("FillCurseList"),
                    Postfix = patches.GetPrivateMethod("PostFillCurseList")
                },
                [PatchID.TreatCurseMenu_FillTreatList] = new PatchInfo()
                {
                    Target = treatCurseMenu.GetMethod("FillTreatList"),
                    Postfix = patches.GetPrivateMethod("PostFillTreatList")
                },
                [PatchID.Game1_RogueLike_ActivatePerks] = new PatchInfo()
                {
                    Target = game1.GetMethod("_RogueLike_ActivatePerks"),
                    Postfix = patches.GetPrivateMethod("PostPerkActivation")
                },
                [PatchID.Game1_LevelLoading_DoStuff_ArcadeModeRoom] = new PatchInfo()
                {
                    Target = game1.GetMethod("_LevelLoading_DoStuff_ArcadeModeRoom"),
                    Postfix = patches.GetPrivateMethod("PostArcadeRoomStart")
                },
                [PatchID.PerkInfo_Init] = new PatchInfo()
                {
                    Target = typeof(RogueLikeMode.PerkInfo).GetMethod("Init"),
                    Postfix = patches.GetPrivateMethod("PostPerkListInit")
                },


                [PatchID.Game1_Chat_ParseCommand] = new PatchInfo()
                {
                    Target = game1.GetMethod("_Chat_ParseCommand"),
                    Transpiler = patches.GetPrivateMethod("CommandTranspiler")
                },
                [PatchID.ItemCodex_GetItemDescription] = new PatchInfo()
                {
                    Target = itemCodex.GetMethod("GetItemDescription"),
                    Prefix = patches.GetPrivateMethod("OnGetItemDescription")
                },
                [PatchID.ItemCodex_GetItemInstance] = new PatchInfo()
                {
                    Target = itemCodex.GetMethod("GetItemInstance"),
                    Prefix = patches.GetPrivateMethod("OnGetItemInstance")
                },
                [PatchID.EquipmentCodex_GetArmorInfo] = new PatchInfo()
                {
                    Target = equipmentCodex.GetMethod("GetArmorInfo"),
                    Prefix = patches.GetPrivateMethod("OnGetEquipmentInfo")
                },
                [PatchID.EquipmentCodex_GetAccessoryInfo] = new PatchInfo()
                {
                    Target = equipmentCodex.GetMethod("GetAccessoryInfo"),
                    Prefix = patches.GetPrivateMethod("OnGetEquipmentInfo")
                },
                [PatchID.EquipmentCodex_GetShieldInfo] = new PatchInfo()
                {
                    Target = equipmentCodex.GetMethod("GetShieldInfo"),
                    Prefix = patches.GetPrivateMethod("OnGetEquipmentInfo")
                },
                [PatchID.EquipmentCodex_GetShoesInfo] = new PatchInfo()
                {
                    Target = equipmentCodex.GetMethod("GetShoesInfo"),
                    Prefix = patches.GetPrivateMethod("OnGetEquipmentInfo")
                },
                [PatchID.FacegearCodex_GetHatInfo] = new PatchInfo()
                {
                    Target = facegearCodex.GetMethod("GetHatInfo"),
                    Prefix = patches.GetPrivateMethod("OnGetFacegearInfo")
                },
                [PatchID.HatCodex_GetHatInfo] = new PatchInfo()
                {
                    Target = hatCodex.GetMethod("GetHatInfo"),
                    Prefix = patches.GetPrivateMethod("OnGetHatInfo")
                },
                [PatchID.WeaponCodex_GetWeaponInfo] = new PatchInfo()
                {
                    Target = weaponCodex.GetMethod("GetWeaponInfo"),
                    Prefix = patches.GetPrivateMethod("OnGetWeaponInfo")
                },
                [PatchID.WeaponContentManager_LoadBatch] = new PatchInfo()
                {
                    Target = weaponContentManager.GetDeclaredMethods("LoadBatch").ElementAt(1),
                    Prefix = patches.GetPrivateMethod("OnLoadBatch")
                },
                [PatchID.Game1_Animations_GetAnimationSet] = new PatchInfo()
                {
                    Target = game1.GetDeclaredMethods("_Animations_GetAnimationSet").ElementAt(1),
                    Prefix = patches.GetPrivateMethod("OnGetAnimationSet")
                },
                [PatchID.SoundSystem_PlayInterfaceCue] = new PatchInfo()
                {
                    Target = soundSystem.GetMethod("PlayInterfaceCue"),
                    Transpiler = patches.GetPrivateMethod("PlayEffectTranspiler")
                },
                [PatchID.SoundSystem_PlayTrackableInterfaceCue] = new PatchInfo()
                {
                    Target = soundSystem.GetMethod("PlayTrackableInterfaceCue"),
                    Transpiler = patches.GetPrivateMethod("GetEffectTranspiler")
                },
                [PatchID.SoundSystem_PlayCue1] = new PatchInfo()
                {
                    Target = soundSystem.GetDeclaredMethods("PlayCue").ElementAt(0),
                    Transpiler = patches.GetPrivateMethod("GetEffectTranspiler")
                },
                [PatchID.SoundSystem_PlayCue2] = new PatchInfo()
                {
                    Target = soundSystem.GetDeclaredMethods("PlayCue").ElementAt(1),
                    Transpiler = patches.GetPrivateMethod("GetEffectTranspiler")
                },
                [PatchID.SoundSystem_PlayCue3] = new PatchInfo()
                {
                    Target = soundSystem.GetDeclaredMethods("PlayCue").ElementAt(2),
                    Transpiler = patches.GetPrivateMethod("GetEffectTranspiler")
                },
                [PatchID.SoundSystem_PlayCue4] = new PatchInfo()
                {
                    Target = soundSystem.GetDeclaredMethods("PlayCue").ElementAt(3),
                    Transpiler = patches.GetPrivateMethod("GetEffectTranspiler")
                },
                [PatchID.SoundSystem_ReadySongInCue] = new PatchInfo()
                {
                    Target = soundSystem.GetMethod("ReadySongInCue"),
                    Transpiler = patches.GetPrivateMethod("GetMusicTranspiler")
                },
                [PatchID.SoundSystem_PlaySong] = new PatchInfo()
                {
                    Target = soundSystem.GetMethod("PlaySong"),
                    Prefix = patches.GetPrivateMethod("OnPlaySong"),
                    Transpiler = patches.GetPrivateMethod("GetMusicTranspiler"),
                },
                [PatchID.SoundSystem_PlayMixCues] = new PatchInfo()
                {
                    Target = soundSystem.GetMethod("PlayMixCues"),
                    Transpiler = patches.GetPrivateMethod("PlayMixTranspiler")
                },
                [PatchID.SoundSystem_ChangeSongRegionIfNecessary] = new PatchInfo()
                {
                    Target = soundSystem.GetMethod("ChangeSongRegionIfNecessary"),
                    Prefix = patches.GetPrivateMethod("OnChangeSongRegionIfNecessary")
                },
                [PatchID.Game1_Saving_SaveCharacterToFile] = new PatchInfo()
                {
                    Target = game1.GetMethod("_Saving_SaveCharacterToFile"),
                    Postfix = patches.GetPrivateMethod("PostCharacterSave")
                },
                [PatchID.Game1_Saving_SaveWorldToFile] = new PatchInfo()
                {
                    Target = game1.GetMethod("_Saving_SaveWorldToFile"),
                    Postfix = patches.GetPrivateMethod("PostWorldSave")
                },
                [PatchID.Game1_Saving_SaveRogueToFile] = new PatchInfo()
                {
                    Target = game1.GetDeclaredMethods("_Saving_SaveRogueToFile").Where(item => item.GetParameters().Count() == 1).First(),
                    Postfix = patches.GetPrivateMethod("PostArcadeSave")
                },
                [PatchID.Game1_Loading_LoadCharacterFromFile] = new PatchInfo()
                {
                    Target = game1.GetMethod("_Loading_LoadCharacterFromFile"),
                    Postfix = patches.GetPrivateMethod("PostCharacterLoad")
                },
                [PatchID.Game1_Loading_LoadWorldFromFile] = new PatchInfo()
                {
                    Target = game1.GetMethod("_Loading_LoadWorldFromFile"),
                    Postfix = patches.GetPrivateMethod("PostWorldLoad")
                },
                [PatchID.Game1_Loading_LoadRogueFile] = new PatchInfo()
                {
                    Target = game1.GetMethod("_Loading_LoadRogueFile"),
                    Postfix = patches.GetPrivateMethod("PostArcadeLoad")
                },
                [PatchID.Game1_LevelLoading_DoStuff] = new PatchInfo()
                {
                    Target = game1.GetMethod("_LevelLoading_DoStuff"),
                    Transpiler = patches.GetPrivateMethod("LevelDoStuffTranspiler")
                },
                [PatchID.LevelBlueprint_GetBlueprint] = new PatchInfo()
                {
                    Target = typeof(LevelBlueprint).GetMethod("GetBlueprint"),
                    Prefix = patches.GetPrivateMethod("OnGetLevelBlueprint")
                },

                [PatchID.Game1_ShopMenu_Render_TreatCurseAssign] = new PatchInfo()
                {
                    Target = game1.GetMethod("_ShopMenu_Render_TreatCurseAssign"),
                    Transpiler = patches.GetPrivateMethod("RenderTreatCurseAssignTranspiler")
                },
            };
        }
    }
}
