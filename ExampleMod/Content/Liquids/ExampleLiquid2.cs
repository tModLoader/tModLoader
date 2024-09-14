using ExampleMod.Content.Items;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent.UI.Elements;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace ExampleMod.Content.Liquids
{
	public class ExampleLiquid2 : ModLiquid
	{
		public override void SetStaticDefaults() {
			base.SetStaticDefaults();
			WaterfallLength = 16;
			DefaultOpacity = 1f;
			AddMapEntry(Color.Black, Language.GetText("Mods.ExampleMod.Liquids.ExampleLiquid2.MapEntry"));
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

		public override void OnEntityCollision(Entity entity, CollisionType collisionType) {
			switch (entity) {
				case Item item:
					if (item.type == ItemID.WoodenSink) {
						for (int i = 0; i < 10; i++) {
							int num7 = Dust.NewDust(new Vector2(item.position.X - 6f, item.position.Y + (float)(item.height / 2) - 8f), item.width + 12, 24, DustID.FireworkFountain_Red);
							Main.dust[num7].velocity.Y -= 4f;
							Main.dust[num7].velocity.X *= 2.5f;
							Main.dust[num7].scale *= 0.8f;
							Main.dust[num7].alpha = 100;
							Main.dust[num7].noGravity = true;
						}

						CombatText.NewText(item.getRect(), Color.White, "Let that sink in!", true);
						SoundEngine.PlaySound(SoundID.DD2_GoblinBomb, item.position);

						item.TurnToAir();
					}
					return;
			}
			
			for (int i = 0; i < 20; i++) {
				int num7 = Dust.NewDust(new Vector2(entity.position.X - 6f, entity.position.Y + (float)(entity.height / 2) - 8f), entity.width + 12, 24, DustID.Bee);
				Main.dust[num7].velocity.Y -= 4f;
				Main.dust[num7].velocity.X *= 2.5f;
				Main.dust[num7].scale *= 0.8f;
				Main.dust[num7].alpha = 100;
				Main.dust[num7].noGravity = true;
			}
			SoundEngine.PlaySound(SoundID.Splash, entity.position);
			
		}

		public override void Merge(int otherLiquid, bool[] liquidNearby, ref int liquidMergeTileType, ref int liquidMergeType) {
			liquidMergeTileType = 0;


			if (liquidNearby[LiquidID.Shimmer]) {
				liquidMergeType = TileID.ShimmerBlock;
			} else if (liquidNearby[ModContent.GetInstance<ExampleLiquid>().Type]) {
				liquidMergeType = TileID.LunarOre;
			}
			
			liquidMergeType = 4;
		}
	}
}
