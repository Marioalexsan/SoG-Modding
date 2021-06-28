using Microsoft.Xna.Framework.Content;
using System;
using System.Collections.Generic;
using System.IO;

namespace SoG.Modding
{
    public class BaseScript
    {
        internal readonly int _audioID;

        internal readonly ModLibrary ModLib = new ModLibrary();

        public ContentManager ModContent { get; private set; }

        public string ModPath { get; private set; }

        public static readonly string SoGPath = "Content/";

        protected ConsoleLogger Logger { get; private set; }

        protected BaseScript() 
        {
            Logger = new ConsoleLogger(ConsoleLogger.LogLevels.Debug, GetType().Name) { SourceColor = ConsoleColor.Yellow };

            ModContent = new ContentManager(GrindScript.Game.Content.ServiceProvider, GrindScript.Game.Content.RootDirectory);

            ModPath = $"ModContent/{GetType().Name}/";

            Logger.Info($"ModPath set as {ModPath}");

            var allAudio = ModLibrary.Global.Audio;
            if (allAudio.ContainsKey(IDAllocator.AudioIDNext))
            {
                _audioID = IDAllocator.AudioIDNext;
                Logger.Warn($"An existing audio entry ({_audioID}) was found while trying to create one for mod {GetType().Name}!");
            }
            else
            {
                _audioID = IDAllocator.NewAudioEntry();
                allAudio.Add(_audioID, new ModAudioEntry() { allocatedID = _audioID, owner = this });
                Logger.Info($"AudioID set as {_audioID}");
            }

            ModLibrary.Global.Commands[GetType().Name] = new Dictionary<string, CommandParser>();
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
