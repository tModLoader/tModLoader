using ExampleMod.Content.Items;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExampleMod.Content.Liquids
{
	public class ExampleLiquid : ModLiquid
	{
		public override void SetStaticDefaults() {
			base.SetStaticDefaults();
			WaterfallLength = 16;
			DefaultOpacity = 0.6f;
		}

		public override void PreUpdate(int x, int y) {
			Tile tile = Main.tile[x, y];

			if (y < 200 && tile.LiquidAmount > 0) {
				byte b = 2;
				if (tile.LiquidAmount < b)
					b = tile.LiquidAmount;

				tile.LiquidAmount -= b;
			}
		}

		public override void OnItemCollide(Item item) {
			if (item.type == ItemID.DirtBlock)
			{
				for (int i = 0; i < 10; i++) {
					int num7 = Dust.NewDust(new Vector2(item.position.X - 6f, item.position.Y + (float)(item.height / 2) - 8f), item.width + 12, 24, DustID.FireworkFountain_Red);
					Main.dust[num7].velocity.Y -= 4f;
					Main.dust[num7].velocity.X *= 2.5f;
					Main.dust[num7].scale *= 0.8f;
					Main.dust[num7].alpha = 100;
					Main.dust[num7].noGravity = true;
				}
				

				SoundEngine.PlaySound(SoundID.DD2_GoblinBomb, item.position);

				item.SetDefaults(ModContent.ItemType<ExampleItem>());
			}
			else {
				for (int i = 0; i < 20; i++) {
					int num7 = Dust.NewDust(new Vector2(item.position.X - 6f, item.position.Y + (float)(item.height / 2) - 8f), item.width + 12, 24, DustID.Bee);
					Main.dust[num7].velocity.Y -= 4f;
					Main.dust[num7].velocity.X *= 2.5f;
					Main.dust[num7].scale *= 0.8f;
					Main.dust[num7].alpha = 100;
					Main.dust[num7].noGravity = true;
				}
				SoundEngine.PlaySound(SoundID.Splash, item.position);
			}

			base.OnItemCollide(item);
		}

		public override void Merge(int otherLiquid, bool[] liquidNearby, ref int liquidMergeTileType, ref int liquidMergeType) {
			liquidMergeTileType = 0;

			if (otherLiquid == ModContent.GetInstance<ExampleLiquid2>().Type) {
				liquidMergeTileType = TileID.LunarOre;
			}
			else if (otherLiquid == LiquidID.Shimmer) {
				liquidMergeTileType = TileID.ShimmerBlock;
			}

			liquidMergeType = 4;
		}
	}
}
