using ExampleMod.Content.Dusts;
using ExampleMod.Content.Tiles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria.Audio;
using Terraria.GameContent.Liquid;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExampleMod.Content.Liquids
{
	//An example of a basic modded liquid, a liquid that does nothing fancy
	//Just a basic liquid that is slightly opaque, doesnt drown the player, and has custom merging behavior
	//It is recommended that you look at ExampleLiquid to know exactly what everything does and how it contributes to the modded liquid 
	public class ExampleBasicLiquid : ModLiquid
	{
		public override void SetStaticDefaults() {
			LiquidRenderer.WATERFALL_LENGTH[Type] = 3;
			LiquidRenderer.DEFAULT_OPACITY[Type] = 0.75f;
			SlopeOpacity = 0.8f;
			LiquidRenderer.VISCOSITY_MASK[Type] = 200;
			FallDelay = 5;
			SplashDustType = ModContent.DustType<ExampleSolutionDust>();
			SplashSound = SoundID.Splash;
			ChecksForDrowning = false;
			AllowEmitBreathBubbles = false;
			AddMapEntry(new Color(80, 53, 80));
			//Please see ExampleFishingPlayer to see modded liquid fishing pool examples
		}

		public override int ChooseWaterfallStyle(int i, int j) {
			return ModContent.GetInstance<ExampleBasicLiquidfall>().Slot;
		}

		public override void RetroDrawEffects(int i, int j, SpriteBatch spriteBatch, ref RetroLiquidDrawInfo drawData, float liquidAmountModified, int liquidGFXQuality) {
			drawData.liquidAlphaMultiplier *= 1.5f;
			if (drawData.liquidAlphaMultiplier > 1f) {
				drawData.liquidAlphaMultiplier = 1f;
			}
		}

		public override int LiquidMerge(int i, int j, int otherLiquid) {
			if (otherLiquid == LiquidID.Water) {
				return ModContent.TileType<ExampleBlock>();
			}
			else if (otherLiquid == LiquidID.Honey) {
				return ModContent.TileType<ExampleOre>();
			}
			else if (otherLiquid == LiquidID.Shimmer) {
				return TileID.ShimmerBlock;
			}
			return TileID.Stone;
		}

		public override void LiquidMergeSound(int i, int j, int otherLiquid, ref SoundStyle? collisionSound) {
			collisionSound = SoundID.LiquidsHoneyWater;
			if (otherLiquid == LiquidID.Lava) {
				collisionSound = SoundID.LiquidsHoneyLava;
			}
			else if (otherLiquid == LiquidID.Shimmer) {
				collisionSound = SoundID.ShimmerWeak1;
			}
		}
	}
}