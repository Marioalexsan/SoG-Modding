using System;
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

        public class PatchDescription
        {
            public MethodInfo Target;
            public MethodInfo Prefix;
            public MethodInfo Transpiler;
            public MethodInfo Postfix;
        }

        public static PatchDescription GetPatch(Patches which)
        {
            Type Transpilers = typeof(Transpilers); // Commonly used
            Type Callbacks = typeof(Callbacks); // Commonly used
            TypeInfo Game1 = GrindScript.GetGameType("SoG.Game1"); // Commonly used
            TypeInfo SoundSystem = GrindScript.GetGameType("SoG.SoundSystem"); // SoundSystem patching

            MethodInfo Target = null, Prefix = null, Postfix = null, Transpiler = null;
            switch (which)
            {
                case Patches.Game1_Initialize:
                    Target = Game1.GetPrivateInstanceMethod("Initialize");
                    Prefix = Callbacks.GetPrivateStaticMethod("OnGame1Initialize");
                    break;
                case Patches.Game1_StartupThreadExecute:
                    Target = Game1.GetMethod("__StartupThreadExecute");
                    Transpiler = Transpilers.GetPrivateStaticMethod("StartupTranspiler");
                    break;
                case Patches.Game1_FinalDraw:
                    Target = Game1.GetPublicInstanceMethod("FinalDraw");
                    Prefix = Callbacks.GetPrivateStaticMethod("OnFinalDrawPrefix");
                    break;
                case Patches.Game1_Player_TakeDamage:
                    Target = Game1.GetPublicInstanceMethod("_Player_TakeDamage");
                    Prefix = Callbacks.GetPrivateStaticMethod("OnPlayerTakeDamagePrefix");
                    break;
                case Patches.Game1_Player_KillPlayer:
                    Target = Game1.GetMethod("_Player_KillPlayer", new Type[] { typeof(PlayerView), typeof(bool), typeof(bool) });
                    Prefix = Callbacks.GetPrivateStaticMethod("OnPlayerKilledPrefix");
                    break;
                case Patches.Game1_Player_ApplyLvUpBonus:
                    Target = Game1.GetPublicInstanceMethod("_Player_ApplyLvUpBonus");
                    Postfix = Callbacks.GetPrivateStaticMethod("PostPlayerLevelUp");
                    break;
                case Patches.Game1_Enemy_TakeDamage:
                    Target = Game1.GetPublicInstanceMethod("_Enemy_TakeDamage");
                    Prefix = Callbacks.GetPrivateStaticMethod("OnEnemyTakeDamagePrefix");
                    break;
                case Patches.Game1_NPC_TakeDamage:
                    Target = Game1.GetPublicInstanceMethod("_NPC_TakeDamage");
                    Prefix = Callbacks.GetPrivateStaticMethod("OnNPCTakeDamagePrefix");
                    break;
                case Patches.Game1_NPC_Interact:
                    Target = Game1.GetPublicInstanceMethod("_NPC_Interact");
                    Prefix = Callbacks.GetPrivateStaticMethod("OnNPCInteractionPrefix");
                    break;
                case Patches.Game1_LevelLoading_DoStuff_Arcadia:
                    Target = Game1.GetPublicInstanceMethod("_LevelLoading_DoStuff_Arcadia");
                    Prefix = Callbacks.GetPrivateStaticMethod("OnArcadiaLoadPrefix");
                    break;
                case Patches.Game1_Chat_ParseCommand:
                    Target = Game1.GetPublicInstanceMethod("_Chat_ParseCommand");
                    Transpiler = Transpilers.GetPrivateStaticMethod("CommandTranspiler");
                    break;
                case Patches.Game1_Item_Use:
                    Target = Game1.GetDeclaredMethods("_Item_Use").ElementAt(1);
                    Prefix = Callbacks.GetPrivateStaticMethod("OnItemUsePrefix");
                    break;
                case Patches.ItemCodex_GetItemDescription:
                    Target = typeof(ItemCodex).GetMethod("GetItemDescription");
                    Prefix = Callbacks.GetPrivateStaticMethod("OnGetItemDescription");
                    break;
                case Patches.ItemCodex_GetItemInstance:
                    Target = typeof(ItemCodex).GetMethod("GetItemInstance");
                    Prefix = Callbacks.GetPrivateStaticMethod("OnGetItemInstance");
                    break;
                case Patches.EquipmentCodex_GetArmorInfo:
                    Target = typeof(EquipmentCodex).GetMethod("GetArmorInfo");
                    Prefix = Callbacks.GetPrivateStaticMethod("OnGetEquipmentInfo");
                    break;
                case Patches.EquipmentCodex_GetAccessoryInfo:
                    Target = typeof(EquipmentCodex).GetMethod("GetAccessoryInfo");
                    Prefix = Callbacks.GetPrivateStaticMethod("OnGetEquipmentInfo");
                    break;
                case Patches.EquipmentCodex_GetShieldInfo:
                    Target = typeof(EquipmentCodex).GetMethod("GetShieldInfo");
                    Prefix = Callbacks.GetPrivateStaticMethod("OnGetEquipmentInfo");
                    break;
                case Patches.EquipmentCodex_GetShoesInfo:
                    Target = typeof(EquipmentCodex).GetMethod("GetShoesInfo");
                    Prefix = Callbacks.GetPrivateStaticMethod("OnGetEquipmentInfo");
                    break;
                case Patches.FacegearCodex_GetHatInfo:
                    Target = typeof(FacegearCodex).GetMethod("GetHatInfo");
                    Prefix = Callbacks.GetPrivateStaticMethod("OnGetFacegearInfo");
                    break;
                case Patches.HatCodex_GetHatInfo:
                    Target = typeof(HatCodex).GetMethod("GetHatInfo");
                    Prefix = Callbacks.GetPrivateStaticMethod("OnGetHatInfo");
                    break;
                case Patches.WeaponCodex_GetWeaponInfo:
                    Target = typeof(WeaponCodex).GetMethod("GetWeaponInfo");
                    Prefix = Callbacks.GetPrivateStaticMethod("OnGetWeaponInfo");
                    break;
                case Patches.WeaponContentManager_LoadBatch:
                    Target = typeof(WeaponContentManager).GetMethods(BindingFlags.NonPublic | BindingFlags.Instance).Where(item => item.Name == "LoadBatch").ElementAt(1);
                    Prefix = Callbacks.GetPrivateStaticMethod("OnLoadBatch");
                    break;
                case Patches.Game1_Animations_GetAnimationSet:
                    Target = Game1.GetMethods(BindingFlags.Public | BindingFlags.Instance).Where(item => item.Name == "_Animations_GetAnimationSet").ElementAt(1);
                    Prefix = Callbacks.GetPrivateStaticMethod("OnGetAnimationSet");
                    break;
                case Patches.SoundSystem_PlayInterfaceCue:
                    Target = SoundSystem.GetMethod("PlayInterfaceCue");
                    Transpiler = Transpilers.GetPrivateStaticMethod("PlayEffectTranspiler");
                    break;
                case Patches.SoundSystem_PlayTrackableInterfaceCue:
                    Target = SoundSystem.GetMethod("PlayTrackableInterfaceCue");
                    Transpiler = Transpilers.GetPrivateStaticMethod("GetEffectTranspiler");
                    break;
                case Patches.SoundSystem_PlayCue1:
                    Target = SoundSystem.GetDeclaredMethods("PlayCue").ElementAt(0);
                    Transpiler = Transpilers.GetPrivateStaticMethod("GetEffectTranspiler");
                    break;
                case Patches.SoundSystem_PlayCue2:
                    Target = SoundSystem.GetDeclaredMethods("PlayCue").ElementAt(1);
                    Transpiler = Transpilers.GetPrivateStaticMethod("GetEffectTranspiler");
                    break;
                case Patches.SoundSystem_PlayCue3:
                    Target = SoundSystem.GetDeclaredMethods("PlayCue").ElementAt(2);
                    Transpiler = Transpilers.GetPrivateStaticMethod("GetEffectTranspiler");
                    break;
                case Patches.SoundSystem_PlayCue4:
                    Target = SoundSystem.GetDeclaredMethods("PlayCue").ElementAt(3);
                    Transpiler = Transpilers.GetPrivateStaticMethod("GetEffectTranspiler");
                    break;
                case Patches.SoundSystem_ReadySongInCue:
                    Target = SoundSystem.GetMethod("ReadySongInCue");
                    Transpiler = Transpilers.GetPrivateStaticMethod("GetMusicTranspiler");
                    break;
                case Patches.SoundSystem_PlaySong:
                    Target = SoundSystem.GetMethod("PlaySong");
                    Transpiler = Transpilers.GetPrivateStaticMethod("GetMusicTranspiler");
                    Prefix = Callbacks.GetPrivateStaticMethod("OnPlaySong");
                    break;
                case Patches.SoundSystem_PlayMixCues:
                    Target = SoundSystem.GetMethod("PlayMixCues");
                    Transpiler = Transpilers.GetPrivateStaticMethod("PlayMixTranspiler");
                    break;
                case Patches.SoundSystem_ChangeSongRegionIfNecessary:
                    Target = SoundSystem.GetMethod("ChangeSongRegionIfNecessary");
                    Prefix = Callbacks.GetPrivateStaticMethod("OnChangeSongRegionIfNecessary");
                    break;
                case Patches.Game1_Saving_SaveCharacterToFile:
                    Target = Game1.GetMethod("_Saving_SaveCharacterToFile");
                    Postfix = Callbacks.GetPrivateStaticMethod("PostCharacterSave");
                    break;
                case Patches.Game1_Saving_SaveWorldToFile:
                    Target = Game1.GetMethod("_Saving_SaveWorldToFile");
                    Postfix = Callbacks.GetPrivateStaticMethod("PostWorldSave");
                    break;
                case Patches.Game1_Saving_SaveRogueToFile:
                    Target = Game1.GetMethods().Where(item => item.Name == "_Saving_SaveRogueToFile" && item.GetParameters().Count() == 1).First();
                    Postfix = Callbacks.GetPrivateStaticMethod("PostArcadiaSave");
                    break;
                case Patches.Game1_Loading_LoadCharacterFromFile:
                    Target = Game1.GetMethod("_Loading_LoadCharacterFromFile");
                    Postfix = Callbacks.GetPrivateStaticMethod("PostCharacterLoad");
                    break;
                case Patches.Game1_Loading_LoadWorldFromFile:
                    Target = Game1.GetMethod("_Loading_LoadWorldFromFile");
                    Postfix = Callbacks.GetPrivateStaticMethod("PostWorldLoad");
                    break;
                case Patches.Game1_Loading_LoadRogueFile:
                    Target = Game1.GetMethod("_Loading_LoadRogueFile");
                    Postfix = Callbacks.GetPrivateStaticMethod("PostArcadiaLoad");
                    break;
                default:
                    break;
            }

            return new PatchDescription()
            {
                Target = Target,
                Prefix = Prefix,
                Postfix = Postfix,
                Transpiler = Transpiler
            };
        }
    }
}
