using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using Terraria.ObjectData;
using Terraria.Utilities;
using Terraria.WorldBuilding;

namespace Terraria.GameContent.ItemDropRules.VanillaChests;
public class ShadowKeyRule(int chanceDenominator = 3, int amountDroppedMinimum = 1, int amountDroppedMaximum = 1, int chanceNumerator = 1) : CommonDrop(ItemID.ShadowKey, chanceDenominator, amountDroppedMinimum, amountDroppedMaximum, chanceNumerator)
{
	public override ItemDropAttemptResult TryDroppingItem(DropAttemptInfo info)
	{
		ItemDropAttemptResult result = default;
		if (info.chest is null) {
			result.State = ItemDropAttemptResultState.DoesntFillConditions;
			return result;
		}
		Tile tile = Main.tile[info.chest.x, info.chest.y];
		if (tile.type != TileID.Containers || TileObjectData.GetTileStyle(tile) == 0 || !WorldGen.IsDungeon(info.chest.x, info.chest.y + 2)) {
			result.State = ItemDropAttemptResultState.DoesntFillConditions;
			return result;
		}
		if (!GenVars.generatedShadowKey || info.RollLuck(chanceDenominator) < chanceNumerator) {
			CommonCode.DropItem(info, itemId, info.rng.Next(amountDroppedMinimum, amountDroppedMaximum + 1));
			GenVars.generatedShadowKey = true;
			result.State = ItemDropAttemptResultState.Success;
			return result;
		}

		result.State = ItemDropAttemptResultState.FailedRandomRoll;
		return result;
	}
}
