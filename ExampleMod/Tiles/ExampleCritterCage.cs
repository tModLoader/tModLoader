using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;
using static Terraria.ModLoader.ModContent;

namespace ExampleMod.Tiles
{
	class ExampleCritterCage : ModTile
	{
		public override void SetDefaults() {
			Main.tileFrameImportant[Type] = true;
			Main.tileLighted[Type] = true;
			Main.tileLavaDeath[Type] = true;

			// The larger cage uses Style6x3.
			TileObjectData.newTile.CopyFrom(TileObjectData.StyleSmallCage);
			TileObjectData.addTile(Type);

			animationFrameHeight = 36;

			ModTranslation name = CreateMapEntryName();
			name.SetDefault("Lava Snail Cage");
			AddMapEntry(new Color(122, 217, 232), name);
		}

		public override void KillMultiTile(int i, int j, int frameX, int frameY) {
			Item.NewItem(i * 16, j * 16, 48, 32, ItemType<ExampleCritterCageItem>());
		}

		public override void AnimateIndividualTile(int type, int i, int j, ref int frameXOffset, ref int frameYOffset) {
			// This code utilizes some math to stagger each individual tile. First the top left tile is found, then those coordinates are passed into some math to stagger an index into Main.snail2CageFrame
			// Main.snail2CageFrame is used since we want the same animation, but if we wanted a different frame count or a different animation timing, we could write our own by adapting vanilla code and placing the code in AnimateTile
			Tile tile = Main.tile[i, j];
			Main.critterCage = true;
			int left = i - tile.frameX / 18;
			int top = j - tile.frameY / 18;
			int offset = left / 3 * (top / 3);
			offset %= Main.cageFrames;
			frameYOffset = Main.snail2CageFrame[offset] * animationFrameHeight;
		}
	}

	internal class ExampleCritterCageItem : ModItem
	{
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Lava Snail Cage");
		}

		public override void SetDefaults() {
			//item.useStyle = 1;
			//item.useTurn = true;
			//item.useAnimation = 15;
			//item.useTime = 10;
			//item.autoReuse = true;
			//item.maxStack = 99;
			//item.consumable = true;
			//item.createTile = 285 + type - 2174;
			//item.width = 12;
			//item.height = 12;

			item.CloneDefaults(ItemID.GlowingSnailCage);
			item.createTile = TileType<ExampleCritterCage>();
		}

		public override void AddRecipes() {
			ModRecipe recipe = new ModRecipe(mod);
			recipe.AddIngredient(ItemID.Terrarium);
			recipe.AddIngredient(ItemType<NPCs.ExampleCritterItem>());
			recipe.SetResult(this);
			recipe.AddRecipe();
		}
	}
}
