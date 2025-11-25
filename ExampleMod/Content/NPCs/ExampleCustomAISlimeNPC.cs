using ExampleMod.Content.Items.Placeable.Banners;
using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ModLoader.Utilities;

namespace ExampleMod.Content.NPCs
{
	// This ModNPC serves as an example of a completely custom AI.
	public class ExampleCustomAISlimeNPC : ModNPC
	{
		// Here we define an enum we will use with the State slot. Using an ai slot as a means to store "state" can simplify things greatly. Think flowchart.
		private enum ActionState
		{
			Asleep,
			Notice,
			Jump,
			Hover,
			Fall
		}

		// Our texture is 36x36 with 2 pixels of padding vertically, so 38 is the vertical spacing.
		// These are for our benefit and the numbers could easily be used directly in the code below, but this is how we keep code organized.
		private enum Frame
		{
			Asleep,
			Notice,
			Falling,
			Flutter1,
			Flutter2,
			Flutter3
		}

		// These are reference properties. One, for example, lets us write AI_State as if it's NPC.ai[0], essentially giving the index zero our own name.
		// Here they help to keep our AI code clear of clutter. Without them, every instance of "AI_State" in the AI code below would be "npc.ai[0]", which is quite hard to read.
		// This is all to just make beautiful, manageable, and clean code.
		public ref float AI_State => ref NPC.ai[0];
		public ref float AI_Timer => ref NPC.ai[1];
		public ref float AI_FlutterTime => ref NPC.ai[2];

		public static LocalizedText GotStompedText { get; private set; }

		public override void SetStaticDefaults() {
			Main.npcFrameCount[Type] = 6; // make sure to set this for your ModNPCs.

			NPCID.Sets.ShimmerTransformToNPC[Type] = NPCID.ShimmerSlime;

			// Specify the debuffs it is immune to.
			// This NPC will be immune to the Poisoned debuff.
			NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.Poisoned] = true;
			NPCID.Sets.SpecificDebuffImmunity[Type][ModContent.BuffType<Buffs.ExampleGravityDebuff>()] = true;

			GotStompedText = this.GetLocalization("GotStomped");
		}

		public override void SetDefaults() {
			NPC.width = 36; // The width of the npc's hitbox (in pixels)
			NPC.height = 36; // The height of the npc's hitbox (in pixels)
			NPC.aiStyle = -1; // This npc has a completely unique AI, so we set this to -1. The default aiStyle 0 will face the player, which might conflict with custom AI code.
			NPC.damage = 7; // The amount of damage that this npc deals
			NPC.defense = 2; // The amount of defense that this npc has
			NPC.lifeMax = 25; // The amount of health that this npc has
			NPC.HitSound = SoundID.NPCHit1; // The sound the NPC will make when being hit.
			NPC.DeathSound = SoundID.NPCDeath1; // The sound the NPC will make when it dies.
			NPC.value = 25f; // How many copper coins the NPC will drop when killed.

			Banner = Type;
			BannerItem = ModContent.ItemType<ExampleCustomAISlimeNPCBanner>();
		}

		public override float SpawnChance(NPCSpawnInfo spawnInfo) {
			// we would like this npc to spawn in the overworld.
			return SpawnCondition.OverworldDaySlime.Chance * 0.1f;
		}

		// Our AI here makes our NPC sit waiting for a player to enter range, jumps to attack, flutter mid-fall to stay afloat a little longer, then falls to the ground. Note that animation should happen in FindFrame
		public override void AI() {
			// The npc starts in the asleep state, waiting for a player to enter range
			switch (AI_State) {
				case (float)ActionState.Asleep:
					FallAsleep();
					break;
				case (float)ActionState.Notice:
					Notice();
					break;
				case (float)ActionState.Jump:
					Jump();
					break;
				case (float)ActionState.Hover:
					Hover();
					break;
				case (float)ActionState.Fall:
					if (NPC.velocity.Y == 0) {
						NPC.velocity.X = 0;
						AI_State = (float)ActionState.Asleep;
						AI_Timer = 0;
					}

					break;
			}
		}

		// Here in FindFrame, we want to set the animation frame our npc will use depending on what it is doing.
		// We set npc.frame.Y to x * frameHeight where x is the xth frame in our spritesheet, counting from 0. For convenience, we have defined a enum above.
		public override void FindFrame(int frameHeight) {
			// This makes the sprite flip horizontally in conjunction with the npc.direction.
			NPC.spriteDirection = NPC.direction;

			// For the most part, our animation matches up with our states.
			switch (AI_State) {
				case (float)ActionState.Asleep:
					// npc.frame.Y is the goto way of changing animation frames. npc.frame starts from the top left corner in pixel coordinates, so keep that in mind.
					NPC.frame.Y = (int)Frame.Asleep * frameHeight;
					break;
				case (float)ActionState.Notice:
					// Going from Notice to Asleep makes our npc look like it's crouching to jump.
					if (AI_Timer < 10) {
						NPC.frame.Y = (int)Frame.Notice * frameHeight;
					}
					else {
						NPC.frame.Y = (int)Frame.Asleep * frameHeight;
					}

					break;
				case (float)ActionState.Jump:
					NPC.frame.Y = (int)Frame.Falling * frameHeight;
					break;
				case (float)ActionState.Hover:
					// Here we have 3 frames that we want to cycle through.
					NPC.frameCounter++;

					if (NPC.frameCounter < 10) {
						NPC.frame.Y = (int)Frame.Flutter1 * frameHeight;
					}
					else if (NPC.frameCounter < 20) {
						NPC.frame.Y = (int)Frame.Flutter2 * frameHeight;
					}
					else if (NPC.frameCounter < 30) {
						NPC.frame.Y = (int)Frame.Flutter3 * frameHeight;
					}
					else {
						NPC.frameCounter = 0;
					}

					break;
				case (float)ActionState.Fall:
					NPC.frame.Y = (int)Frame.Falling * frameHeight;
					break;
			}
		}

