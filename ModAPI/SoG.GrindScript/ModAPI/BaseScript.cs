using Microsoft.Xna.Framework.Content;
using System;

namespace SoG.Modding
{
    public partial class BaseScript
    {
        public ContentManager CustomAssets { get; private set; }

        protected ConsoleLogger Logger { get; private set; }

        internal readonly int _AudioID;

        protected BaseScript() 
        {
            Logger = new ConsoleLogger(ConsoleLogger.LogLevels.Debug, GetType().Name) { SourceColor = ConsoleColor.Yellow };

            CustomAssets = new ContentManager(GrindScript.Game.Content.ServiceProvider, "ModContent/" + GetType().Name);
            Logger.Info($"ContentManager path set as {CustomAssets.RootDirectory}");

            var allAudio = ModLibrary.Global.ModAudio;
            if (allAudio.ContainsKey(ModAllocator.AudioIDNext))
            {
                _AudioID = ModAllocator.AudioIDNext;
                Logger.Warn($"An existing audio entry ({_AudioID}) was found while trying to create one for mod {GetType().Name}!");
            }
            else
            {
                _AudioID = ModAllocator.AllocateAudioEntry();
                allAudio.Add(_AudioID, new ModAudioEntry() { allocatedID = _AudioID, owner = this });
                Logger.Info($"AudioID set as {_AudioID}");
            }
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
