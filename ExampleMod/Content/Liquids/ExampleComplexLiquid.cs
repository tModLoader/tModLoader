using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent.Liquid;
using Terraria.Graphics;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.GameContent.Liquid.LiquidRenderer;

namespace ExampleMod.Content.Liquids
{
	//An example of a more complex modded liquid
	//This liquid both has custom merging, custom rendering and more
	//It is recommended that you look at ExampleLiquid which is a lesser complex liquid to help understand how smaller things works
	public class ExampleComplexLiquid : ModLiquid
	{
		public override void SetStaticDefaults() {
			LiquidRenderer.WATERFALL_LENGTH[Type] = 3;
			LiquidRenderer.DEFAULT_OPACITY[Type] = 0.75f;
			SlopeOpacity = 0.8f;
			LiquidRenderer.VISCOSITY_MASK[Type] = 200;
			FallDelay = 5;
			SplashSound = SoundID.Splash;
			ChecksForDrowning = false;
			AllowEmitBreathBubbles = false;
			AddMapEntry(new Color(55, 35, 200));
		}

		//For our custom liquid, we want it to explode when interacting with any other liquid
		//Here we use PreLiquidMerge to both spawn an explosion as well as remove liquid in a radious
		public override bool PreLiquidMerge(int liquidX, int liquidY, int tileX, int tileY, int otherLiquid) {
			//Firstly, we remove all liquid in a 3.5f area using the same Util's method that the dry bomb uses
			if (Main.netMode != NetmodeID.MultiplayerClient) {
				int y = liquidY;
				Tile tile = Main.tile[liquidX, liquidY];
				//We grab the tile above to explode at by checking if the tile is not solid or sloped
				if (tile.HasTile && tile.IsHalfBlock) {
					int yPos = y - 1;
					if (yPos >= 0) {
						tile = Main.tile[liquidX, yPos];
						if (!WorldGen.SolidOrSlopedTile(tile)) {
							y--;
						}
					}
				}
				//We then call the same method dry bombs call for removing liquid
				DelegateMethods.v2_1 = new Vector2(liquidX, y);
				DelegateMethods.f_1 = 3.5f;
				Utils.PlotTileArea(liquidX, y, DelegateMethods.SpreadDry);
			}
			//Here we call the visual effects for the explosion
			//We seperate this into 2 as we need to directly call the visuals if on single player and send through a packet calling the explosion in multiplayer
			//This method (PreLiquidMerge) is called only on servers, which is why we need to send a packet to sync data between every client
			if (Main.netMode == NetmodeID.SinglePlayer) {
				SpawnVisualExplosion(liquidX, liquidY);
			}
			if (Main.netMode == NetmodeID.Server) {
				ModPacket packet = ModContent.GetInstance<ExampleMod>().GetPacket();
				packet.Write((byte)ExampleMod.MessageType.LiquidMergeExplosion);
				packet.Write(liquidX);
				packet.Write(liquidY);
				packet.Send();
			}
			return false;
		}

		//Here are the visuals for the explosions, spawning smoke dusts and gores as well as playing an explosion sound
		public static void SpawnVisualExplosion(int x, int y) {
			Vector2 position = new Vector2(x * 16, y * 16);
			SoundEngine.PlaySound(SoundID.Item14, position);
			int dustType = DustID.Smoke;
			for (int i = 0; i < 30; i++) {
				Dust dust = Dust.NewDustDirect(position, 22, 22, dustType, 0f, 0f, 100, Color.Transparent, 1.5f);
				dust.velocity *= 1.4f;
			}
			for (int i = 0; i < 80; i++) {
				Dust dust = Dust.NewDustDirect(position, 22, 22, dustType, 0f, 0f, 100, Color.Transparent, 1.2f);
				dust.velocity *= 7f;
				dust = Dust.NewDustDirect(position, 22, 22, dustType, 0f, 0f, 100, Color.Transparent, 0.3f);
				dust.velocity *= 4f;
			}
			for (int i = 1; i <= 2; i++) {
				for (int j = -1; j <= 1; j += 2) {
					for (int k = -1; k <= 1; k += 2) {
						Gore gore = Gore.NewGoreDirect(new EntitySource_Misc("Liquid Explosion"), position, Vector2.Zero, Main.rand.Next(GoreID.Smoke1, GoreID.Smoke3 + 1));
						gore.velocity *= ((i == 1) ? 0.4f : 0.8f);
						gore.velocity += new Vector2(j, k);
					}
				}
			}
		}

