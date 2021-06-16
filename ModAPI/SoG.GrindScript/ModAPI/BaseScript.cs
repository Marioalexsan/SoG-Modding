using Microsoft.Xna.Framework.Content;
using System;

namespace SoG.Modding
{
    public partial class BaseScript
    {
        protected ContentManager CustomAssets;

        protected ConsoleLogger Logger;

        protected BaseScript() 
        {
            CustomAssets = new ContentManager(GrindScript.Game.Content.ServiceProvider, "ModContent/" + GetType().Name);
            Logger = new ConsoleLogger(ConsoleLogger.LogLevels.Debug, GetType().Name) { SourceColor = ConsoleColor.Yellow };

            Logger.Info($"ContentManager path set as {CustomAssets.RootDirectory}");
        }

        public virtual void OnDraw()
        {
            return;
        }

        public virtual void OnPlayerDamaged(ref int damage, ref byte type)
        {
            return;
        }

        public virtual void OnPlayerKilled()
        {
            return;
        }

        public virtual void PostPlayerLevelUp(PlayerView player)
        {
            return;
        }

        public virtual void OnEnemyDamaged(Enemy enemy, ref int damage, ref byte type)
        {
            return;
        }

        public virtual void OnNPCDamaged(NPC enemy, ref int damage, ref byte type)
        {
            return;
        }

        public virtual void OnNPCInteraction(NPC npc)
        {
            return;
        }

        public virtual void OnArcadiaLoad()
        {
            return;
        }

        public virtual void OnCustomContentLoad()
        {
            return;
        }

        // This should return true if the command was parsed, false otherwise.
        public virtual bool OnChatParseCommand(string command, string argList, int connection)
        {
            return false;
        }

        public virtual void OnItemUse(ItemCodex.ItemTypes enItem, PlayerView xView, ref bool bSend)
        {
            return;
        }
    }
}
