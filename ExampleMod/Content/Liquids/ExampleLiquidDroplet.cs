using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.GameContent.Shaders;
using Terraria.Graphics.Effects;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExampleMod.Content.Liquids
{
	//Very similar to Example Droplet,
	//except this droplet directly reimplements the movement that other droplets have to have the same animation speed as honey droplets,
	//while also giving off light like lava droplets
	public class ExampleLiquidDroplet : ModGore
	{
		public override void SetStaticDefaults() {
			ChildSafety.SafeGore[Type] = true;
			GoreID.Sets.LiquidDroplet[Type] = true;
		}

		public override bool Update(Gore gore) {
			if (GoreID.Sets.LiquidDroplet[Type]) {
				if (gore.position.Y < Main.worldSurface * 16.0 + 8.0) {
					gore.alpha = 0;
				}
				else {
					gore.alpha = 100;
				}
				int frameSpeed = 4;
				gore.frameCounter++;
				if (gore.frame <= 4) {
					int num3 = (int)(gore.position.X / 16f);
					int num4 = (int)(gore.position.Y / 16f) - 1;
					if (WorldGen.InWorld(num3, num4) && !Main.tile[num3, num4].HasTile) {
						gore.active = false;
					}
					if (gore.frame == 0) {
						frameSpeed = 24 + Main.rand.Next(256);
					}
					if (gore.frame == 1) {
						frameSpeed = 24 + Main.rand.Next(256);
					}
					if (gore.frame == 2) {
						frameSpeed = 24 + Main.rand.Next(256);
					}
					if (gore.frame == 3) {
						frameSpeed = 24 + Main.rand.Next(96);
					}
					if (gore.frame == 5) {
						frameSpeed = 16 + Main.rand.Next(64);
					}

					frameSpeed *= 4; //we multiply the speed by 4, to make the animation 4x slower

					if (gore.frameCounter >= frameSpeed) {
						gore.frameCounter = 0;
						gore.frame++;
						if (gore.frame == 5) {
							int num5 = Gore.NewGore(new EntitySource_Misc("SpawnFinalGoreFrames"), gore.position, gore.velocity, gore.type);
							Main.gore[num5].frame = 9;
							Gore obj = Main.gore[num5];
							obj.velocity *= 0f;
						}
					}
				}
				else if (gore.frame <= 6) {
					frameSpeed = 8;

					frameSpeed *= 3; //again, modify the speed to be 3x slower

					if (gore.frameCounter >= frameSpeed) {
						gore.frameCounter = 0;
						gore.frame++;
						if (gore.frame == 7) {
							gore.active = false;
						}
					}
				}
				else if (gore.frame <= 9) {
					frameSpeed = 6;

					frameSpeed *= 2; //modify to be 2x slower
					gore.velocity.Y += 0.15f;

					if ((double)gore.velocity.Y < 0.5) {
						gore.velocity.Y = 0.5f;
					}
					if (gore.velocity.Y > 12f) {
						gore.velocity.Y = 12f;
					}
					if (gore.frameCounter >= frameSpeed) {
						gore.frameCounter = 0;
						gore.frame++;
					}
					if (gore.frame > 9) {
						gore.frame = 7;
					}
				}
				else {
					frameSpeed *= 6; //lastly, modify the speed to be 6x slower

					gore.velocity.Y += 0.1f;
					if (gore.frameCounter >= frameSpeed) {
						gore.frameCounter = 0;
						gore.frame++;
					}
					gore.velocity *= 0f;
					if (gore.frame > 14) {
						gore.active = false;
					}
				}
			}

			if (GoreID.Sets.LiquidDroplet[Type]) {
				float liquidR = 1f;
				float liquidG = 1f;
				float liquidB = 1f;
				float lightModifier = 0.6f;
				lightModifier = ((gore.frame == 0) ? (lightModifier * 0.1f) : ((gore.frame == 1) ? (lightModifier * 0.2f) : ((gore.frame == 2) ? (lightModifier * 0.3f) : ((gore.frame == 3) ? (lightModifier * 0.4f) : ((gore.frame == 4) ? (lightModifier * 0.5f) :
					((gore.frame == 5) ? (lightModifier * 0.4f) : ((gore.frame == 6) ? (lightModifier * 0.2f) : ((gore.frame <= 9) ? (lightModifier * 0.5f) : ((gore.frame == 10) ? (lightModifier * 0.5f) : ((gore.frame == 11) ? (lightModifier * 0.4f) :
					((gore.frame == 12) ? (lightModifier * 0.3f) : ((gore.frame == 13) ? (lightModifier * 0.2f) : ((gore.frame != 14) ? 0f : (lightModifier * 0.1f))))))))))))));
				liquidR = 1f * lightModifier; //1f light R
				liquidG = 1f * lightModifier; //1f light G
				liquidB = 1f * lightModifier; //1f light B
				Lighting.AddLight(gore.position + new Vector2(8f, 8f), liquidR, liquidG, liquidB);

				Vector2 velo = gore.velocity;
				gore.velocity = Collision.TileCollision(gore.position, gore.velocity, 16, 14);
				if (gore.velocity != velo) {
					if (gore.frame < 10) {
						gore.frame = 10;
						gore.frameCounter = 0;

						SoundEngine.PlaySound(SoundID.Drip, new Vector2((int)gore.position.X + 8, (int)gore.position.Y + 8));
					}
				}
				else if (Collision.WetCollision(gore.position + gore.velocity, 16, 14)) {
					if (gore.frame < 10) {
						gore.frame = 10;
						gore.frameCounter = 0;

						SoundEngine.PlaySound(SoundID.Drip, new Vector2((int)gore.position.X + 8, (int)gore.position.Y + 8));

						((WaterShaderData)Filters.Scene["WaterDistortion"].GetShader()).QueueRipple(gore.position + new Vector2(8f, 8f));
					}
					int posX = (int)(gore.position.X + 8f) / 16;
					int posY = (int)(gore.position.Y + 14f) / 16;
					if (Main.tile[posX, posY].LiquidAmount > 0) {
						gore.velocity *= 0f;
						gore.position.Y = posY * 16 - Main.tile[posX, posY].LiquidAmount / 16;
					}
				}
			}

			gore.position += gore.velocity;

			if (gore.alpha >= 255) {
				gore.active = false;
			}
			return false;
		}

		public override Color? GetAlpha(Gore gore, Color lightColor) {
			return new Color(255, 255, 255, 200);
		}
	}
}
