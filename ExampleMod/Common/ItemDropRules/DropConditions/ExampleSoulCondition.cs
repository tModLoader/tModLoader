using ExampleMod.Content.Biomes;
using Terraria;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.Localization;

namespace ExampleMod.Common.ItemDropRules.DropConditions
{
	public class ExampleSoulCondition : IItemDropRuleCondition
	{
		private static LocalizedText Description;

		public ExampleSoulCondition() {
			Description ??= Language.GetOrRegister("Mods.ExampleMod.DropConditions.ExampleSoul");
		}

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
			return Description.Value;
		}
	}
}
