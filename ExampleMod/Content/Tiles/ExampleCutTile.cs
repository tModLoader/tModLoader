using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace ExampleMod.Content.Tiles
{
	// This example shows how to have a tile that is cut by weapons, like vines and grass.
	// This example also shows how to spawn a projectile on death like Beehive and Boulder trap.
	public class ExampleCutTile : ModTile
	{
		public override void SetStaticDefaults() {
			Main.tileFrameImportant[Type] = true;
			Main.tileCut[Type] = true;

			TileObjectData.newTile.CopyFrom(TileObjectData.Style3x3);
			// We need to change the 3x3 default to allow only placement anchored to top rather than on bottom. Also, the 1,1 means that only the middle tile needs to attach
			TileObjectData.newTile.AnchorTop = new AnchorData(AnchorType.SolidTile | AnchorType.SolidSide, 1, 1);
			TileObjectData.newTile.AnchorBottom = AnchorData.Empty;
			// This is so we can place from above.
			TileObjectData.newTile.Origin = new Point16(1, 0);
			TileObjectData.newTile.DrawYOffset = -2;
			TileObjectData.addTile(Type);
		}

		public override bool IsTileDangerous(int i, int j, Player player) => true;

		public override bool CreateDust(int i, int j, ref int type) => false;

		public override void KillMultiTile(int i, int j, int frameX, int frameY) {
			if (!WorldGen.gen && Main.netMode != NetmodeID.MultiplayerClient) {
				var multitileCenter = new Vector2((i + 1.5f) * 16f, (j + 1.5f) * 16f);
				Projectile.NewProjectile(WorldGen.GetItemSource_FromTileBreak(i, j), multitileCenter, Vector2.Zero, ProjectileID.Boulder, 70, 10f, Main.myPlayer);
			}
		}
	}
}