		//Here we redraw our liquid completely to add both an addition stary overlay as well as to color the liquid rainbow
		public override bool PreDraw(int i, int j, LiquidDrawCache liquidDrawCache, Vector2 drawOffset, bool isBackgroundDraw) {
			Rectangle sourceRectangle = liquidDrawCache.SourceRectangle;
			if (liquidDrawCache.IsSurfaceLiquid)
				sourceRectangle.Y = 1280;
			else
				sourceRectangle.Y += Main.liquidFrame[Type] * 80;

			Vector2 liquidOffset = liquidDrawCache.LiquidOffset;
			float opacity = liquidDrawCache.Opacity * (isBackgroundDraw ? 1f : DEFAULT_OPACITY[Type]);
			opacity = Math.Min(1f, opacity);
			Lighting.GetCornerColors(i, j, out var vertices);
			//Everything before here is the same as vanilla when it comes to rendering a liquid
			//Here we change the verticies' both opacity and color with our seperate method 
			SetComplexLiquidVertexColors(ref vertices, opacity);
			//DrawTileInWater allows tiles like lilypads to draw to the water (and attatch itself to the water ripple shader)
			//Make sure this gets called so both vanilla and modded tiles that render in water can draw
			Main.DrawTileInWater(drawOffset, i, j);
			Main.tileBatch.Draw(LiquidLoader.LiquidAssets[Type].Value, new Vector2(i << 4, j << 4) + drawOffset + liquidOffset, sourceRectangle, vertices, Vector2.Zero, 1f, SpriteEffects.None);
			//The source rectangle here is reset so only the first frame of the liquid is drawn, this is so our extra texture overlay doesnt animate alongside our initial texture
			sourceRectangle = liquidDrawCache.SourceRectangle;
			//The overlay is actually positioned on the liquid texture but 48 pixels to the right, here we offset the X by 48 so only the overlay is rendered from here on
			sourceRectangle.X += 48;
			Lighting.GetCornerColors(i, j, out var vertices2);
			//opacity is tuned down by half so the overlay is less visible
			opacity /= 2;
			vertices2.BottomLeftColor *= opacity;
			vertices2.BottomRightColor *= opacity;
			vertices2.TopLeftColor *= opacity;
			vertices2.TopRightColor *= opacity;
			//Render the overlay
			Main.tileBatch.Draw(LiquidLoader.LiquidAssets[Type].Value, new Vector2(i << 4, j << 4) + drawOffset + liquidOffset, sourceRectangle, vertices2, Vector2.Zero, 1f, SpriteEffects.None);
			return false;
		}

		//Modifies the vertecies to be rainbow as well as adding the opacity of the liquid
		public static void SetComplexLiquidVertexColors(ref VertexColors colors, float opacity) {
			colors.BottomLeftColor = Color.White;
			colors.BottomRightColor = Color.White;
			colors.TopLeftColor = Color.White;
			colors.TopRightColor = Color.White;
			colors.BottomLeftColor *= opacity;
			colors.BottomRightColor *= opacity;
			colors.TopLeftColor *= opacity;
			colors.TopRightColor *= opacity;
			colors.BottomLeftColor = new Color(colors.BottomLeftColor.ToVector4() * Main.DiscoColor.ToVector4());
			colors.BottomRightColor = new Color(colors.BottomRightColor.ToVector4() * Main.DiscoColor.ToVector4());
			colors.TopLeftColor = new Color(colors.TopLeftColor.ToVector4() * Main.DiscoColor.ToVector4());
			colors.TopRightColor = new Color(colors.TopRightColor.ToVector4() * Main.DiscoColor.ToVector4());
		}


		//Here, using RetroDrawEffects, we edit the color of the liquid in the retro lightmode to also be rainbow
		//We don't do anything too fancy for Retro rendering, but Pre/PostRetroDraw can also be used similarly to Predraw to render the liquid completely differently in the retro lightmode
		//RetroLiquidDrawInfo has a series of different properties that can be useful for different rendering use cases
		public override void RetroDrawEffects(int i, int j, SpriteBatch spriteBatch, ref RetroLiquidDrawInfo drawData, float liquidAmountModified, int liquidGFXQuality) {
			drawData.liquidColor = Main.DiscoColor;
		}

		//Similarly to RetroDrawEffects, we use PreSlopeDraw to edit the rendering of slopes to also modify the color to be rainbow too
		//As of oct 1st 2025, slopes are only really used for retro rendering so we only change the color simialrly to retro lighting to make slopes not look out of place
		public override bool PreSlopeDraw(int i, int j, bool behindBlocks, ref Vector2 drawPosition, ref Rectangle liquidSize, ref VertexColors colors) {
			SetComplexLiquidVertexColors(ref colors, 1f);
			return true;
		}

