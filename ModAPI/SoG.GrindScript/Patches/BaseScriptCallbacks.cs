using System.Linq;

namespace SoG.Modding
{
    /// <summary>
    /// Contains methods that act as patches for the game (Prefix, Postfix and Transpilers).
    /// </summary>

    internal static partial class Patches
    {
        private static void OnContentLoad()
        {
            GrindScript._loadedScripts.ForEach(mod => mod.LoadContent());
        }

        private static void OnFinalDraw()
        {
            GrindScript._loadedScripts.ForEach(mod => mod.OnDraw());
        }

        private static void OnPlayerTakeDamage(ref int iInDamage, ref byte byType)
        {
            foreach (BaseScript mod in GrindScript._loadedScripts)
                mod.OnPlayerDamaged(ref iInDamage, ref byType);
        }

        private static void OnPlayerKilled(PlayerView xView)
        {
            GrindScript._loadedScripts.ForEach(mod => mod.OnPlayerKilled(xView));
        }

        private static void PostPlayerLevelUp(PlayerView xView)
        {
            GrindScript._loadedScripts.ForEach(mod => mod.PostPlayerLevelUp(xView));
        }

        private static void OnEnemyTakeDamage(Enemy xEnemy, ref int iDamage, ref byte byType)
        {
            foreach (BaseScript mod in GrindScript._loadedScripts)
                mod.OnEnemyDamaged(xEnemy, ref iDamage, ref byType);
        }

        private static void OnNPCTakeDamage(NPC xEnemy, ref int iDamage, ref byte byType)
        {
            foreach (BaseScript mod in GrindScript._loadedScripts)
                mod.OnNPCDamaged(xEnemy, ref iDamage, ref byType);
        }

        private static void OnNPCInteraction(PlayerView xView, NPC xNPC)
        {
            foreach (BaseScript mod in GrindScript._loadedScripts)
                mod.OnNPCInteraction(xNPC);
        }

        private static void OnArcadiaLoad()
        {
            foreach (BaseScript mod in GrindScript._loadedScripts)
                mod.OnArcadiaLoad();

            // Just in case it didn't get set before; submitting modded runs is not a good idea
            GrindScript.Game.xGameSessionData.xRogueLikeSession.bTemporaryHighScoreBlock = true;
        }

        private static void OnItemUse(ItemCodex.ItemTypes enItem, PlayerView xView, ref bool bSend)
        {
            if (xView.xViewStats.bIsDead) return;
            foreach (BaseScript mod in GrindScript._loadedScripts)
                mod.OnItemUse(enItem, xView, ref bSend);
        }

        private static void PostArcadeRoomStart()
        {
            GameSessionData.RogueLikeSession session = GrindScript.Game.xGameSessionData.xRogueLikeSession;
            foreach (BaseScript mod in GrindScript._loadedScripts)
                mod.PostArcadeRoomStart(session);
        }
    }
}
