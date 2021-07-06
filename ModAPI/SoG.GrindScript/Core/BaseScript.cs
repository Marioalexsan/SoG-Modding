using Microsoft.Xna.Framework.Content;
using System;
using System.Collections.Generic;
using System.IO;
using SoG.Modding.Utils;

namespace SoG.Modding.Core
{
    /// <summary>
    /// Represents the base class for all mods.
    /// Mod DLLs should have one class that subclasses from BaseScript to be usable. <para/>
    /// All mods need to implement the LoadContent method. GrindScript will call this method when modded content should be loaded. <para/>
    /// BaseScript has a few callbacks that can be overriden to add extra behavior to the game for certain events.
    /// </summary>
    public abstract class BaseScript
    {
        internal int ModIndex { get; set; }

        internal readonly ModLibrary Library = new ModLibrary();

        public ConsoleLogger Logger { get; private set; }

        public ContentManager ModContent { get; internal set; }

        public string ModPath { get; internal set; }

        public GrindScript ModAPI { get; internal set; }

        public BaseScript()
        {
            Logger = new ConsoleLogger(ConsoleLogger.LogLevels.Debug, GetType().Name) { SourceColor = ConsoleColor.Yellow };
            Logger.Info($"Mod instantiated!");
        }

        /// <summary>
        /// Creates modded game content. GrindScript calls this method during the game's initialization.
        /// </summary>
        public abstract void LoadContent();

        /// <summary>
        /// Called when the game begins a frame render.
        /// </summary>
        public virtual void OnDraw() { }

        /// <summary>
        /// Called when a player is damaged by something.
        /// </summary>
        public virtual void OnPlayerDamaged(PlayerView player, ref int damage, ref byte type) { }

        /// <summary>
        /// Called when a player dies.
        /// </summary>
        public virtual void OnPlayerKilled(PlayerView player) { }

        /// <summary>
        /// Called when a player levels up.
        /// During save file loading, this method is called multiple times to initialize the player's stats to their level.
        /// </summary>
        public virtual void PostPlayerLevelUp(PlayerView player) { }

        /// <summary>
        /// Called when an enemy is damaged by something.
        /// </summary>
        public virtual void OnEnemyDamaged(Enemy enemy, ref int damage, ref byte type) { }

        /// <summary>
        /// Called when an NPC is damaged by something.
        /// </summary>
        public virtual void OnNPCDamaged(NPC enemy, ref int damage, ref byte type) { }

        /// <summary>
        /// Called when a player interacts with an NPC
        /// </summary>
        public virtual void OnNPCInteraction(NPC npc) { }

        /// <summary>
        /// Called when the game loads Arcadia's level.
        /// </summary>
        public virtual void OnArcadiaLoad() { }

        /// <summary>
        /// Called when a player uses an item. This method can be used to implement behavior for usable items.
        /// Items can be used if they have the "Usable" item category.
        /// </summary>
        public virtual void OnItemUse(ItemCodex.ItemTypes enItem, PlayerView xView, ref bool bSend) { }
    
        /// <summary>
        /// Called when a new Arcade room is entered, after it has been prepared by the game
        /// (i.e. enemies have been spawned, traps laid out, etc.)
        /// </summary>
        public virtual void PostArcadeRoomStart(GameSessionData.RogueLikeSession session) { }
    }
}
