using System.Linq;
using System.Reflection;

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
            SoundSystem_ChangeSongRegionIfNecessary
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
            TypeInfo Methods = typeof(PatchMethods).GetTypeInfo(); // Commonly used
            TypeInfo Game1 = GrindScript.GetGameType("SoG.Game1"); // Commonly used
            TypeInfo SoundSystem = GrindScript.GetGameType("SoG.SoundSystem"); // SoundSystem patching

            PatchDescription patch = new PatchDescription();
            switch (which)
            {
                case Patches.Game1_Initialize:
                    patch.Target = Game1.GetMethod("Initialize", BindingFlags.Instance | BindingFlags.NonPublic);
                    patch.Prefix = Methods.GetPrivateStaticMethod("OnGame1Initialize");
                    break;
                case Patches.Game1_StartupThreadExecute:
                    patch.Target = Game1.GetMethod("__StartupThreadExecute", BindingFlags.Instance | BindingFlags.Public);
                    patch.Transpiler = Methods.GetPrivateStaticMethod("StartupThreadExecute_Transpiler");
                    break;
                case Patches.Game1_FinalDraw:
                    patch.Target = Game1.GetPublicInstanceMethod("FinalDraw");
                    patch.Prefix = Methods.GetPrivateStaticMethod("OnFinalDrawPrefix");
                    break;
                case Patches.Game1_Player_TakeDamage:
                    patch.Target = Game1.GetPublicInstanceMethod("_Player_TakeDamage");
                    patch.Prefix = Methods.GetPrivateStaticMethod("OnPlayerTakeDamagePrefix");
                    break;
                case Patches.Game1_Player_KillPlayer:
                    patch.Target = Game1.GetMethods(BindingFlags.Instance | BindingFlags.Public).First(m => m.Name == "_Player_KillPlayer" && m.GetParameters().Count() > 1);
                    patch.Prefix = Methods.GetPrivateStaticMethod("OnPlayerKilledPrefix");
                    break;
                case Patches.Game1_Player_ApplyLvUpBonus:
                    patch.Target = Game1.GetPublicInstanceMethod("_Player_ApplyLvUpBonus");
                    patch.Postfix = Methods.GetPrivateStaticMethod("PostPlayerLevelUp");
                    break;
                case Patches.Game1_Enemy_TakeDamage:
                    patch.Target = Game1.GetPublicInstanceMethod("_Enemy_TakeDamage");
                    patch.Prefix = Methods.GetPrivateStaticMethod("OnEnemyTakeDamagePrefix");
                    break;
                case Patches.Game1_NPC_TakeDamage:
                    patch.Target = Game1.GetPublicInstanceMethod("_NPC_TakeDamage");
                    patch.Prefix = Methods.GetPrivateStaticMethod("OnNPCTakeDamagePrefix");
                    break;
                case Patches.Game1_NPC_Interact:
                    patch.Target = Game1.GetPublicInstanceMethod("_NPC_Interact");
                    patch.Prefix = Methods.GetPrivateStaticMethod("OnNPCInteractionPrefix");
                    break;
                case Patches.Game1_LevelLoading_DoStuff_Arcadia:
                    patch.Target = Game1.GetPublicInstanceMethod("_LevelLoading_DoStuff_Arcadia");
                    patch.Prefix = Methods.GetPrivateStaticMethod("OnArcadiaLoadPrefix");
                    break;
                case Patches.Game1_Chat_ParseCommand:
                    patch.Target = Game1.GetPublicInstanceMethod("_Chat_ParseCommand");
                    patch.Transpiler = Methods.GetPrivateStaticMethod("Chat_ParseCommandTranspiler");
                    break;
                case Patches.Game1_Item_Use:
                    patch.Target = Game1.GetDeclaredMethods("_Item_Use").ElementAt(1);
                    patch.Prefix = Methods.GetPrivateStaticMethod("OnItemUsePrefix");
                    break;
                case Patches.ItemCodex_GetItemDescription:
                    patch.Target = typeof(ItemCodex).GetMethod("GetItemDescription");
                    patch.Prefix = Methods.GetPrivateStaticMethod("OnGetItemDescription");
                    break;
                case Patches.ItemCodex_GetItemInstance:
                    patch.Target = typeof(ItemCodex).GetMethod("GetItemInstance");
                    patch.Prefix = Methods.GetPrivateStaticMethod("OnGetItemInstance");
                    break;
                case Patches.EquipmentCodex_GetArmorInfo:
                    patch.Target = typeof(EquipmentCodex).GetMethod("GetArmorInfo");
                    patch.Prefix = Methods.GetPrivateStaticMethod("OnGetEquipmentInfo");
                    break;
                case Patches.EquipmentCodex_GetAccessoryInfo:
                    patch.Target = typeof(EquipmentCodex).GetMethod("GetAccessoryInfo");
                    patch.Prefix = Methods.GetPrivateStaticMethod("OnGetEquipmentInfo");
                    break;
                case Patches.EquipmentCodex_GetShieldInfo:
                    patch.Target = typeof(EquipmentCodex).GetMethod("GetShieldInfo");
                    patch.Prefix = Methods.GetPrivateStaticMethod("OnGetEquipmentInfo");
                    break;
                case Patches.EquipmentCodex_GetShoesInfo:
                    patch.Target = typeof(EquipmentCodex).GetMethod("GetShoesInfo");
                    patch.Prefix = Methods.GetPrivateStaticMethod("OnGetEquipmentInfo");
                    break;
                case Patches.FacegearCodex_GetHatInfo:
                    patch.Target = typeof(FacegearCodex).GetMethod("GetHatInfo");
                    patch.Prefix = Methods.GetPrivateStaticMethod("OnGetFacegearInfo");
                    break;
                case Patches.HatCodex_GetHatInfo:
                    patch.Target = typeof(HatCodex).GetMethod("GetHatInfo");
                    patch.Prefix = Methods.GetPrivateStaticMethod("OnGetHatInfo");
                    break;
                case Patches.WeaponCodex_GetWeaponInfo:
                    patch.Target = typeof(WeaponCodex).GetMethod("GetWeaponInfo");
                    patch.Prefix = Methods.GetPrivateStaticMethod("OnGetWeaponInfo");
                    break;
                case Patches.WeaponContentManager_LoadBatch:
                    patch.Target = typeof(WeaponAssets.WeaponContentManager).GetMethods(BindingFlags.NonPublic | BindingFlags.Instance).Where(item => item.Name == "LoadBatch").ElementAt(1);
                    patch.Prefix = Methods.GetPrivateStaticMethod("OnLoadBatch");
                    break;
                case Patches.Game1_Animations_GetAnimationSet:
                    patch.Target = Game1.GetMethods(BindingFlags.Public | BindingFlags.Instance).Where(item => item.Name == "_Animations_GetAnimationSet").ElementAt(1);
                    patch.Prefix = Methods.GetPrivateStaticMethod("OnGetAnimationSet");
                    break;
                case Patches.SoundSystem_PlayInterfaceCue:
                    patch.Target = SoundSystem.GetMethod("PlayInterfaceCue");
                    patch.Transpiler = Methods.GetPrivateStaticMethod("PlayEffectCueTranspiler");
                    break;
                case Patches.SoundSystem_PlayTrackableInterfaceCue:
                    patch.Target = SoundSystem.GetMethod("PlayTrackableInterfaceCue");
                    patch.Transpiler = Methods.GetPrivateStaticMethod("GetEffectCueTranspiler");
                    break;
                case Patches.SoundSystem_PlayCue1:
                    patch.Target = SoundSystem.GetDeclaredMethods("PlayCue").ElementAt(0);
                    patch.Transpiler = Methods.GetPrivateStaticMethod("GetEffectCueTranspiler");
                    break;
                case Patches.SoundSystem_PlayCue2:
                    patch.Target = SoundSystem.GetDeclaredMethods("PlayCue").ElementAt(1);
                    patch.Transpiler = Methods.GetPrivateStaticMethod("GetEffectCueTranspiler");
                    break;
                case Patches.SoundSystem_PlayCue3:
                    patch.Target = SoundSystem.GetDeclaredMethods("PlayCue").ElementAt(2);
                    patch.Transpiler = Methods.GetPrivateStaticMethod("GetEffectCueTranspiler");
                    break;
                case Patches.SoundSystem_PlayCue4:
                    patch.Target = SoundSystem.GetDeclaredMethods("PlayCue").ElementAt(3);
                    patch.Transpiler = Methods.GetPrivateStaticMethod("GetEffectCueTranspiler");
                    break;
                case Patches.SoundSystem_ReadySongInCue:
                    patch.Target = SoundSystem.GetMethod("ReadySongInCue");
                    patch.Transpiler = Methods.GetPrivateStaticMethod("GetMusicCueTranspiler");
                    break;
                case Patches.SoundSystem_PlaySong:
                    patch.Target = SoundSystem.GetMethod("PlaySong");
                    patch.Transpiler = Methods.GetPrivateStaticMethod("GetMusicCueTranspiler");
                    patch.Prefix = Methods.GetPrivateStaticMethod("OnPlaySong");
                    break;
                case Patches.SoundSystem_PlayMixCues:
                    patch.Target = SoundSystem.GetMethod("PlayMixCues");
                    patch.Transpiler = Methods.GetPrivateStaticMethod("PlayMixCuesTranspiler");
                    break;
                case Patches.SoundSystem_ChangeSongRegionIfNecessary:
                    patch.Target = SoundSystem.GetMethod("ChangeSongRegionIfNecessary");
                    patch.Prefix = Methods.GetPrivateStaticMethod("OnChangeSongRegionIfNecessary");
                    break;
                default:
                    patch = null;
                    break;
            }

            return patch;
        }
    }
}
