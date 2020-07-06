using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExampleMod.NPCs
{
	// This example is very advanced. It shows how to use shaders, manual NPC drawing, CheckDead usage, and making a death animation. It also has a fairly basic custom AI that acts fairly similar to the Dungeon Guardian.
	// Watch this for a quick demonstration of the death animation and shader: https://gfycat.com/SlowUnimportantFlea
	public class DeathAnimation : ModNPC
	{
		public override void SetStaticDefaults() {
			Main.npcFrameCount[npc.type] = 2;
		}

		public override void SetDefaults() {
			npc.width = 120;
			npc.height = 120;
			npc.aiStyle = -1;
			npc.damage = 10;
			npc.defense = 2;
			npc.lifeMax = 100;
			npc.HitSound = SoundID.NPCHit2;
			npc.DeathSound = SoundID.NPCDeath2;
			npc.noGravity = true;
			npc.noTileCollide = true;
			npc.knockBackResist = 0f;

			// Because our width and height don't match the texture size, we use drawOffsetY to attempt to center the drawing of the NPC. This lets the hitbox better conform to the shape of our NPC. Hitboxes don't rotate, so this approach is needed to let the hitbox better represent the position of the damageable portion of the NPC.
			drawOffsetY = 30;
		}

		public override float SpawnChance(NPCSpawnInfo spawnInfo) {
			// Spawn this NPC with something like Cheat Sheet or Hero's Mod
			return 0f;
		}

		// PreDraw and PostDraw are responsible for applying and then removing the shader. If you omit PostDraw, the following NPC to be drawn will inherit the shader, so don't do that.
		// Basically, we need to End the previous spriteBatch, start it again, apply our shader, draw the NPC, and finally End and Start a fresh spriteBatch.
		public override bool PreDraw(SpriteBatch spriteBatch, Color drawColor) {
			Main.spriteBatch.End();
			Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.Default, RasterizerState.CullNone, null, Main.GameViewMatrix.ZoomMatrix);

			// Retrieve reference to shader
			var deathShader = GameShaders.Misc["ExampleMod:DeathAnimation"];

			// Reset back to default value.
			deathShader.UseOpacity(1f);
			// We use npc.ai[3] as a counter since the real death.
			if (npc.ai[3] > 30f) {
				// Our shader uses the Opacity register to drive the effect. See ExampleEffectDeath.fx to see how the Opacity parameter factors into the shader math. 
				deathShader.UseOpacity(1f - (npc.ai[3] - 30f) / 150f);
			}
			// Call Apply to apply the shader to the SpriteBatch. Only 1 shader can be active at a time.
			deathShader.Apply(null);
			return true;
		}

		public override void PostDraw(SpriteBatch spriteBatch, Color drawColor) {
			// As mentioned above, be sure not to forget this step.
			Main.spriteBatch.End();
			Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullCounterClockwise, null, Main.GameViewMatrix.TransformationMatrix);
		}

		// We use CheckDead to delay death providing time for our death drama to happen. The logic here is a bit complicated, if you are curious, please step through AI and CheckDead in Visual Studio to see how CheckDead prevents death the first time but allows it after the death drama has finished.
		public override bool CheckDead() {
			if (npc.ai[3] == 0f) {
				npc.ai[3] = 1f;
				npc.damage = 0;
				npc.life = npc.lifeMax;
				npc.dontTakeDamage = true;
				npc.netUpdate = true;
				return false;
			}
			return true;
		}

		public override void FindFrame(int frameHeight) {
			if (npc.ai[3] > 0f) {
				// If our ModNPC is currently dying, show the sad face part of the sprite.
				npc.frame.Y = frameHeight;
			}
		}

		// This AI was adapted from the Dungeon Guardian AI and then modified. Below are some notes I used while figuring out what each ai value represented. It is a good skill to develop if you wish to make AI.
		// npc.ai[0]: Just Spawned
		//		0: just spawned, needs target
		//		1: has target
		// npc.ai[1]:
		//		0: No roar yet
		//		2: played roar, chasing
		//		3: Retreat Down
		// npc.ai[2]: Not Used
		// npc.ai[3]:
		//		0: death drama not activated
		//		>0: dying animation timer
		public override void AI() {
			// death drama
			if (npc.ai[3] > 0f) {
				npc.dontTakeDamage = true;
				npc.ai[3] += 1f; // increase our death timer.
								//npc.velocity = Vector2.UnitY * npc.velocity.Length();
				npc.velocity.X *= 0.95f; // lose inertia
				if (npc.velocity.Y < 0.5f) {
					npc.velocity.Y = npc.velocity.Y + 0.02f;
				}
				if (npc.velocity.Y > 0.5f) {
					npc.velocity.Y = npc.velocity.Y - 0.02f;
				}
				if (npc.ai[3] > 120f) {
					//		npc.Opacity = 1f - (npc.ai[3] - 120f) / 60f;
				}
				if (Main.rand.NextBool(5) && npc.ai[3] < 120f) {
					// This dust spawn adapted from the Pillar death code in vanilla.
					for (int dustNumber = 0; dustNumber < 3; dustNumber++) {
						Dust dust = Main.dust[Dust.NewDust(npc.Left, npc.width, npc.height / 2, 242, 0f, 0f, 0, default(Color), 1f)];
						dust.position = npc.Center + Vector2.UnitY.RotatedByRandom(4.1887903213500977) * new Vector2(npc.width * 1.5f, npc.height * 1.1f) * 0.8f * (0.8f + Main.rand.NextFloat() * 0.2f);
						dust.velocity.X = 0f;
						dust.velocity.Y = -Math.Abs(dust.velocity.Y - (float)dustNumber + npc.velocity.Y - 4f) * 3f;
						dust.noGravity = true;
						dust.fadeIn = 1f;
						dust.scale = 1f + Main.rand.NextFloat() + (float)dustNumber * 0.3f;
					}
				}

				if (npc.ai[3] % 60f == 1f) {
					//SoundEngine.PlaySound(4, npc.Center, 22);
					SoundEngine.PlaySound(SoundID.NPCDeath22, npc.Center); // every second while dying, play a sound
				}
				if (npc.ai[3] >= 180f) {
					npc.life = 0;
					npc.HitEffect(0, 0);
					npc.checkDead(); // This will trigger ModNPC.CheckDead the second time, causing the real death.
				}
				return;
			}

			// Below this point is the normal AI code.
			if (npc.ai[0] == 0f && Main.netMode != NetmodeID.MultiplayerClient) {
				npc.TargetClosest(true);
				npc.ai[0] = 1f;
			}
			if (npc.ai[1] != 3f && npc.ai[1] != 2f) {
				SoundEngine.PlaySound(SoundID.Roar, (int)npc.position.X, (int)npc.position.Y, 0, 1f, 0f);
				npc.ai[1] = 2f;
			}
			if (Main.player[npc.target].dead || Math.Abs(npc.position.X - Main.player[npc.target].position.X) > 2000f || Math.Abs(npc.position.Y - Main.player[npc.target].position.Y) > 2000f) {
				npc.TargetClosest(true);
				if (Main.player[npc.target].dead || Math.Abs(npc.position.X - Main.player[npc.target].position.X) > 2000f || Math.Abs(npc.position.Y - Main.player[npc.target].position.Y) > 2000f) {
					npc.ai[1] = 3f;
				}
			}
			if (npc.ai[1] == 2f) {
				npc.rotation += npc.direction * 0.03f;
				if (Vector2.Distance(Main.player[npc.target].Center, npc.Center) > 250) {
					//npc.velocity += (Main.player[npc.target].Center - npc.Center) * new Vector2(.3f, .1f);
					npc.velocity += Vector2.Normalize(Main.player[npc.target].Center - npc.Center) * new Vector2(.3f, .1f);
				}

				npc.velocity *= 0.98f;
				npc.velocity.X = Utils.Clamp(npc.velocity.X, -4, 4);
				npc.velocity.Y = Utils.Clamp(npc.velocity.Y, -2, 2);
				//npc.velocity = Utils.Clamp(npc.velocity.Length(), -3, 3) * npc.velocity.SafeNormalize(Vector2.Zero);
			}
			else if (npc.ai[1] == 3f) {
				npc.velocity.Y = npc.velocity.Y + 0.1f;
				if (npc.velocity.Y < 0f) {
					npc.velocity.Y = npc.velocity.Y * 0.95f;
				}
				npc.velocity.X = npc.velocity.X * 0.95f;
				if (npc.timeLeft > 50) {
					npc.timeLeft = 50;
				}
			}
		}
	}
}

