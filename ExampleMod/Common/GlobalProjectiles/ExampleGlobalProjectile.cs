using ExampleMod.Content.NPCs;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExampleMod.Common.GlobalProjectiles
{
	// This file shows an example of a GlobalProjectile class. GlobalProjectile hooks are called on all projectiles in the game and are suitable for sweeping
	// changes like adding additional data to all projectiles in the game. Here we simply override PostAI in order to make Purification Power loop through the
	// npc array to affect Wraiths specifically, transforming them into Fallen Souls, as it is simple to understand.
	public class ExampleGlobalProjectile : GlobalProjectile
	{
		// Make purification powder transform wraiths into purified ghosts.
		public override void PostAI(Projectile projectile) {
			if (projectile.type != ProjectileID.PurificationPowder || Main.netMode == NetmodeID.MultiplayerClient) {
				return;
			}

			for (int i = 0; i < Main.maxNPCs; i++) {
				if (Main.npc[i].active && Main.npc[i].type == NPCID.Wraith && projectile.Hitbox.Intersects(Main.npc[i].Hitbox)) {
					Main.npc[i].Transform(ModContent.NPCType<FallenSoul>());
				}
			}
		}
	}
}
