using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using System.Reflection;
using System.IO;
using LevelLoading;

namespace SoG.Modding
{
	/// <summary>
	/// Extension methods for LevelBlueprint that expose some vanilla methods and add new ones.
	/// </summary>

    public static class LevelExtension
    {
		/// <summary>
		/// Adds a collider that acts as an invisible wall, and applies an offset to its position.
		/// </summary>

		public static void AddCollider(this LevelBlueprint blueprint, Collider col, int bitLayers, bool isFlat, Vector2 offset, bool isOutline = true)
        {
            if (blueprint.lxInvisibleWalls != null && offset != null)
            {
				col.xLocalTransform.v2Pos += offset;
				col.ibitLayers = Utility.CreateIntMask(bitLayers);
				col.bIsFlatCollider = isFlat;
				col.bIsLarge = !isFlat;
				if (isOutline)
				{
					col.ibitLayers = Utility.AddIntFlag(col.ibitLayers, 30);
				}
				col.ibitLayers = Utility.AddIntFlag(col.ibitLayers, 29);
				col.AddSpecialLayer(Collider.SpecialLayer.StaticOutlineCollider);
				blueprint.lxInvisibleWalls.Add(col);
			}
        }

		/// <summary>
		/// Adds colliders from a file at the path given.
		/// The path is relative to the SoG executable.
		/// </summary>

		public static void AddCollidersFromFile(this LevelBlueprint blueprint, string relativePath, Vector2 offset, int overrideLayer = 0)
		{
			relativePath = Directory.GetCurrentDirectory() + relativePath;

			if (!File.Exists(relativePath))
				return;

			using (BinaryReader br = new BinaryReader(new FileStream(relativePath, FileMode.Open, FileAccess.Read)))
			{
				int iLoadedIteration = br.ReadInt32();
				int iCount = br.ReadInt32();
				for (int i = 0; i < iCount; i++)
				{
					Collider col = Loader.LoadCollider(br, blueprint.lxInvisibleWalls, offset, iLoadedIteration);
					if (overrideLayer > 0)
						col.ibitLayers = overrideLayer;
				}
			}
		}

		/// <summary>
		/// Adds a bush to the level.
		/// </summary>

		public static void AddBush(this LevelBlueprint blueprint, Vector2 position, int layer, bool orangeBush = false)
        {
			LevelBlueprint.StaticObject whackToUse = LevelBlueprint.StaticObject._Forest_BushWhacked;
			LevelBlueprint.StaticObject bushToUse = LevelBlueprint.StaticObject._Dynamic_Bush;

			if (orangeBush)
			{
				whackToUse = LevelBlueprint.StaticObject._Forest_BushWhackedOrange;
				bushToUse = LevelBlueprint.StaticObject._Dynamic_BushOrange;
			}

			blueprint.lxStaticLevelObjects.Add(new LevelBlueprint.LevelObjectBlueprint(whackToUse, position, true, 0f, layer));
			blueprint.lxDynamicLevelObjects.Add(new LevelBlueprint.LevelObjectBlueprint(bushToUse, position, true, 0f, layer));
		}

		/// <summary>
		/// Adds multiple bushes to the level, in a grid pattern.
		/// </summary>

		public static void AddBushGrid(this LevelBlueprint blueprint, Vector2 topLeftBush, int spacing, int collumns, int rows, int layer, bool orangeBushes = false)
        {
			for (int i = 0; i < collumns; i++)
            {
				for (int j = 0; j < rows; j++)
                {
					Vector2 position = topLeftBush + new Vector2(spacing * i, spacing * j);

					blueprint.AddBush(position, layer, orangeBushes);
				}
            }
        }

		/// <summary>
		/// Adds a level switch.
		/// This causes a level switch if everyone is within the respective bounds.
		/// </summary>

		public static void AddLevelSwitch(this LevelBlueprint blueprint, Rectangle leaderBounds, Rectangle teamBounds, Level.ZoneEnum targetZone, int spawnpoint)
        {
			blueprint.lxZoningFields.Add(new Level.ZoningField(leaderBounds, teamBounds, targetZone, spawnpoint));
        }

		/// <summary>
		/// Add a transport within this level.
		/// This is normally used for indoors / outdoors transitions.
		/// </summary>

		public static void AddTransport(this LevelBlueprint blueprint, Rectangle bounds, Vector2 targetPosition, int layer)
		{
			blueprint.lxPhasingFields.Add(new Level.PhasingField(bounds, targetPosition, layer));
		}

		/// <summary>
		/// Adds a new spawnpoint in this level, and returns its index.
		/// </summary>

		public static int AddSpawnpoint(this LevelBlueprint blueprint, Vector2 spawnpoint, int layer)
        {
			int nextIndex = blueprint.av2SpawnPoint.Length;

			Vector2[] newSP = new Vector2[nextIndex + 1];
			newSP[nextIndex] = spawnpoint;
			blueprint.av2SpawnPoint.CopyTo(newSP, 0);
			blueprint.av2SpawnPoint = newSP;

			int[] newSPL = new int[nextIndex + 1];
			newSPL[nextIndex] = layer;
			blueprint.aiSpawnColliderLayer.CopyTo(newSPL, 0);
			blueprint.aiSpawnColliderLayer = newSPL;

			return nextIndex;
		}
	}
}
