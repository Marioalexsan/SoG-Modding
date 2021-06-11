using System;
using SoG.GrindScript;

namespace SoG.FeatureExample
{
    public class FeatureExample: BaseScript
    {
        bool drewStuff = false;

        public FeatureExample()
        {
            Console.WriteLine("FeatureExample is an example mod that tests GrindScript functionality.");
        }

        public override void OnCustomContentLoad()
        {
            Console.WriteLine("FeatureExample: Loading....");
            try
            {

                Console.WriteLine("FeatureExample: Loaded Successfully!");
            }
            catch(Exception e)
            {
                Console.WriteLine("FeatureExample: Failed to load!");
                Console.WriteLine(e.StackTrace);
                Console.WriteLine(e.Message);
            }
        }

        public override void OnDraw()
        {
            if (!drewStuff) return;
            drewStuff = true;
            Console.WriteLine("FeatureExample::OnDraw() called!");
        }

        public override void OnPlayerDamaged(ref int damage, ref byte type)
        {
            Console.WriteLine("FeatureExample::OnPlayerDamaged() called!");
        }

        public override void OnPlayerKilled()
        {
            Console.WriteLine("FeatureExample::OnPlayerKilled() called!");
        }

        public override void PostPlayerLevelUp(PlayerView player)
        {
            Console.WriteLine("FeatureExample::PostPlayerLevelUp() called!");
        }

        public override void OnEnemyDamaged(Enemy enemy, ref int damage, ref byte type)
        {
            Console.WriteLine("FeatureExample::OnEnemyDamaged() called!");
        }

        public override void OnNPCDamaged(NPC enemy, ref int damage, ref byte type)
        {
            Console.WriteLine("FeatureExample::OnNPCDamaged() called!");
        }

        public override void OnNPCInteraction(NPC npc)
        {
            Console.WriteLine("FeatureExample::OnNPCInteraction() called!");
        }

        public override void OnArcadiaLoad()
        {
            Console.WriteLine("FeatureExample::OnArcadiaLoad() called!");
        }

        public override bool OnChatParseCommand(string command, string argList, int connection)
        {
            Console.WriteLine("FeatureExample::OnChatParseCommand() called!");
            CAS.AddChatMessage("Loud and clear!");
            return true;
        }

        public override void OnItemUse(ItemCodex.ItemTypes enItem, PlayerView xView, ref bool bSend)
        {
            Console.WriteLine("FeatureExample::OnItemUse() called!");
        }
    }
}
