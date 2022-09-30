using ExampleMod.Common.Systems;
using ExampleMod.Content.Biomes;
using ExampleMod.Content.EmoteBubbles;
using System.Collections.Generic;
using Terraria;
using Terraria.GameContent.UI;
using Terraria.ModLoader;

namespace ExampleMod.Common.GlobalNPCs
{
	// This is where we add our regular emotes for all NPCs
	public class EmotePickerGlobalNPC : GlobalNPC
	{
		public override int? PickEmote(NPC npc, Player closestPlayer, List<int> emoteList, WorldUIAnchor otherAnchor) {
			// Add the biome emote to the list if the player is in Example Biome
			// And with random chance, this emote will be less likely to appear
			if (Main.rand.NextBool(2) && ModContent.GetInstance<ExampleSurfaceBiome>().IsBiomeActive(closestPlayer)) {
				emoteList.Add(ModContent.EmoteBubbleType<ExampleBiomeEmote>());
			}

			// If minion boss is downed, its emote should appear
			if (Main.rand.NextBool(3) && DownedBossSystem.downedMinionBoss) {
				emoteList.Add(ModContent.EmoteBubbleType<MinionBossEmote>());
			}

			// Don't forget to return base method because we don't want to override the emote totally
			return base.PickEmote(npc, closestPlayer, emoteList, otherAnchor);
		}
	}
}