/*
// Here is an alternate approach where we manually draw the NPC rather than let vanilla do that. This might be useful if you need more logic in how the npc is drawn.
public override bool PreDraw(SpriteBatch spriteBatch, Color drawColor)
{
	SpriteEffects spriteEffects = SpriteEffects.None;
	if (npc.spriteDirection == 1)
	{
		spriteEffects = SpriteEffects.FlipHorizontally;
	}

	Main.spriteBatch.End();
	Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.Default, RasterizerState.CullNone, null, Main.GameViewMatrix.ZoomMatrix);

	// Retrieve reference to shader
	var deathShader = GameShaders.Misc["ExampleMod:DeathAnimation"];
	deathShader.UseOpacity(1f);
	// npc.ai[3] we use as a counter since the real death.
	if (npc.ai[3] > 30f)
	{
		// Our shader uses Opacity to drive the effect.
		deathShader.UseOpacity(1f - (npc.ai[3] - 30f) / 150f);
	}
	deathShader.Apply(null);

	float extraDrawY = Main.NPCAddHeight(npc.whoAmI);
	Vector2 origin = new Vector2(Main.npcTexture[npc.type].Width / 2, Main.npcTexture[npc.type].Height / Main.npcFrameCount[npc.type] / 2);
	Main.spriteBatch.Draw(Main.npcTexture[npc.type],
		new Vector2(npc.position.X - Main.screenPosition.X + npc.width / 2 - (float) Main.npcTexture[npc.type].Width* npc.scale / 2f + origin.X* npc.scale,
		npc.position.Y - Main.screenPosition.Y + npc.height - Main.npcTexture[npc.type].Height* npc.scale / Main.npcFrameCount[npc.type] + 4f + extraDrawY + origin.Y* npc.scale + npc.gfxOffY),
		npc.frame,
		npc.GetAlpha(drawColor), npc.rotation, origin, npc.scale, spriteEffects, 0f);
	if (npc.color != default(Color))
	{
		Main.spriteBatch.Draw(Main.npcTexture[npc.type], new Vector2(npc.position.X - Main.screenPosition.X + npc.width / 2 - Main.npcTexture[npc.type].Width* npc.scale / 2f + origin.X* npc.scale, npc.position.Y - Main.screenPosition.Y + npc.height - Main.npcTexture[npc.type].Height* npc.scale / Main.npcFrameCount[npc.type] + 4f + extraDrawY + origin.Y* npc.scale + npc.gfxOffY), npc.frame, npc.GetColor(drawColor), npc.rotation, origin, npc.scale, spriteEffects, 0f);
	}

	// Restart spriteBatch to reset applied shaders
	Main.spriteBatch.End();
	Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullCounterClockwise, null, Main.Transform);

	// Prevent Vanilla drawing
	return false;
}
*/
