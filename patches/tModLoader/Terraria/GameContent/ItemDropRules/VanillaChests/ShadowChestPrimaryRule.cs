using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using Terraria.Utilities;
using Terraria.WorldBuilding;

namespace Terraria.GameContent.ItemDropRules.VanillaChests;
public class ShadowChestPrimaryRule(int chanceDenominator = 1, int amountDroppedMinimum = 1, int amountDroppedMaximum = 1, int chanceNumerator = 1) : DropFromItemPoolRule(ChestLootLoader.ItemPoolNames.ShadowRare, chanceDenominator, amountDroppedMinimum, amountDroppedMaximum, chanceNumerator)
{
	public override ItemDropAttemptResult TryDroppingItem(DropAttemptInfo info)
	{
		ItemDropAttemptResult result;
		if (info.RollLuck(chanceDenominator) < chanceNumerator) {
			Tuple<(int type, List<IItemDropRule> chainedRules), double>[] options = GetDropableEntries(info).ToArray();
			if (options.Length == 0) {
				result = default;
				result.State = ItemDropAttemptResultState.DoesntFillConditions;
				return result;
			}
			(int itemId, List<IItemDropRule> chainedRules) = options[GenVars.hellChest % options.Length].Item1;
			CommonCode.DropItem(info, itemId, info.rng.Next(amountDroppedMinimum, amountDroppedMaximum + 1));
			for (int i = 0; i < chainedRules.Count; i++) {
				ItemDropResolver.ResolveRule(chainedRules[i], info);
			}
			result = default;
			result.State = ItemDropAttemptResultState.Success;
			return result;
		}

		result = default;
		result.State = ItemDropAttemptResultState.FailedRandomRoll;
		return result;
	}
}
