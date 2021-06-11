using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace SoG.GrindScript
{
    public partial class BaseScript
    {
        protected ContentManager ModContent;

        protected BaseScript() 
        {
            ModContent = new ContentManager(GrindScript.Game.Content.ServiceProvider, "ModContent/" + GetType().Name);

            Console.WriteLine(this.GetType().Name + " ContentManager path set as " + ModContent.RootDirectory);
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
