using System;

namespace Terraria.GameContent.ItemDropRules;

partial class Conditions
{
	/// <summary>
	/// Evaluates a runtime pot reward condition. Pot rewards are not evaluated during UI simulations.
	/// </summary>
	public sealed class PotDrop : IItemDropRuleCondition
	{
		private readonly Func<DropAttemptInfo, bool> predicate;

		public PotDrop(Func<DropAttemptInfo, bool> predicate)
		{
			ArgumentNullException.ThrowIfNull(predicate);
			this.predicate = predicate;
		}

		public bool CanDrop(DropAttemptInfo info) => info.pot != null && !info.IsInSimulation && predicate(info);

		public bool CanShowItemDropInUI() => false;

		public string GetConditionDescription() => null;
	}
}
