using System.Collections.Generic;
using Terraria.GameContent.ItemDropRules;

namespace Terraria.ModLoader;

public abstract class BaseItemDropRule : IItemDropRule
{
	public bool Disabled { get; protected set; }
	public List<IItemDropRuleChainAttempt> ChainedRules { get; } = new();

	public void Disable()
	{
		Disabled = true;
	}

	public virtual bool CanDrop(DropAttemptInfo info) => true;
	public abstract void ReportDroprates(List<DropRateInfo> drops, DropRateInfoChainFeed ratesInfo);
	public abstract ItemDropAttemptResult TryDroppingItem(DropAttemptInfo info);
}
