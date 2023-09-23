namespace Terraria.GameContent.ItemDropRules;

public class CommonDropNotScalingWithLuckWithNumerator : CommonDrop
{
	public CommonDropNotScalingWithLuckWithNumerator(int itemId, int chanceDenominator, int chanceNumerator, int amountDroppedMinimum, int amountDroppedMaximum)
		: base(itemId, chanceDenominator, amountDroppedMinimum, amountDroppedMaximum, chanceNumerator)
	{
	}

	public override ItemDropAttemptResult TryDroppingItem(DropAttemptInfo info)
	{
		ItemDropAttemptResult result;
		if (info.rng.Next(chanceDenominator) < chanceNumerator) {
			CommonCode.DropItem(info, itemId, info.rng.Next(amountDroppedMinimum, amountDroppedMaximum + 1));
			result = default(ItemDropAttemptResult);
			result.State = ItemDropAttemptResultState.Success;
			return result;
		}

		result = default(ItemDropAttemptResult);
		result.State = ItemDropAttemptResultState.FailedRandomRoll;
		return result;
	}
}
