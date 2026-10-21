using System;

namespace Terraria.GameContent.ItemDropRules;

/// <summary>
/// Drops a pot item with a context-dependent stack without scaling its chance with luck.
/// The item ID and stack bounds remain available to loot modifiers and rate reporting.
/// </summary>
public sealed class PotStackRule : CommonDropNotScalingWithLuck
{
	private readonly Func<DropAttemptInfo, int> stack;

	public PotStackRule(int item, Func<DropAttemptInfo, int> stack, int min, int max)
		: base(item, 1, min, max)
	{
		ArgumentNullException.ThrowIfNull(stack);
		this.stack = stack;
	}

	public override bool CanDrop(DropAttemptInfo info) => info.pot != null && !info.IsInSimulation;

	public override ItemDropAttemptResult TryDroppingItem(DropAttemptInfo info)
	{
		CommonCode.DropItem(info, itemId, stack(info));
		return new ItemDropAttemptResult { State = ItemDropAttemptResultState.Success };
	}
}
