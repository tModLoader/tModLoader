using Terraria;
using Terraria.Audio;
using Terraria.GameInput;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExampleMod.Content.NPCs
{
	// This NPC is simply an exhibition of the DrawBehind method.
	// The npc cycles between all the available "layers" that a ModNPC can be drawn at.
	// Spawn this NPC with something like Cheat Sheet or Hero's Mod to view the effect.
	// In addition, this NPC showcases the PreHoverInteract hook demonstrating a custom right click and hover interaction. This example is unrelated to DrawBehind exhibition.
	public class ExampleDrawBehindNPC : ModNPC
	{
		public override void SetStaticDefaults() {
			// Total count animation frames
			Main.npcFrameCount[Type] = 6;
		}

		public override void SetDefaults() {
			NPC.width = 30; // The width of the npc hitbox
			NPC.height = 40; // The height of the npc hitbox
			NPC.aiStyle = -1; // Using custom AI
			NPC.damage = 0; // The amount of damage this NPC will deal on collision
			NPC.defense = 2; // How resistant to damage this NPC is
			NPC.lifeMax = 100; // The maximum life of this NPC
			NPC.HitSound = SoundID.NPCHit2; // The sound that plays when this npc is hit
			NPC.DeathSound = SoundID.NPCDeath2; // The sound that plays when this npc dies
			NPC.noGravity = true; // If true, the npc will not be affected by gravity
			NPC.noTileCollide = true; // If true, the npc does not collide with tiles
			NPC.knockBackResist = 0f; // How much of the knockback it receives will actually apply. 1f: full knockback; 0f: no knockback
		}

		// The current drawing layer will change every 40 ticks
		private int CurrentLayer => (int)(NPC.ai[0] / 40);

		// This changes the frame from the this NPC's texture that is drawn, depending on the current layer
		public override void FindFrame(int frameHeight) {
			NPC.frame.Y = CurrentLayer * frameHeight;
		}

		public override void AI() {
			NPC.ai[0] = (NPC.ai[0] + 1) % 240;

			// These are the defaults for normal drawing(case 3)
			NPC.hide = false;
			NPC.behindTiles = false;

			switch (CurrentLayer) {
				case 0:
				case 1:
				case 4:
				case 5:
					NPC.hide = true;
					break;
				case 2:
					NPC.behindTiles = true;
					break;
				case 3:
					break;
			}
		}

		// This method allows you to specify that this npc should be drawn behind certain elements
		public override void DrawBehind(int index) {
			// The 6 available positions are as follows:
			switch (CurrentLayer) {
				case 0: // Behind tiles and walls
					Main.instance.DrawCacheNPCsMoonMoon.Add(index);
					break;
				case 1: // Behind non solid tiles, but in front of walls
					Main.instance.DrawCacheNPCsBehindNonSolidTiles.Add(index);
					break;
				case 2: // Behind tiles, but in front of non solid tiles
				case 3: // Normal (in front of tiles)
					break;
				case 4: // In front of all normal NPC
					Main.instance.DrawCacheNPCProjectiles.Add(index);
					break;
				case 5: // In front of Players
					Main.instance.DrawCacheNPCsOverPlayers.Add(index);
					break;
			}
		}

		// PreHoverInteract gives this NPC a custom right click interaction and hover behavior. This example is unrelated to the DrawBehind exhibition.
		public override bool PreHoverInteract(bool mouseIntersects) {
			if (CurrentLayer < 3) {
				return true;
			}

			// This code is similar to the code that lets the player right click on the Old Shaking Chest NPC to consume a GoldenKey from the player and transform into Elder Slime.
			Player player = Main.LocalPlayer;
			int keyItem = ModContent.ItemType<Items.Placeable.ExampleBlock>();
			player.cursorItemIconEnabled = true;
			player.cursorItemIconID = keyItem;
			player.cursorItemIconText = "";
			player.noThrow = 2;

			// This code can be used if you want to enforce the normal NPC interaction range in this method.
			/*
			Rectangle interactionRange = new Rectangle((int)(player.Center.X - Player.tileRangeX * 16), (int)(player.Center.Y - Player.tileRangeY * 16), Player.tileRangeX * 16 * 2, Player.tileRangeY * 16 * 2);
			if (!interactionRange.Intersects(NPC.getRect())) {
				return false;
			}
			*/

			if (!player.dead) {
				PlayerInput.SetZoom_MouseInWorld();
				if (Main.mouseRight && Main.npcChatRelease) {
					Main.npcChatRelease = false;
					if (PlayerInput.UsingGamepad) {
						player.releaseInventory = false;
					}

					if (player.talkNPC != NPC.whoAmI && !player.tileInteractionHappened) {
						if (player.HasItem(keyItem) && player.ConsumeItem(keyItem)) {
							SoundEngine.PlaySound(SoundID.Item14); // The bomb explosion sound
							NPC.SimpleStrikeNPC(1000, 0);
						}
						else {
							SoundEngine.PlaySound(SoundID.MenuClose);
						}
					}
				}
			}

			return false; // skip vanilla.
		}

		// We can use CanChat to force smart interact to target this NPC, even though we don't intend to actually chat with it due to PreHoverInteract bypassing the chat code.
		public override bool CanChat() {
			if (CurrentLayer < 3) {
				return base.CanChat();
			}

			return true;
		}
	}
}
