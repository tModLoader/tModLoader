using Terraria;
using Terraria.GameContent.ItemDropRules;
using Terraria.Localization;

namespace ExampleMod.Common.ItemDropRules.DropConditions
{
	// Very simple drop condition: drop during daytime
	public class ExampleDropCondition : IItemDropRuleCondition
	{
		private static LocalizedText Description;

		public ExampleDropCondition() {
			Description ??= Language.GetOrRegister("Mods.ExampleMod.DropConditions.Example");
		}

		public bool CanDrop(DropAttemptInfo info) {
			return Main.dayTime;
		}

		public bool CanShowItemDropInUI() {
			return true;
		}

		public string GetConditionDescription() {
			return Description.Value;
		}
	}
}
