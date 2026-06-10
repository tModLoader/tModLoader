using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;
using static Terraria.WaterfallManager;

namespace ExampleMod.Content.Tiles
{
	//A customly drawn waterfall
	//Snow and Rain clouds both produce an effect where snow/rain will come out from beneath the tile.
	//What you might not have known is that, this effect is actually a waterfall (ID 11 and 22)
	//Here we do a similar effect with our own waterfall by drawing it differently to other waterfalls
	public class ExampleRainWaterfall : ModWaterfallStyle
	{
		//PreDraw allows us to redraw the waterfall entirely by returning false.
		//Here we copy some of the WaterfallManager.DrawWaterfalls to replicate the drawing of a snow/rain waterfall.
		public override bool PreDraw(WaterfallData currentWaterfallData, int i, int j, SpriteBatch spriteBatch) {
			if (Main.drewLava) //Some special waterfalls do not render if lava has not been rendered yet
			{
				return false;
			}
			//Although set to our current position now, 'y' is how we climb and find where to draw the next frame of the waterfall.
			int y = j;
			//If the waterfall is too long, we use the step counter to prevent it from rendering anymore (as well as apply a fade to the end)
			int step = Main.instance.waterfallManager.waterfallDist / 4;
			if (currentWaterfallData.stopAtStep > step) {
				currentWaterfallData.stopAtStep = step;
			}
			if (currentWaterfallData.stopAtStep == 0 || (y + step) < Main.screenPosition.Y / 16f || i < Main.screenPosition.X / 16f - 20f || i > (Main.screenPosition.X + Main.screenWidth) / 16f + 20f) {
				return false;
			}
			//get the frame count the waterfall should have.
			//here we get the rain frame counter
			int rainFrame;
			if (i % 2 == 0) {
				rainFrame = Main.wFallFrameBack[Slot] + 2;
				if (rainFrame > 7) {
					rainFrame -= 8;
				}
			}
			else {
				rainFrame = Main.wFallFrameBack[Slot];
			}
			//here we set the original settings for a few variables, as they are altered in the upcoming loop
			Rectangle frame = new((7 - rainFrame) * 18, 0, 16, 16);
			Vector2 position = (y % 2 != 0) ? (new Vector2(i * 16 + 8, y * 16 + 8) - Main.screenPosition) : (new Vector2(i * 16 + 9, y * 16 + 8) - Main.screenPosition);
			Tile tileAbove = Main.tile[i, y - 1];
			if (tileAbove.HasTile && tileAbove.BottomSlope) {
				position.Y -= 16f;
			}
			bool stopDraw = false;
			float rotation = 0f;
			//Loops over the avaliable distance the waterfall can cover
			for (int s = 0; s < step; s++) {
				//create the color the waterfall should draw at
				Color lightColor = Lighting.GetColor(i, y);
				float modAmount = 0.6f;
				if (s > step - 8) {
					modAmount *= (step - s) / 8f;
				}
				Color color = lightColor * modAmount;
				//render the waterfall segment
				Main.spriteBatch.Draw(ModContent.Request<Texture2D>(Texture).Value, position, (Rectangle?)frame, color, rotation, new(8f, 8f), 1f, 0, 0f);
				//if the waterfall has reached its maximum length
				if (stopDraw) {
					break;
				}
				//We then modify what frame and what position the waterfall draws at
				y++;
				Tile tile = Main.tile[i, y];
				if (WorldGen.SolidTile(tile)) {
					stopDraw = true; //If the next waterfall is in a solid tile, stop rendering
				}
				//To not have an akward cutoff when touching liquids or having waterfalls rendering in liquids themselves
				//we trim the height of the waterfall frame
				if (tile.LiquidAmount > 0) {
					int waterfallLength = (int)(16f * ((float)(int)tile.LiquidAmount / 255f)) & 0xFE;
					if (waterfallLength >= 15) {
						break;
					}
					frame.Height -= waterfallLength;
				}
				//finally, editing the position before looping again
				if (y % 2 == 0) {
					position.X += 1f;
				}
				else {
					position.X -= 1f;
				}
				position.Y += 16f;
			}
			currentWaterfallData.stopAtStep = 0;
			return false;
		}

		public override bool PlayWaterfallSounds() {
			return false;
		}

		//Here we animate our waterfall similarly to the rain waterfall
		//We use the unused frameBackground to get what normally would be the rain waterfall background's frame
		public override void AnimateWaterfall(ref int frame, ref int frameBackground, ref int frameCounter) {
			frameCounter++;
			if (frameCounter > 0) {
				frame++;
				if (frame > 7) {
					frame -= 8;
				}
				if (frameCounter > 2) {
					frameCounter = 0;
					frameBackground--;
					if (frameBackground < 0) {
						frameBackground = 7;
					}
				}
			}
		}
	}
}
