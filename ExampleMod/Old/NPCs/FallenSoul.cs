using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace ExampleMod.NPCs
{
	// This NPC inherits from the Hover abstract class included in ExampleMod, which is a more customizable copy of the vanilla Hovering AI.
	// It implements the `CustomBehavior` and `ShouldMove` virtual methods being overridden here, as well as the `acceleration` and `accelerationY` field being set in the class constructor.
	public class FallenSoul : Hover
	{
		public FallenSoul() {
			acceleration = 0.06f;
			accelerationY = 0.025f;
		}

		public override void SetStaticDefaults() {
			Main.npcFrameCount[npc.type] = Main.npcFrameCount[NPCID.Wraith];
		}

		public override void SetDefaults() {
			npc.CloneDefaults(NPCID.Wraith);
			npc.width = 28;
			npc.height = 36;
			npc.aiStyle = -1;
			npc.damage = 0;
			npc.friendly = true;
			npc.dontTakeDamageFromHostiles = true;
			animationType = NPCID.Wraith;
		}

		// Allows hitting the NPC with melee type weapons, even if it's friendly.
		public override bool? CanBeHitByItem(Player player, Item item) {
			return true;
		}

		// Same as the above but with projectiles.
		public override bool? CanBeHitByProjectile(Projectile projectile) {
			return projectile.friendly && projectile.owner < 255;
		}

		public override void HitEffect(int hitDirection, double damage) {
			if (npc.life > 0) {
				for (int i = 0; i < damage / npc.lifeMax * 100; i++) {
					Dust dust = Dust.NewDustDirect(npc.position, npc.width, npc.height, 192, hitDirection, -1f, 100, new Color(100, 100, 100, 100), 1f);
					dust.noGravity = true;
				}
				return;
			}
			for (int i = 0; i < 50; i++) {
				Dust dust = Dust.NewDustDirect(npc.position, npc.width, npc.height, 192, 2 * hitDirection, -2f, 100, new Color(100, 100, 100, 100), 1f);
				dust.noGravity = true;
			}
		}

		// Allows the NPC to talk with the player, even if it isn't a town NPC.
		public override bool CanChat() {
			return true;
		}

		public override string GetChat() {
			// npc.SpawnedFromStatue value is kept when the NPC is transformed.
			switch (Main.rand.Next(npc.SpawnedFromStatue ? 5 : 3)) {
				case 0:
					return "Thank you, now i don't have to haunt random people anymore, only you.";
				case 1:
					return "Keep breaking those evil altars, me and many others were cursed to haunt anyone who did so.";
				case 2:
					return "Can you help me get into heaven?";
				default:
					return "Please stop messing with that haunted statue. Don't you know what \"RIP\" means?";
			}
		}

		public override void SetChatButtons(ref string button, ref string button2) {
			button = "Send to heaven";
		}

		public override void OnChatButtonClicked(bool firstButton, ref bool shop) {
			if (firstButton) {
				// Hit the NPC for about 500 damage
				Main.LocalPlayer.ApplyDamageToNPC(npc, Main.DamageVar(500), 5f, Main.LocalPlayer.direction, true);
			}
		}

		// Only show health bar of the NPC when close to the player
		public override bool? DrawHealthBar(byte hbPosition, ref float scale, ref Vector2 position) {
			float distance = npc.Distance(Main.player[npc.target].Center);
			if (distance <= 200) {
				if (distance > 100) {
					// Make the health bar become smaller the farther away the NPC is.
					scale *= (100 - (distance - 100)) / 100;
				}
				return null;
			}
			return false;
		}

		// Make the NPC invisible when far away from the player.
		public override void CustomBehavior(ref float ai) {
			float distance = npc.Distance(Main.player[npc.target].Center);
			if (distance <= 250) {
				npc.alpha = 100;
				if (distance > 100) {
					// Make the NPC fade out the farther away the NPC is.
					npc.alpha += (int)(155 * ((distance - 100) / 150));
				}
				return;
			}
			npc.alpha = 255;
		}

		// Make the NPC stop moving if it is close to the player.
		public override bool ShouldMove(float ai) {
			npc.ai[2] = 0; // Prevents the NPC from stopping following their target.
			if (npc.Distance(Main.player[npc.target].Center) < 150f) {
				npc.velocity *= 0.95f;
				if (Math.Abs(npc.velocity.X) < 0.1f) {
					npc.spriteDirection = Main.player[npc.target].Center.X > npc.Center.X ? 1 : -1;
					npc.velocity.X = 0;
				}
				return false;
			}
			return true;
		}
	}

	public class PurificationPowder : GlobalProjectile
	{
		// Make purification powder transform wraiths into purified ghosts.
		public override void PostAI(Projectile projectile) {
			if (projectile.type != ProjectileID.PurificationPowder || Main.netMode == NetmodeID.MultiplayerClient) {
				return;
			}

			for (int i = 0; i < Main.maxNPCs; i++) {
				if (Main.npc[i].active && Main.npc[i].type == NPCID.Wraith && projectile.Hitbox.Intersects(Main.npc[i].Hitbox)) {
					Main.npc[i].Transform(NPCType<FallenSoul>());
				}
			}
		}
	}
}
