using Terraria;
using Terraria.GameContent.ItemDropRules;
using Terraria.Localization;

namespace ExampleMod.Common.ItemDropRules.DropConditions
{
	// Drop condition where items drop only on Journey mode.
	public class ExampleJourneyModeDropCondition : IItemDropRuleCondition
	{
		private static LocalizedText Description;

		public ExampleJourneyModeDropCondition() {
			Description ??= Language.GetOrRegister("Mods.ExampleMod.DropConditions.JourneyMode");
		}

		public bool CanDrop(DropAttemptInfo info) {
			return Main.GameModeInfo.IsJourneyMode;
		}

		public bool CanShowItemDropInUI() {
			return Main.GameModeInfo.IsJourneyMode; // We don't want players to see this drop listed while in normal worlds.
		}

		public string GetConditionDescription() {
			return Description.Value;
		}
	}
}
