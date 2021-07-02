using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using WeaponAssets;
using SoG.Modding.Patches;
using SoG.Modding.Extensions;

namespace SoG.Modding.Core
{
    /// <summary>
    /// Factory class used for retrieving Harmony patches via an enum.
    /// </summary>

    public static class PatchCodex
    {
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

            // Game1 has OnEnemyKilled event

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
            LevelBlueprint_GetBlueprint
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

        public static PatchInfo GetPatch(PatchID which)
        {
            return _patchCodex.TryGetValue(which, out var patch) ? patch : null;
        }

        static PatchCodex()
        {
            TypeInfo Patches = typeof(PatchCollection).GetTypeInfo(); // Commonly used
            TypeInfo Game1 = typeof(Game1).GetTypeInfo(); // Commonly used
            TypeInfo SoundSystem = typeof(SoundSystem).GetTypeInfo(); // SoundSystem patching
            TypeInfo WeaponContentManager = typeof(WeaponContentManager).GetTypeInfo();
            TypeInfo ItemCodex = typeof(ItemCodex).GetTypeInfo();
            TypeInfo EquipmentCodex = typeof(EquipmentCodex).GetTypeInfo();
            TypeInfo FacegearCodex = typeof(FacegearCodex).GetTypeInfo();
            TypeInfo HatCodex = typeof(HatCodex).GetTypeInfo();
            TypeInfo WeaponCodex = typeof(WeaponCodex).GetTypeInfo();
            TypeInfo TreatCurseMenu = typeof(ShopMenu.TreatCurseMenu).GetTypeInfo();

            _patchCodex = new Dictionary<PatchID, PatchInfo>()
            {
                [PatchID.Game1_Initialize] = new PatchInfo()
                {
                    Target = Game1.GetPrivateMethod("Initialize"),
                    Prefix = Patches.GetPrivateMethod("OnGame1Initialize")
                },
                [PatchID.Game1_StartupThreadExecute] = new PatchInfo()
                {
                    Target = Game1.GetMethod("__StartupThreadExecute"),
                    Transpiler = Patches.GetPrivateMethod("StartupTranspiler")
                },


                [PatchID.Game1_FinalDraw] = new PatchInfo()
                {
                    Target = Game1.GetMethod("FinalDraw"),
                    Prefix = Patches.GetPrivateMethod("OnFinalDraw")
                },
                [PatchID.Game1_Player_TakeDamage] = new PatchInfo()
                {
                    Target = Game1.GetMethod("_Player_TakeDamage"),
                    Prefix = Patches.GetPrivateMethod("OnPlayerTakeDamage")
                },
                [PatchID.Game1_Player_KillPlayer] = new PatchInfo()
                {
                    Target = Game1.GetMethod("_Player_KillPlayer", new Type[] { typeof(PlayerView), typeof(bool), typeof(bool) }),
                    Prefix = Patches.GetPrivateMethod("OnPlayerKilled")
                },
                [PatchID.Game1_Player_ApplyLvUpBonus] = new PatchInfo()
                {
                    Target = Game1.GetMethod("_Player_ApplyLvUpBonus"),
                    Postfix = Patches.GetPrivateMethod("PostPlayerLevelUp")
                },
                [PatchID.Game1_Enemy_TakeDamage] = new PatchInfo()
                {
                    Target = Game1.GetMethod("_Enemy_TakeDamage"),
                    Prefix = Patches.GetPrivateMethod("OnEnemyTakeDamage")
                },
                [PatchID.Game1_NPC_TakeDamage] = new PatchInfo()
                {
                    Target = Game1.GetMethod("_NPC_TakeDamage"),
                    Prefix = Patches.GetPrivateMethod("OnNPCTakeDamage")
                },
                [PatchID.Game1_NPC_Interact] = new PatchInfo()
                {
                    Target = Game1.GetMethod("_NPC_Interact"),
                    Prefix = Patches.GetPrivateMethod("OnNPCInteraction")
                },
                [PatchID.Game1_LevelLoading_DoStuff_Arcadia] = new PatchInfo()
                {
                    Target = Game1.GetMethod("_LevelLoading_DoStuff_Arcadia"),
                    Prefix = Patches.GetPrivateMethod("OnArcadiaLoad")
                },
                [PatchID.Game1_Item_Use] = new PatchInfo()
                {
                    Target = Game1.GetDeclaredMethods("_Item_Use").ElementAt(1),
                    Prefix = Patches.GetPrivateMethod("OnItemUse")
                },


                [PatchID.Game1_RogueLike_GetPerkTexture] = new PatchInfo()
                {
                    Target = Game1.GetMethod("_RogueLike_GetPerkTexture"),
                    Prefix = Patches.GetPrivateMethod("OnGetPerkTexture")
                },
                [PatchID.Game1_RogueLike_GetTreatCurseTexture] = new PatchInfo()
                {
                    Target = Game1.GetMethod("_RogueLike_GetTreatCurseTexture"),
                    Prefix = Patches.GetPrivateMethod("OnGetTreatCurseTexture")
                },
                [PatchID.Game1_RogueLike_GetTreatCurseInfo] = new PatchInfo()
                {
                    Target = Game1.GetMethod("_RogueLike_GetTreatCurseInfo"),
                    Prefix = Patches.GetPrivateMethod("OnGetTreatCurseInfo")
                },
                [PatchID.TreatCurseMenu_FillCurseList] = new PatchInfo()
                {
                    Target = TreatCurseMenu.GetMethod("FillCurseList"),
                    Postfix = Patches.GetPrivateMethod("PostFillCurseList")
                },
                [PatchID.TreatCurseMenu_FillTreatList] = new PatchInfo()
                {
                    Target = TreatCurseMenu.GetMethod("FillTreatList"),
                    Postfix = Patches.GetPrivateMethod("PostFillTreatList")
                },
                [PatchID.Game1_RogueLike_ActivatePerks] = new PatchInfo()
                {
                    Target = Game1.GetMethod("_RogueLike_ActivatePerks"),
                    Postfix = Patches.GetPrivateMethod("PostPerkActivation")
                },
                [PatchID.Game1_LevelLoading_DoStuff_ArcadeModeRoom] = new PatchInfo()
                {
                    Target = Game1.GetMethod("_LevelLoading_DoStuff_ArcadeModeRoom"),
                    Postfix = Patches.GetPrivateMethod("PostArcadeRoomStart")
                },
                [PatchID.PerkInfo_Init] = new PatchInfo()
                {
                    Target = typeof(RogueLikeMode.PerkInfo).GetMethod("Init"),
                    Postfix = Patches.GetPrivateMethod("PostPerkListInit")
                },


                [PatchID.Game1_Chat_ParseCommand] = new PatchInfo()
                {
                    Target = Game1.GetMethod("_Chat_ParseCommand"),
                    Transpiler = Patches.GetPrivateMethod("CommandTranspiler")
                },
                [PatchID.ItemCodex_GetItemDescription] = new PatchInfo()
                {
                    Target = ItemCodex.GetMethod("GetItemDescription"),
                    Prefix = Patches.GetPrivateMethod("OnGetItemDescription")
                },
                [PatchID.ItemCodex_GetItemInstance] = new PatchInfo()
                {
                    Target = ItemCodex.GetMethod("GetItemInstance"),
                    Prefix = Patches.GetPrivateMethod("OnGetItemInstance")
                },
                [PatchID.EquipmentCodex_GetArmorInfo] = new PatchInfo()
                {
                    Target = EquipmentCodex.GetMethod("GetArmorInfo"),
                    Prefix = Patches.GetPrivateMethod("OnGetEquipmentInfo")
                },
                [PatchID.EquipmentCodex_GetAccessoryInfo] = new PatchInfo()
                {
                    Target = EquipmentCodex.GetMethod("GetAccessoryInfo"),
                    Prefix = Patches.GetPrivateMethod("OnGetEquipmentInfo")
                },
                [PatchID.EquipmentCodex_GetShieldInfo] = new PatchInfo()
                {
                    Target = EquipmentCodex.GetMethod("GetShieldInfo"),
                    Prefix = Patches.GetPrivateMethod("OnGetEquipmentInfo")
                },
                [PatchID.EquipmentCodex_GetShoesInfo] = new PatchInfo()
                {
                    Target = EquipmentCodex.GetMethod("GetShoesInfo"),
                    Prefix = Patches.GetPrivateMethod("OnGetEquipmentInfo")
                },
                [PatchID.FacegearCodex_GetHatInfo] = new PatchInfo()
                {
                    Target = FacegearCodex.GetMethod("GetHatInfo"),
                    Prefix = Patches.GetPrivateMethod("OnGetFacegearInfo")
                },
                [PatchID.HatCodex_GetHatInfo] = new PatchInfo()
                {
                    Target = HatCodex.GetMethod("GetHatInfo"),
                    Prefix = Patches.GetPrivateMethod("OnGetHatInfo")
                },
                [PatchID.WeaponCodex_GetWeaponInfo] = new PatchInfo()
                {
                    Target = WeaponCodex.GetMethod("GetWeaponInfo"),
                    Prefix = Patches.GetPrivateMethod("OnGetWeaponInfo")
                },
                [PatchID.WeaponContentManager_LoadBatch] = new PatchInfo()
                {
                    Target = WeaponContentManager.GetDeclaredMethods("LoadBatch").ElementAt(1),
                    Prefix = Patches.GetPrivateMethod("OnLoadBatch")
                },
                [PatchID.Game1_Animations_GetAnimationSet] = new PatchInfo()
                {
                    Target = Game1.GetDeclaredMethods("_Animations_GetAnimationSet").ElementAt(1),
                    Prefix = Patches.GetPrivateMethod("OnGetAnimationSet")
                },
                [PatchID.SoundSystem_PlayInterfaceCue] = new PatchInfo()
                {
                    Target = SoundSystem.GetMethod("PlayInterfaceCue"),
                    Transpiler = Patches.GetPrivateMethod("PlayEffectTranspiler")
                },
                [PatchID.SoundSystem_PlayTrackableInterfaceCue] = new PatchInfo()
                {
                    Target = SoundSystem.GetMethod("PlayTrackableInterfaceCue"),
                    Transpiler = Patches.GetPrivateMethod("GetEffectTranspiler")
                },
                [PatchID.SoundSystem_PlayCue1] = new PatchInfo()
                {
                    Target = SoundSystem.GetDeclaredMethods("PlayCue").ElementAt(0),
                    Transpiler = Patches.GetPrivateMethod("GetEffectTranspiler")
                },
                [PatchID.SoundSystem_PlayCue2] = new PatchInfo()
                {
                    Target = SoundSystem.GetDeclaredMethods("PlayCue").ElementAt(1),
                    Transpiler = Patches.GetPrivateMethod("GetEffectTranspiler")
                },
                [PatchID.SoundSystem_PlayCue3] = new PatchInfo()
                {
                    Target = SoundSystem.GetDeclaredMethods("PlayCue").ElementAt(2),
                    Transpiler = Patches.GetPrivateMethod("GetEffectTranspiler")
                },
                [PatchID.SoundSystem_PlayCue4] = new PatchInfo()
                {
                    Target = SoundSystem.GetDeclaredMethods("PlayCue").ElementAt(3),
                    Transpiler = Patches.GetPrivateMethod("GetEffectTranspiler")
                },
                [PatchID.SoundSystem_ReadySongInCue] = new PatchInfo()
                {
                    Target = SoundSystem.GetMethod("ReadySongInCue"),
                    Transpiler = Patches.GetPrivateMethod("GetMusicTranspiler")
                },
                [PatchID.SoundSystem_PlaySong] = new PatchInfo()
                {
                    Target = SoundSystem.GetMethod("PlaySong"),
                    Prefix = Patches.GetPrivateMethod("OnPlaySong"),
                    Transpiler = Patches.GetPrivateMethod("GetMusicTranspiler"),
                },
                [PatchID.SoundSystem_PlayMixCues] = new PatchInfo()
                {
                    Target = SoundSystem.GetMethod("PlayMixCues"),
                    Transpiler = Patches.GetPrivateMethod("PlayMixTranspiler")
                },
                [PatchID.SoundSystem_ChangeSongRegionIfNecessary] = new PatchInfo()
                {
                    Target = SoundSystem.GetMethod("ChangeSongRegionIfNecessary"),
                    Prefix = Patches.GetPrivateMethod("OnChangeSongRegionIfNecessary")
                },
                [PatchID.Game1_Saving_SaveCharacterToFile] = new PatchInfo()
                {
                    Target = Game1.GetMethod("_Saving_SaveCharacterToFile"),
                    Postfix = Patches.GetPrivateMethod("PostCharacterSave")
                },
                [PatchID.Game1_Saving_SaveWorldToFile] = new PatchInfo()
                {
                    Target = Game1.GetMethod("_Saving_SaveWorldToFile"),
                    Postfix = Patches.GetPrivateMethod("PostWorldSave")
                },
                [PatchID.Game1_Saving_SaveRogueToFile] = new PatchInfo()
                {
                    Target = Game1.GetDeclaredMethods("_Saving_SaveRogueToFile").Where(item => item.GetParameters().Count() == 1).First(),
                    Postfix = Patches.GetPrivateMethod("PostArcadeSave")
                },
                [PatchID.Game1_Loading_LoadCharacterFromFile] = new PatchInfo()
                {
                    Target = Game1.GetMethod("_Loading_LoadCharacterFromFile"),
                    Postfix = Patches.GetPrivateMethod("PostCharacterLoad")
                },
                [PatchID.Game1_Loading_LoadWorldFromFile] = new PatchInfo()
                {
                    Target = Game1.GetMethod("_Loading_LoadWorldFromFile"),
                    Postfix = Patches.GetPrivateMethod("PostWorldLoad")
                },
                [PatchID.Game1_Loading_LoadRogueFile] = new PatchInfo()
                {
                    Target = Game1.GetMethod("_Loading_LoadRogueFile"),
                    Postfix = Patches.GetPrivateMethod("PostArcadeLoad")
                },
                [PatchID.Game1_LevelLoading_DoStuff] = new PatchInfo()
                {
                    Target = Game1.GetMethod("_LevelLoading_DoStuff"),
                    Transpiler = Patches.GetPrivateMethod("LevelDoStuffTranspiler")
                },
                [PatchID.LevelBlueprint_GetBlueprint] = new PatchInfo()
                {
                    Target = typeof(LevelBlueprint).GetMethod("GetBlueprint"),
                    Prefix = Patches.GetPrivateMethod("OnGetLevelBlueprint")
                },
            };
        }
    }
}
