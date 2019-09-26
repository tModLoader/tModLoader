using Terraria;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;
using static Terraria.ModLoader.ModContent;

namespace ExampleMod.Tiles
{
	// This example shows how to have a tile that is cut by weapons, like vines and grass.
	// This example also shows how to spawn a projectile on death like Beehive and Boulder trap.
	internal class ExampleCutTileTile : ModTile
	{
		public override void SetDefaults() {
			Main.tileFrameImportant[Type] = true;
			Main.tileCut[Type] = true;

			TileObjectData.newTile.CopyFrom(TileObjectData.Style3x3);
			// We need to change the 3x3 default to allow only placement anchored to top rather than on bottom. Also, the 1,1 means that only the middle tile needs to attach
			TileObjectData.newTile.AnchorTop = new AnchorData(AnchorType.SolidTile | AnchorType.SolidSide, 1, 1);
			TileObjectData.newTile.AnchorBottom = AnchorData.Empty;
			// This is so we can place from above.
			TileObjectData.newTile.Origin = new Point16(1, 0);
			TileObjectData.addTile(Type);
		}

		public override bool Dangersense(int i, int j, Player player) {
			return true;
		}

		public override bool CreateDust(int i, int j, ref int type) {
			return false;
		}

		public override void KillMultiTile(int i, int j, int frameX, int frameY) {
			//Projectile.NewProjectile((float)(k * 16) + 15.5f, (float)(num4 * 16 + 16), 0f, 0f, 99, 70, 10f, Main.myPlayer, 0f, 0f);
			if (!WorldGen.gen && Main.netMode != 1) {
				Projectile.NewProjectile((i + 1.5f) * 16f, (j + 1.5f) * 16f, 0f, 0f, ProjectileID.Boulder, 70, 10f, Main.myPlayer, 0f, 0f);
			}

			//Item.NewItem(i * 16, j * 16, 48, 48, ItemType<ExampleCutTileItem>());
		}
	}

	internal class ExampleCutTileItem : ModItem
	{
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Fragile Boulder Trap");
		}

		public override void SetDefaults() {
			item.CloneDefaults(ItemID.DartTrap);
			item.createTile = TileType<ExampleCutTileTile>();
			item.value = 1000;
		}

		public override void AddRecipes() {
			ModRecipe recipe = new ModRecipe(mod);
			recipe.AddIngredient(ItemID.StoneBlock, 10);
			recipe.AddIngredient(ItemID.Rope, 10);
			recipe.AddTile(TileID.HeavyWorkBench);
			recipe.SetResult(this);
			recipe.AddRecipe();
		}
	}
}
