using Terraria;
using Terraria.GameContent.ItemDropRules;

namespace ExampleMod.Common.ItemDropRules.Conditions
{
	//Very simple drop condition: drop during daytime
	public class ExampleDropCondition : IItemDropRuleCondition
	{
		public bool CanDrop(DropAttemptInfo info) {
			if (!info.IsInSimulation) {
				return Main.dayTime;
			}
			return false;
		}

		public bool CanShowItemDropInUI() {
			return true;
		}

		public string GetConditionDescription() {
			return "Drops during daytime";
		}
	}
}
