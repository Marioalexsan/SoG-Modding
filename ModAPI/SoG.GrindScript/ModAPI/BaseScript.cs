using Microsoft.Xna.Framework.Content;
using System;
using System.Collections.Generic;

namespace SoG.Modding
{
    public partial class BaseScript
    {
        internal readonly int _audioID;

        internal readonly ModLibrary ModLib = new ModLibrary();

        public ContentManager CustomAssets { get; private set; }

        protected ConsoleLogger Logger { get; private set; }

        protected BaseScript() 
        {
            Logger = new ConsoleLogger(ConsoleLogger.LogLevels.Debug, GetType().Name) { SourceColor = ConsoleColor.Yellow };

            CustomAssets = new ContentManager(GrindScript.Game.Content.ServiceProvider, "ModContent/" + GetType().Name);
            Logger.Info($"ContentManager path set as {CustomAssets.RootDirectory}");

            var allAudio = ModLibrary.Global.ModAudio;
            if (allAudio.ContainsKey(ModAllocator.AudioIDNext))
            {
                _audioID = ModAllocator.AudioIDNext;
                Logger.Warn($"An existing audio entry ({_audioID}) was found while trying to create one for mod {GetType().Name}!");
            }
            else
            {
                _audioID = ModAllocator.AllocateAudioEntry();
                allAudio.Add(_audioID, new ModAudioEntry() { allocatedID = _audioID, owner = this });
                Logger.Info($"AudioID set as {_audioID}");
            }

            ModLibrary.Global.ModCommands[GetType().Name] = new Dictionary<string, CommandParser>();
        }

        public virtual void LoadContent() { }

        // Hooks

        public virtual void OnDraw() { }

        public virtual void OnPlayerDamaged(ref int damage, ref byte type) { }

        public virtual void OnPlayerKilled() { }

        public virtual void PostPlayerLevelUp(PlayerView player) { }

        public virtual void OnEnemyDamaged(Enemy enemy, ref int damage, ref byte type) { }

        public virtual void OnNPCDamaged(NPC enemy, ref int damage, ref byte type) { }

        public virtual void OnNPCInteraction(NPC npc) { }

        public virtual void OnArcadiaLoad() { }

        public virtual void OnItemUse(ItemCodex.ItemTypes enItem, PlayerView xView, ref bool bSend) { }
    }
}
