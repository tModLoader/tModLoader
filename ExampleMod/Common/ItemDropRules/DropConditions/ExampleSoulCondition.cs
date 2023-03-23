using ExampleMod.Content.Biomes;
using Terraria;
using Terraria.ID;
using Terraria.GameContent.ItemDropRules;

namespace ExampleMod.Common.ItemDropRules.DropConditions
{
	public class ExampleSoulCondition : IItemDropRuleCondition
	{
		public bool CanDrop(DropAttemptInfo info) {
			NPC npc = info.npc;
			return Main.hardMode
				&& !NPCID.Sets.CannotDropSouls[npc.type]
				&& !npc.boss
				&& !npc.friendly
				&& npc.lifeMax > 1
				&& npc.value >= 1f
				&& info.player.InModBiome<ExampleUndergroundBiome>();
		}

		public bool CanShowItemDropInUI() {
			return true;
		}

		public string GetConditionDescription() {
			return "Drops in 'Example Underground Biome' in hardmode";
		}
	}
}
