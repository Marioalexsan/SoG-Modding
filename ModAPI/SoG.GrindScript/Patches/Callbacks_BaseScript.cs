using System;
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
            foreach (BaseScript mod in APIGlobals.API.LoadedScripts)
            {
                APIGlobals.API.CallWithContext(mod, x =>
                {
                    try
                    {
                        x.LoadContent();
                    }
                    catch (Exception e)
                    {
                        APIGlobals.Logger.Error($"Mod {mod.GetType().Name} threw an exception during LoadContent: {e.Message}");
                    }
                });
            }
        }

        private static void OnFinalDraw()
        {
            foreach (BaseScript mod in APIGlobals.API.LoadedScripts)
                mod.OnDraw();
        }

        private static void OnPlayerTakeDamage(PlayerView xView, ref int iInDamage, ref byte byType)
        {
            foreach (BaseScript mod in APIGlobals.API.LoadedScripts)
                mod.OnPlayerDamaged(xView, ref iInDamage, ref byType);
        }

        private static void OnPlayerKilled(PlayerView xView)
        {
            foreach (BaseScript mod in APIGlobals.API.LoadedScripts)
                mod.OnPlayerKilled(xView);
        }

        private static void PostPlayerLevelUp(PlayerView xView)
        {
            foreach (BaseScript mod in APIGlobals.API.LoadedScripts)
                mod.PostPlayerLevelUp(xView);
        }

        private static void OnEnemyTakeDamage(Enemy xEnemy, ref int iDamage, ref byte byType)
        {
            foreach (BaseScript mod in APIGlobals.API.LoadedScripts)
                mod.OnEnemyDamaged(xEnemy, ref iDamage, ref byType);
        }

        private static void OnNPCTakeDamage(NPC xEnemy, ref int iDamage, ref byte byType)
        {
            foreach (BaseScript mod in APIGlobals.API.LoadedScripts)
                mod.OnNPCDamaged(xEnemy, ref iDamage, ref byType);
        }

        private static void OnNPCInteraction(PlayerView xView, NPC xNPC)
        {
            foreach (BaseScript mod in APIGlobals.API.LoadedScripts)
                mod.OnNPCInteraction(xNPC);
        }

        private static void OnArcadiaLoad()
        {
            foreach (BaseScript mod in APIGlobals.API.LoadedScripts)
                mod.OnArcadiaLoad();

            // Just in case it didn't get set before; submitting modded runs is not a good idea
            APIGlobals.Game.xGameSessionData.xRogueLikeSession.bTemporaryHighScoreBlock = true;
        }

        private static void OnItemUse(ItemCodex.ItemTypes enItem, PlayerView xView, ref bool bSend)
        {
            if (xView.xViewStats.bIsDead)
                return;
            foreach (BaseScript mod in APIGlobals.API.LoadedScripts)
                mod.OnItemUse(enItem, xView, ref bSend);
        }

        private static void PostArcadeRoomStart()
        {
            foreach (BaseScript mod in APIGlobals.API.LoadedScripts)
                mod.PostArcadeRoomStart(APIGlobals.Game.xGameSessionData.xRogueLikeSession);
        }
    }
}
