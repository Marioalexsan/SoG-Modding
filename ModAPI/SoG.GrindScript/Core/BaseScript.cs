using Microsoft.Xna.Framework.Content;
using System;
using System.Collections.Generic;
using System.IO;
using SoG.Modding.Tools;

namespace SoG.Modding.Core
{
    public abstract class BaseScript
    {
        internal int ModIndex { get; set; }

        internal readonly PersistentLibrary Library = new PersistentLibrary();

        protected ConsoleLogger Logger { get; private set; }

        public ContentManager ModContent { get; private set; }

        public string ModPath { get; private set; }

        public GrindScript ModAPI { get; internal set; }

        protected BaseScript() 
        {
            Logger = new ConsoleLogger(ConsoleLogger.LogLevels.Debug, GetType().Name) { SourceColor = ConsoleColor.Yellow };

            ModContent = new ContentManager(ModGlobals.Game.Content.ServiceProvider, ModGlobals.Game.Content.RootDirectory);

            ModPath = $"ModContent/{GetType().Name}/";

            Logger.Info($"Mod instantiated! ModPath set as {ModPath}.");
        }

        public abstract void LoadContent();

        // Hooks

        public virtual void OnDraw() { }

        public virtual void OnPlayerDamaged(ref int damage, ref byte type) { }

        public virtual void OnPlayerKilled(PlayerView player) { }

        public virtual void PostPlayerLevelUp(PlayerView player) { }

        public virtual void OnEnemyDamaged(Enemy enemy, ref int damage, ref byte type) { }

        public virtual void OnNPCDamaged(NPC enemy, ref int damage, ref byte type) { }

        public virtual void OnNPCInteraction(NPC npc) { }

        public virtual void OnArcadiaLoad() { }

        public virtual void OnItemUse(ItemCodex.ItemTypes enItem, PlayerView xView, ref bool bSend) { }
    
        public virtual void PostArcadeRoomStart(GameSessionData.RogueLikeSession session) { }
    }
}
