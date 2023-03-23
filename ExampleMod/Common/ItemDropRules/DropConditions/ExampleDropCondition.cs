using Terraria;
using Terraria.GameContent.ItemDropRules;

namespace ExampleMod.Common.ItemDropRules.DropConditions
{
	// Very simple drop condition: drop during daytime
	public class ExampleDropCondition : IItemDropRuleCondition
	{
		public bool CanDrop(DropAttemptInfo info) {
			return Main.dayTime;
		}

		public bool CanShowItemDropInUI() {
			return true;
		}

		public string GetConditionDescription() {
			return "Drops during daytime";
		}
	}
}
