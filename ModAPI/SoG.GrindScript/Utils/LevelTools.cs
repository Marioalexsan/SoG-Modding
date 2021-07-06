using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace SoG.Modding.Utils
{
    internal class LevelTools
    {
        public static void BlueprintSanityCheck(LevelBlueprint bp, bool withSpawns = false)
        {
            // aiSpawnColliderLayer and aiLayerDefaultHeight must either be null,
            // or be at least as long as av2SpawnPoint

            // av2SpawnPoint.Length must be larger than entry point index,
            // but we can't check that here.

            if (bp == null) return;

            if (bp.lxStaticLevelObjects == null) bp.lxStaticLevelObjects = new List<LevelBlueprint.LevelObjectBlueprint>();
            if (bp.lxDynamicLevelObjects == null) bp.lxDynamicLevelObjects = new List<LevelBlueprint.LevelObjectBlueprint>();
            if (bp.lxInvisibleWalls == null) bp.lxInvisibleWalls = new List<Collider>();
            if (bp.lxBackgroundSprites == null) bp.lxBackgroundSprites = new List<LevelBlueprint.BackgroundSpriteBP>();
            if (bp.lxZoningFields == null) bp.lxZoningFields = new List<Level.ZoningField>();
            if (bp.lxPhasingFields == null) bp.lxPhasingFields = new List<Level.PhasingField>();
            if (bp.lxDescriptionSquares == null) bp.lxDescriptionSquares = new List<DescriptionSquare>();
            if (bp.lrecBattleBoxes == null) bp.lrecBattleBoxes = new List<Rectangle>();
            if (bp.lrecBattleBoxes == null) bp.lrecBattleBoxes = new List<Rectangle>();
            if (bp.dsv2GrindeaSpriteFiles == null) bp.dsv2GrindeaSpriteFiles = new Dictionary<string, Vector2>();
            if (bp.dsv2GrindeaWaypointFiles == null) bp.dsv2GrindeaWaypointFiles = new Dictionary<string, Vector2>();
            if (bp.lenPuzzles == null) bp.lenPuzzles = new List<IPuzzle.PuzzleID>();

            if (withSpawns)
            {
                if (bp.av2SpawnPoint == null || bp.av2SpawnPoint.Length == 0)
                    bp.av2SpawnPoint = new Vector2[] { new Vector2(0, 0) };

                int spawnpoints = bp.av2SpawnPoint.Length;

                if (bp.aiSpawnColliderLayer != null && bp.aiSpawnColliderLayer.Length < spawnpoints)
                {
                    var old = bp.aiSpawnColliderLayer;
                    bp.aiSpawnColliderLayer = new int[spawnpoints];
                    old.CopyTo(bp.aiSpawnColliderLayer, 0);
                }

                if (bp.aiLayerDefaultHeight != null && bp.aiLayerDefaultHeight.Length < spawnpoints)
                {
                    var old = bp.aiLayerDefaultHeight;
                    bp.aiLayerDefaultHeight = new int[spawnpoints];
                    old.CopyTo(bp.aiLayerDefaultHeight, 0);
                }
            }
            
            if (bp.lxLevelPartitions == null)
                bp.lxLevelPartitions = new List<Level.LevelPartition> { new Level.LevelPartition(bp.recLevelBounds, false, Level.LevelPartition.ReverbSetting.Normal) };
        }
    }
}