		//Here with UpdateLiquid, we use this method to make our liquid evaporate when in the ocean biome
		public override bool UpdateLiquid(int i, int j, Liquid liquid) {
			//Firstly we check if the liquid both has more than 0 liquid amount, and is in the ocean biome part of the world
			if ((i < WorldGen.beachDistance || i > Main.maxTilesX - WorldGen.beachDistance) && Main.tile[i, j].LiquidAmount > 0) {
				byte evaporateAmount = 2; //the amount of liquid removed each frame
				if (Main.tile[i, j].LiquidAmount < evaporateAmount) 
					evaporateAmount = Main.tile[i, j].LiquidAmount; //make sure we don't end up putting the liquid amount in the negitives

				Main.tile[i, j].LiquidAmount -= evaporateAmount; //remove the amount of liquid from the tile
			}
			return true; //since we dont reimplement the liquid's movement, we just return true so the normal liquid updating can also run as well
		}

		//Used to color the splash to be rainbow, Please see ExampleLiquid's splash comments for more info
		public override bool OnPlayerSplash(Player player, bool isEnter) {
			for (int i = 0; i < 20; i++) {
				int dust = Dust.NewDust(new Vector2(player.position.X - 6f, player.position.Y + (player.height / 2) - 8f), player.width + 12, 24, DustID.RainbowTorch);
				Main.dust[dust].velocity.Y -= 1f;
				Main.dust[dust].velocity.X *= 2.5f;
				Main.dust[dust].scale = 1.3f;
				Main.dust[dust].alpha = 100;
				Main.dust[dust].noGravity = true;
				Main.dust[dust].color = Main.DiscoColor;
			}
			SoundEngine.PlaySound(SplashSound, player.position);
			return false;
		}

		public override bool OnNPCSplash(NPC npc, bool isEnter) {
			for (int i = 0; i < 10; i++) {
				int dust = Dust.NewDust(new Vector2(npc.position.X - 6f, npc.position.Y + (npc.height / 2) - 8f), npc.width + 12, 24, DustID.RainbowTorch);
				Main.dust[dust].velocity.Y -= 1f;
				Main.dust[dust].velocity.X *= 2.5f;
				Main.dust[dust].scale = 1.3f;
				Main.dust[dust].alpha = 100;
				Main.dust[dust].noGravity = true;
				Main.dust[dust].color = Main.DiscoColor;
			}
			if (npc.aiStyle != NPCAIStyleID.Slime &&
					npc.type != NPCID.BlueSlime && npc.type != NPCID.MotherSlime && npc.type != NPCID.IceSlime && npc.type != NPCID.LavaSlime &&
					npc.type != NPCID.Mouse &&
					npc.aiStyle != NPCAIStyleID.GiantTortoise &&
					!npc.noGravity) {
				SoundEngine.PlaySound(SplashSound, npc.position);
			}
			return false;
		}

		public override bool OnProjectileSplash(Projectile proj, bool isEnter) {
			for (int i = 0; i < 10; i++) {
				int dust = Dust.NewDust(new Vector2(proj.position.X - 6f, proj.position.Y + (proj.height / 2) - 8f), proj.width + 12, 24, DustID.RainbowTorch);
				Main.dust[dust].velocity.Y -= 1f;
				Main.dust[dust].velocity.X *= 2.5f;
				Main.dust[dust].scale = 1.3f;
				Main.dust[dust].alpha = 100;
				Main.dust[dust].noGravity = true;
				Main.dust[dust].color = Main.DiscoColor;
			}
			SoundEngine.PlaySound(SplashSound, proj.position);
			return false;
		}

		public override bool OnItemSplash(Item item, bool isEnter) {
			for (int i = 0; i < 5; i++) {
				int dust = Dust.NewDust(new Vector2(item.position.X - 6f, item.position.Y + (item.height / 2) - 8f), item.width + 12, 24, DustID.RainbowTorch);
				Main.dust[dust].velocity.Y -= 1f;
				Main.dust[dust].velocity.X *= 2.5f;
				Main.dust[dust].scale = 1.3f;
				Main.dust[dust].alpha = 100;
				Main.dust[dust].noGravity = true;
				Main.dust[dust].color = Main.DiscoColor;
			}
			SoundEngine.PlaySound(SplashSound, item.position);
			return false;
		}

		public override int ChooseWaterfallStyle(int i, int j) {
			return ModContent.GetInstance<ExampleComplexLiquidfall>().Slot;
		}
	}
}
