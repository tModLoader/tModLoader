using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExampleMod.Common.Players
{
	public class ExamplePickTilePlayer : ModPlayer
	{
		public override void ModifyPickWall(IEntitySource source, int x, int y, ref int damage) {
			base.ModifyPickWall(source, x, y, ref damage);
			// If the destruction originates from an item and the item is a wooden hammer, then set the damage to 999.
			if (source is EntitySource_ItemUse itemSource) {
				if(itemSource.Item.type == ItemID.WoodenHammer) {
					damage = 999;
				}
			}

			// If the destruction originates from a drill, then set the damage to 1, making it difficult to destroy walls.
			if (source is EntitySource_Mount mountSource) {
				if (mountSource.MountId == MountID.Drill) {
					damage = 1; // modify drill damage to 1
				}
			}
		}

		public override void ModifyPickTile(IEntitySource source, int x, int y, ref int pick) {
			base.ModifyPickTile(source, x, y, ref pick);
			// if item is copperpickaxe then modify pick to 999
			// So he could dig up everything
			if(source is EntitySource_ItemUse itemSource) {
				if (itemSource.Item.type == ItemID.CopperPickaxe) {
					pick = 999;
				}
			}

			if(source is EntitySource_Mount mountSource) {
				if(mountSource.MountId == MountID.Drill) {
					pick = 1; // modify drill pick to 1
				}
			}

			if(Main.tile[x, y].TileType == TileID.Stone) {
				pick = 1; // If it's a Stone Block, make it extremely difficult to mine.
			}
		}
	}
}
