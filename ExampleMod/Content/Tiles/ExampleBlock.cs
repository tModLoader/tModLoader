using ExampleMod.Content.Biomes;
using ExampleMod.Content.Dusts;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExampleMod.Content.Tiles
{
	public class ExampleBlock : ModTile
	{
		public override void SetStaticDefaults() {
			Main.tileSolid[Type] = true;
			Main.tileMergeDirt[Type] = true;
			Main.tileBlockLight[Type] = true;

			MineResist = 1.5f;

			DustType = ModContent.DustType<Sparkle>();
			VanillaFallbackOnModDeletion = TileID.DiamondGemspark;

			AddMapEntry(new Color(200, 200, 200));
		}

		public override void NumDust(int i, int j, bool fail, ref int num) {
			num = fail ? 1 : 3;
		}

		public override void ChangeWaterfallStyle(ref int style) {
			style = ModContent.GetInstance<ExampleWaterfallStyle>().Slot;
		}

		public override bool DrawCracks(SpriteBatch spriteBatch, int i, int j, int damage, int crackStyle, bool isAnimation, int animationTimeElapsed) {
			Texture2D customCrackTexture = ModContent.Request<Texture2D>("ExampleMod/Content/Tiles/ExampleBlock_Cracks").Value;

			int frame = damage switch {
				>= 80 => 3,
				>= 60 => 2,
				>= 40 => 1,
				>= 20 => 0,
				_ => 0
			};

			int crackFrameHeight = customCrackTexture.Height / 4;
			Rectangle crackSourceRect = new(0, frame * crackFrameHeight, 16, 16);

            Color lightingColor = Lighting.GetColor(i, j);

			if (isAnimation) {
				Vector2 baseDrawPos = new Vector2(i * 16 - (int)Main.screenPosition.X, j * 16 - (int)Main.screenPosition.Y);
				Vector2 drawOrigin = new Vector2(8f, 8f);
				Vector2 finalDrawPosition = baseDrawPos + drawOrigin;

				spriteBatch.Draw(
					customCrackTexture,
					finalDrawPosition,
					crackSourceRect,
					Color.White,
					0f,
					drawOrigin,
					1.0f,
					SpriteEffects.None,
					0f
				);
			} else {
				Vector2 drawOffset = new Vector2(Main.offScreenRange, Main.offScreenRange);
				if (Main.drawToScreen) {
					drawOffset = Vector2.Zero;
				}
				Vector2 baseDrawPos = new Vector2(i * 16 - (int)Main.screenPosition.X, j * 16 - (int)Main.screenPosition.Y) + drawOffset;

				spriteBatch.Draw(
					customCrackTexture,
					baseDrawPos,
					crackSourceRect,
					lightingColor,
					0f,
					Vector2.Zero,
					1f,
					SpriteEffects.None,
					0f
				);
			}

			return true;
		}
	}
}