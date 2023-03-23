using Terraria;
using Terraria.GameContent.ItemDropRules;

namespace ExampleMod.Common.ItemDropRules.DropConditions
{
	// Drop condition where items drop only on Journey mode.
	public class ExampleJourneyModeDropCondition : IItemDropRuleCondition
	{
		public bool CanDrop(DropAttemptInfo info) {
			return Main.GameModeInfo.IsJourneyMode;
		}

		public bool CanShowItemDropInUI() {
			return true;
		}

		public string GetConditionDescription() {
			return "Drops only on Journey Mode";
		}
	}
}
