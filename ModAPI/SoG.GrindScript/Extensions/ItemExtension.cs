using Microsoft.Xna.Framework;
using System;

namespace SoG.Modding
{
    public static class ItemExtension
    {
        /// <summary>
        /// Spawns an item at the target PlayerView's position.
        /// </summary>

        public static Item SpawnItem(this ItemCodex.ItemTypes enType, PlayerView xTarget)
        {
            PlayerEntity xEntity = xTarget.xEntity;

            return enType.SpawnItem(xEntity.xTransform.v2Pos, xEntity.xRenderComponent.fVirtualHeight, xEntity.xCollisionComponent.ibitCurrentColliderLayer);
        }

        /// <summary>
        /// Spawns an item at the target position.
        /// </summary>

        public static Item SpawnItem(this ItemCodex.ItemTypes enType, Vector2 v2Pos, float fVirtualHeight, int iColliderLayer)
        {
            Vector2 v2ThrowDirection = Utility.RandomizeVector2Direction(CAS.RandomInLogic);

            return GrindScript.Game._EntityMaster_AddItem(enType, v2Pos, fVirtualHeight, iColliderLayer, v2ThrowDirection);
        }

        /// <summary>
        /// Checks if the given item ID is a vanilla item (i.e. is present in the base game)
        /// </summary>

        public static bool IsSoGItem(this ItemCodex.ItemTypes enType)
        {
            return Enum.IsDefined(typeof(ItemCodex.ItemTypes), enType);
        }

        /// <summary>
        /// Checks if the given item ID is a mod item (i.e. it is currently allocated by GrindScript)
        /// </summary>

        public static bool IsModItem(this ItemCodex.ItemTypes enType)
        {
            return enType >= IDAllocator.ItemTypesStart && enType < IDAllocator.ItemTypesEnd;
        }
    }
}
