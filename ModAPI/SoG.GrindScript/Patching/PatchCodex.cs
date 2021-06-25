using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using WeaponAssets;

namespace SoG.Modding
{
    /// <summary>
    /// Factory class used for retrieving Harmony patches via an enum.
    /// </summary>

    public static class PatchCodex
    {
        public enum Patches
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
            Game1_Chat_ParseCommand,
            Game1_Item_Use,

            // Item API callbacks
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
            Game1_Loading_LoadRogueFile
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
        }

        private static readonly Dictionary<Patches, PatchInfo> _patchCodex;

        public static PatchInfo GetPatch(Patches which)
        {
            return _patchCodex.TryGetValue(which, out var patch) ? patch : null;
        }

        static PatchCodex()
        {
            Type Transpilers = typeof(Transpilers); // Commonly used
            Type Callbacks = typeof(Callbacks); // Commonly used
            TypeInfo Game1 = GrindScript.GetGameType("SoG.Game1"); // Commonly used
            TypeInfo SoundSystem = GrindScript.GetGameType("SoG.SoundSystem"); // SoundSystem patching

            _patchCodex = new Dictionary<Patches, PatchInfo>()
            {
                [Patches.Game1_Initialize] = new PatchInfo()
                {
                    Target = Game1.GetPrivateMethod("Initialize"), 
                    Prefix = Callbacks.GetPrivateMethod("OnGame1Initialize")
                },
                [Patches.Game1_StartupThreadExecute] = new PatchInfo()
                {
                    Target = Game1.GetMethod("__StartupThreadExecute"),
                    Transpiler = Transpilers.GetPrivateMethod("StartupTranspiler")
                },
                [Patches.Game1_FinalDraw] = new PatchInfo()
                {
                    Target = Game1.GetMethod("FinalDraw"),
                    Prefix = Callbacks.GetPrivateMethod("OnFinalDrawPrefix")
                },
                [Patches.Game1_Player_TakeDamage] = new PatchInfo()
                {
                    Target = Game1.GetMethod("_Player_TakeDamage"),
                    Prefix = Callbacks.GetPrivateMethod("OnPlayerTakeDamagePrefix")
                },
                [Patches.Game1_Player_KillPlayer] = new PatchInfo()
                {
                    Target = Game1.GetMethod("_Player_KillPlayer", new Type[] { typeof(PlayerView), typeof(bool), typeof(bool) }),
                    Prefix = Callbacks.GetPrivateMethod("OnPlayerKilledPrefix")
                },
                [Patches.Game1_Player_ApplyLvUpBonus] = new PatchInfo()
                {
                    Target = Game1.GetMethod("_Player_ApplyLvUpBonus"),
                    Postfix = Callbacks.GetPrivateMethod("PostPlayerLevelUp")
                },
                [Patches.Game1_Enemy_TakeDamage] = new PatchInfo()
                {
                    Target = Game1.GetMethod("_Enemy_TakeDamage"),
                    Prefix = Callbacks.GetPrivateMethod("OnEnemyTakeDamagePrefix")
                },
                [Patches.Game1_NPC_TakeDamage] = new PatchInfo()
                {
                    Target = Game1.GetMethod("_NPC_TakeDamage"),
                    Prefix = Callbacks.GetPrivateMethod("OnNPCTakeDamagePrefix")
                },
                [Patches.Game1_NPC_Interact] = new PatchInfo()
                {
                    Target = Game1.GetMethod("_NPC_Interact"),
                    Prefix = Callbacks.GetPrivateMethod("OnNPCInteractionPrefix")
                },
                [Patches.Game1_LevelLoading_DoStuff_Arcadia] = new PatchInfo()
                {
                    Target = Game1.GetMethod("_LevelLoading_DoStuff_Arcadia"),
                    Prefix = Callbacks.GetPrivateMethod("OnArcadiaLoadPrefix")
                },
                [Patches.Game1_Chat_ParseCommand] = new PatchInfo()
                {
                    Target = Game1.GetMethod("_Chat_ParseCommand"),
                    Transpiler = Transpilers.GetPrivateMethod("CommandTranspiler")
                },
                [Patches.Game1_Item_Use] = new PatchInfo()
                {
                    Target = Game1.GetDeclaredMethods("_Item_Use").ElementAt(1),
                    Prefix = Callbacks.GetPrivateMethod("OnItemUsePrefix")
                },
                [Patches.ItemCodex_GetItemDescription] = new PatchInfo()
                {
                    Target = typeof(ItemCodex).GetMethod("GetItemDescription"),
                    Prefix = Callbacks.GetPrivateMethod("OnGetItemDescription")
                },
                [Patches.ItemCodex_GetItemInstance] = new PatchInfo()
                {
                    Target = typeof(ItemCodex).GetMethod("GetItemInstance"),
                    Prefix = Callbacks.GetPrivateMethod("OnGetItemInstance")
                },
                [Patches.EquipmentCodex_GetArmorInfo] = new PatchInfo()
                {
                    Target = typeof(EquipmentCodex).GetMethod("GetArmorInfo"),
                    Prefix = Callbacks.GetPrivateMethod("OnGetEquipmentInfo")
                },
                [Patches.EquipmentCodex_GetAccessoryInfo] = new PatchInfo()
                {
                    Target = typeof(EquipmentCodex).GetMethod("GetAccessoryInfo"),
                    Prefix = Callbacks.GetPrivateMethod("OnGetEquipmentInfo")
                },
                [Patches.EquipmentCodex_GetShieldInfo] = new PatchInfo()
                {
                    Target = typeof(EquipmentCodex).GetMethod("GetShieldInfo"),
                    Prefix = Callbacks.GetPrivateMethod("OnGetEquipmentInfo")
                },
                [Patches.EquipmentCodex_GetShoesInfo] = new PatchInfo()
                {
                    Target = typeof(EquipmentCodex).GetMethod("GetShoesInfo"),
                    Prefix = Callbacks.GetPrivateMethod("OnGetEquipmentInfo")
                },
                [Patches.FacegearCodex_GetHatInfo] = new PatchInfo()
                {
                    Target = typeof(FacegearCodex).GetMethod("GetHatInfo"),
                    Prefix = Callbacks.GetPrivateMethod("OnGetFacegearInfo")
                },
                [Patches.HatCodex_GetHatInfo] = new PatchInfo()
                {
                    Target = typeof(HatCodex).GetMethod("GetHatInfo"),
                    Prefix = Callbacks.GetPrivateMethod("OnGetHatInfo")
                },
                [Patches.WeaponCodex_GetWeaponInfo] = new PatchInfo()
                {
                    Target = typeof(WeaponCodex).GetMethod("GetWeaponInfo"),
                    Prefix = Callbacks.GetPrivateMethod("OnGetWeaponInfo")
                },
                [Patches.WeaponContentManager_LoadBatch] = new PatchInfo()
                {
                    Target = typeof(WeaponContentManager).GetMethods(BindingFlags.NonPublic | BindingFlags.Instance).Where(item => item.Name == "LoadBatch").ElementAt(1),
                    Prefix = Callbacks.GetPrivateMethod("OnLoadBatch")
                },
                [Patches.Game1_Animations_GetAnimationSet] = new PatchInfo()
                {
                    Target = Game1.GetMethods(BindingFlags.Public | BindingFlags.Instance).Where(item => item.Name == "_Animations_GetAnimationSet").ElementAt(1),
                    Prefix = Callbacks.GetPrivateMethod("OnGetAnimationSet")
                },
                [Patches.SoundSystem_PlayInterfaceCue] = new PatchInfo()
                {
                    Target = SoundSystem.GetMethod("PlayInterfaceCue"),
                    Transpiler = Transpilers.GetPrivateMethod("PlayEffectTranspiler")
                },
                [Patches.SoundSystem_PlayTrackableInterfaceCue] = new PatchInfo()
                {
                    Target = SoundSystem.GetMethod("PlayTrackableInterfaceCue"),
                    Transpiler = Transpilers.GetPrivateMethod("GetEffectTranspiler")
                },
                [Patches.SoundSystem_PlayCue1] = new PatchInfo()
                {
                    Target = SoundSystem.GetDeclaredMethods("PlayCue").ElementAt(0),
                    Transpiler = Transpilers.GetPrivateMethod("GetEffectTranspiler")
                },
                [Patches.SoundSystem_PlayCue2] = new PatchInfo()
                {
                    Target = SoundSystem.GetDeclaredMethods("PlayCue").ElementAt(1),
                    Transpiler = Transpilers.GetPrivateMethod("GetEffectTranspiler")
                },
                [Patches.SoundSystem_PlayCue3] = new PatchInfo()
                {
                    Target = SoundSystem.GetDeclaredMethods("PlayCue").ElementAt(2),
                    Transpiler = Transpilers.GetPrivateMethod("GetEffectTranspiler")
                },
                [Patches.SoundSystem_PlayCue4] = new PatchInfo()
                {
                    Target = SoundSystem.GetDeclaredMethods("PlayCue").ElementAt(3),
                    Transpiler = Transpilers.GetPrivateMethod("GetEffectTranspiler")
                },
                [Patches.SoundSystem_ReadySongInCue] = new PatchInfo()
                {
                    Target = SoundSystem.GetMethod("ReadySongInCue"),
                    Transpiler = Transpilers.GetPrivateMethod("GetMusicTranspiler")
                },
                [Patches.SoundSystem_PlaySong] = new PatchInfo()
                {
                    Target = SoundSystem.GetMethod("PlaySong"),
                    Prefix = Callbacks.GetPrivateMethod("OnPlaySong"),
                    Transpiler = Transpilers.GetPrivateMethod("GetMusicTranspiler"),
                },
                [Patches.SoundSystem_PlayMixCues] = new PatchInfo()
                {
                    Target = SoundSystem.GetMethod("PlayMixCues"),
                    Transpiler = Transpilers.GetPrivateMethod("PlayMixTranspiler")
                },
                [Patches.SoundSystem_ChangeSongRegionIfNecessary] = new PatchInfo()
                {
                    Target = SoundSystem.GetMethod("ChangeSongRegionIfNecessary"),
                    Prefix = Callbacks.GetPrivateMethod("OnChangeSongRegionIfNecessary")
                },
                [Patches.Game1_Saving_SaveCharacterToFile] = new PatchInfo()
                {
                    Target = Game1.GetMethod("_Saving_SaveCharacterToFile"),
                    Postfix = Callbacks.GetPrivateMethod("PostCharacterSave")
                },
                [Patches.Game1_Saving_SaveWorldToFile] = new PatchInfo()
                {
                    Target = Game1.GetMethod("_Saving_SaveWorldToFile"),
                    Postfix = Callbacks.GetPrivateMethod("PostWorldSave")
                },
                [Patches.Game1_Saving_SaveRogueToFile] = new PatchInfo()
                {
                    Target = Game1.GetMethods().Where(item => item.Name == "_Saving_SaveRogueToFile" && item.GetParameters().Count() == 1).First(),
                    Postfix = Callbacks.GetPrivateMethod("PostArcadeSave")
                },
                [Patches.Game1_Loading_LoadCharacterFromFile] = new PatchInfo()
                {
                    Target = Game1.GetMethod("_Loading_LoadCharacterFromFile"),
                    Postfix = Callbacks.GetPrivateMethod("PostCharacterLoad")
                },
                [Patches.Game1_Loading_LoadWorldFromFile] = new PatchInfo()
                {
                    Target = Game1.GetMethod("_Loading_LoadWorldFromFile"),
                    Postfix = Callbacks.GetPrivateMethod("PostWorldLoad")
                },
                [Patches.Game1_Loading_LoadRogueFile] = new PatchInfo()
                {
                    Target = Game1.GetMethod("_Loading_LoadRogueFile"),
                    Postfix = Callbacks.GetPrivateMethod("PostArcadeLoad")
                },
            };
        }
    }
}
