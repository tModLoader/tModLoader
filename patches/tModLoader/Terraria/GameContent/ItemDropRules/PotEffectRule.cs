using System;
using System.Collections.Generic;

namespace Terraria.GameContent.ItemDropRules;

/// <summary>
/// Runs a non-item pot effect or selects a reward for subsequent chained rules.
/// </summary>
public sealed class PotEffectRule : IItemDropRule
{
	private readonly Action<DropAttemptInfo> effect;

	public List<IItemDropRuleChainAttempt> ChainedRules { get; } = new();

	public PotEffectRule(Action<DropAttemptInfo> effect)
	{
		ArgumentNullException.ThrowIfNull(effect);
		this.effect = effect;
	}

	public bool CanDrop(DropAttemptInfo info) => info.pot != null && !info.IsInSimulation;

	public ItemDropAttemptResult TryDroppingItem(DropAttemptInfo info)
	{
		effect(info);
		return new ItemDropAttemptResult { State = ItemDropAttemptResultState.Success };
	}

	public void ReportDroprates(List<DropRateInfo> drops, DropRateInfoChainFeed ratesInfo)
	{
		Chains.ReportDroprates(ChainedRules, 1f, drops, ratesInfo);
	}
}
