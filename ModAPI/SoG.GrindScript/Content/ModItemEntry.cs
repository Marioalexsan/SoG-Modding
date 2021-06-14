using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace SoG.Modding
{
    public static partial class SoGExtension
    {
        public static Item SpawnItem(this ItemCodex.ItemTypes enType, PlayerView xTarget)
        {
            PlayerEntity xEntity = xTarget.xEntity;

            return enType.SpawnItem(xEntity.xTransform.v2Pos, xEntity.xRenderComponent.fVirtualHeight, xEntity.xCollisionComponent.ibitCurrentColliderLayer);
        }

        public static Item SpawnItem(this ItemCodex.ItemTypes enType, Vector2 v2Pos, float fVirtualHeight, int iColliderLayer)
        {
            Vector2 v2ThrowDirection = Utility.RandomizeVector2Direction(CAS.RandomInLogic);

            return GrindScript.Game._EntityMaster_AddItem(enType, v2Pos, fVirtualHeight, iColliderLayer, v2ThrowDirection);
        }

        public static bool IsSoGItem(this ItemCodex.ItemTypes enType)
        {
            return Enum.IsDefined(typeof(ItemCodex.ItemTypes), enType);
        }

        public static bool IsModItem(this ItemCodex.ItemTypes enType)
        {
            return enType >= ModLibrary.ItemTypesStart && enType < ModLibrary.ItemTypesNext;
        }
    }

    internal class ModItemEntry
    {
        public BaseScript owner;

        public ItemCodex.ItemTypes type;

        public ModItem itemInfo;

        public ModEquip equipInfo;
    }

    internal class ModItem
    {
        public ItemDescription vanilla;

        public string shadowToUse = "";

        public string resourceToUse = "";

        public ContentManager managerToUse;
    }

    internal class ModEquip
    {
        public ItemCodex.ItemTypes type;

        public string resourceToUse = "";

        public EquipmentInfo vanilla;

        public ContentManager managerToUse;

        public Dictionary<ItemCodex.ItemTypes, string> hatAltSetResources;

        public ModEquipType equipType;
    }
}
