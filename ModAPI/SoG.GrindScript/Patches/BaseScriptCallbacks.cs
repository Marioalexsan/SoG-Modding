using System.Linq;
using SoG.Modding.Core;

namespace SoG.Modding.Patches
{
    /// <summary>
    /// Contains methods that act as patches for the game (Prefix, Postfix and Transpilers).
    /// </summary>

    internal static partial class PatchCollection
    {
        private static void OnContentLoad()
        {
            foreach (BaseScript mod in ModGlobals.API.LoadedScripts)
            {
                ModGlobals.API.CallWithContext(mod, x => x.LoadContent());
            }
        }

        private static void OnFinalDraw()
        {
            ModGlobals.API.LoadedScripts.ForEach(mod => mod.OnDraw());
        }

        private static void OnPlayerTakeDamage(ref int iInDamage, ref byte byType)
        {
            foreach (BaseScript mod in ModGlobals.API.LoadedScripts)
                mod.OnPlayerDamaged(ref iInDamage, ref byType);
        }

        private static void OnPlayerKilled(PlayerView xView)
        {
            ModGlobals.API.LoadedScripts.ForEach(mod => mod.OnPlayerKilled(xView));
        }

        private static void PostPlayerLevelUp(PlayerView xView)
        {
            ModGlobals.API.LoadedScripts.ForEach(mod => mod.PostPlayerLevelUp(xView));
        }

        private static void OnEnemyTakeDamage(Enemy xEnemy, ref int iDamage, ref byte byType)
        {
            foreach (BaseScript mod in ModGlobals.API.LoadedScripts)
                mod.OnEnemyDamaged(xEnemy, ref iDamage, ref byType);
        }

        private static void OnNPCTakeDamage(NPC xEnemy, ref int iDamage, ref byte byType)
        {
            foreach (BaseScript mod in ModGlobals.API.LoadedScripts)
                mod.OnNPCDamaged(xEnemy, ref iDamage, ref byType);
        }

        private static void OnNPCInteraction(PlayerView xView, NPC xNPC)
        {
            foreach (BaseScript mod in ModGlobals.API.LoadedScripts)
                mod.OnNPCInteraction(xNPC);
        }

        private static void OnArcadiaLoad()
        {
            foreach (BaseScript mod in ModGlobals.API.LoadedScripts)
                mod.OnArcadiaLoad();

            // Just in case it didn't get set before; submitting modded runs is not a good idea
            ModGlobals.Game.xGameSessionData.xRogueLikeSession.bTemporaryHighScoreBlock = true;
        }

        private static void OnItemUse(ItemCodex.ItemTypes enItem, PlayerView xView, ref bool bSend)
        {
            if (xView.xViewStats.bIsDead) return;
            foreach (BaseScript mod in ModGlobals.API.LoadedScripts)
                mod.OnItemUse(enItem, xView, ref bSend);
        }

        private static void PostArcadeRoomStart()
        {
            GameSessionData.RogueLikeSession session = ModGlobals.Game.xGameSessionData.xRogueLikeSession;
            foreach (BaseScript mod in ModGlobals.API.LoadedScripts)
                mod.PostArcadeRoomStart(session);
        }
    }
}