		// Here, because we use custom AI (aiStyle not set to a suitable vanilla value), we should manually decide when Flutter Slime can fall through platforms
		public override bool? CanFallThroughPlatforms() {
			if (AI_State == (float)ActionState.Fall && NPC.HasValidTarget && Main.player[NPC.target].Top.Y > NPC.Bottom.Y) {
				// If Flutter Slime is currently falling, we want it to keep falling through platforms as long as it's above the player
				return true;
			}

			return false;
			// You could also return null here to apply vanilla behavior (which is the same as false for custom AI)
		}

		private void FallAsleep() {
			// TargetClosest sets npc.target to the player.whoAmI of the closest player.
			// The faceTarget parameter means that npc.direction will automatically be 1 or -1 if the targeted player is to the right or left.
			// This is also automatically flipped if npc.confused.
			NPC.TargetClosest(true);

			// Now we check the make sure the target is still valid and within our specified notice range (500)
			if (NPC.HasValidTarget && Main.player[NPC.target].Distance(NPC.Center) < 500f) {
				// Since we have a target in range, we change to the Notice state. (and zero out the Timer for good measure)
				AI_State = (float)ActionState.Notice;
				AI_Timer = 0;
			}
		}

		private void Notice() {
			// If the targeted player is in attack range (250).
			if (Main.player[NPC.target].Distance(NPC.Center) < 250f) {
				// Here we use our Timer to wait .33 seconds before actually jumping. In FindFrame you'll notice AI_Timer also being used to animate the pre-jump crouch
				AI_Timer++;

				if (AI_Timer >= 20) {
					AI_State = (float)ActionState.Jump;
					AI_Timer = 0;
				}
			}
			else {
				NPC.TargetClosest(true);

				if (!NPC.HasValidTarget || Main.player[NPC.target].Distance(NPC.Center) > 500f) {
					// Out targeted player seems to have left our range, so we'll go back to sleep.
					AI_State = (float)ActionState.Asleep;
					AI_Timer = 0;
				}
			}
		}

		private void Jump() {
			AI_Timer++;

			if (AI_Timer == 1) {
				// We apply an initial velocity the first tick we are in the Jump frame. Remember that -Y is up.
				NPC.velocity = new Vector2(NPC.direction * 2, -10f);
			}
			else if (AI_Timer > 40) {
				// after .66 seconds, we go to the hover state. // TODO, gravity?
				AI_State = (float)ActionState.Hover;
				AI_Timer = 0;
			}
		}

		private void Hover() {
			AI_Timer++;

			// Here we make a decision on how long this flutter will last. We check netmode != 1 to prevent Multiplayer Clients from running this code. (similarly, spawning projectiles should also be wrapped like this)
			// netMode == 0 is SP, netMode == 1 is MP Client, netMode == 2 is MP Server.
			// Typically in MP, Client and Server maintain the same state by running deterministic code individually. When we want to do something random, we must do that on the server and then inform MP Clients.
			if (AI_Timer == 1 && Main.netMode != NetmodeID.MultiplayerClient) {
				// For reference: without proper syncing: https://github.com/user-attachments/assets/27b289c0-37a6-47e8-9e35-ea6f641612f0 and with proper syncing: https://github.com/user-attachments/assets/f2bdcea4-8fe6-4eba-aa0d-0d36ade9a16d
				AI_FlutterTime = Main.rand.NextBool() ? 100 : 50;

				// Informing MP Clients is done automatically by syncing the npc.ai array over the network whenever npc.netUpdate is set.
				// Don't set netUpdate unless you do something non-deterministic ("random")
				NPC.netUpdate = true;
			}

			// Here we add a tiny bit of upward velocity to our npc.
			NPC.velocity += new Vector2(0, -.35f);

			// ... and some additional X velocity when traveling slow.
			if (Math.Abs(NPC.velocity.X) < 2) {
				NPC.velocity += new Vector2(NPC.direction * .05f, 0);
			}

			// after fluttering for 100 ticks (1.66 seconds), our Flutter Slime is tired, so he decides to go into the Fall state.
			if (AI_Timer > AI_FlutterTime) {
				AI_State = (float)ActionState.Fall;
				AI_Timer = 0;
			}
		}

		public override bool ModifyCollisionData(Rectangle victimHitbox, ref int immunityCooldownSlot, ref MultipliableFloat damageMultiplier, ref Rectangle npcHitbox) {
			// We can use ModifyCollisionData to customize collision damage.
			// Here we double damage when this npc is in the falling state and the victim is almost directly below the npc
			if (AI_State == (float)ActionState.Fall) {
				// We can modify npcHitbox directly to implement a dynamic hitbox, but in this example we make a new hitbox to apply bonus damage
				// This math creates a hitbox focused on the bottom center of the original 36x36 hitbox:
				// --> ☐☐☐
				//     ☐☒☐
				Rectangle extraDamageHitbox = new Rectangle(npcHitbox.X + 12, npcHitbox.Y + 18, npcHitbox.Width - 24, npcHitbox.Height - 18);
				if (victimHitbox.Intersects(extraDamageHitbox)) {
					damageMultiplier *= 2f;
					Main.NewText(GotStompedText.Value);
				}
			}
			return true;
		}
	}
}
