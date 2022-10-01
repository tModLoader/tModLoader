using ExampleMod.Content.NPCs.MinionBoss;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Chat;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace ExampleMod.Common.Systems
{
	public class NaturalBossSpawnSystem : ModSystem
	{
		public static bool spawnMinionBoss = false;

		// Exiting the world will prevent the Minion Boss from spawing naturally. The same as the Eye of Cthulhu
		public override void OnWorldLoad() {
			spawnMinionBoss = false;
		}

		public override void PostUpdateTime() {
			if (Main.dayTime) {
				spawnMinionBoss = false;
			}

			// The boss will be spawned at 9:10 PM.
			// How the time value came: 9:10 PM - 7:30 PM = 1:40, Total seconds: 1*60*60 + 40*60 = 6000
			if (!spawnMinionBoss || Main.time <= 6000) {
				return;
			}
			
			for (int i = 0; i < Main.maxPlayers; i++) {
				var player = Main.player[i];

				// Never spawn on players that is dead or under the surface.
				if (!player.active || player.DeadOrGhost || player.position.Y >= Main.worldSurface * 16.0) {
					continue;
				}

				NPC.SpawnOnPlayer(i, ModContent.NPCType<MinionBossBody>());
				spawnMinionBoss = false;
				break;
			}
		}

		public override void OnStartNight(ref bool stopEvents) {
			// If events should stop, the sundial is being usedm or minion boss is downed, it should not spawn.
			// Since the boss spawning code shouldn't run on multiplayer client, we don't have to set the spawnMinionBoss for it.
			// Each night the Minion Boss has a 10% chance of being spawned.
			if (Main.fastForwardTime || stopEvents || DownedBossSystem.downedMinionBoss || Main.netMode is NetmodeID.MultiplayerClient || !Main.rand.NextBool(10)) {
				return;
			}

			spawnMinionBoss = true;
			stopEvents = true; // To prevent other events from occuring
			// Give the player a hint and tell them that the boss will spawn tonight
			if (Main.netMode == NetmodeID.SinglePlayer) {
				Main.NewText("The big green eyeball is on its way!", 50, 255, 130);
			}
			else {
				ChatHelper.BroadcastChatMessage(NetworkText.FromLiteral("The big green eyeball is on its way!"), new Color(50, 255, 130));
			}
		}
	}
}
