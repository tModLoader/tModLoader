using Terraria.ObjectData;

namespace Terraria.GameContent.ItemDropRules;

partial class Conditions
{
	public class NotUsedDemonHeart : IItemDropRuleCondition, IProvideItemConditionDescription
	{
		public bool CanDrop(DropAttemptInfo info) => !info.player.extraAccessory;
		public bool CanShowItemDropInUI() => true;
		public string GetConditionDescription() => null;
	}
	public class NoPortalGun : IItemDropRuleCondition, IProvideItemConditionDescription
	{
		public bool CanDrop(DropAttemptInfo info) => !info.player.HasItem(ID.ItemID.PortalGun);
		public bool CanShowItemDropInUI() => true;
		public string GetConditionDescription() => null;
	}
	public class IsPreHardmode : IItemDropRuleCondition, IProvideItemConditionDescription
	{
		public bool CanDrop(DropAttemptInfo info) => !Main.hardMode;
		public bool CanShowItemDropInUI() => true;
		public string GetConditionDescription() => null;
	}
	public class DrunkWorldIsUp : IItemDropRuleCondition, IProvideItemConditionDescription
	{
		public bool CanDrop(DropAttemptInfo info) => Main.drunkWorld;
		public bool CanShowItemDropInUI() => true;
		public string GetConditionDescription() => null;
	}
	public class ForTheWorthyIsUp : IItemDropRuleCondition, IProvideItemConditionDescription
	{
		public bool CanDrop(DropAttemptInfo info) => Main.getGoodWorld;
		public bool CanShowItemDropInUI() => true;
		public string GetConditionDescription() => null;
	}
	public class BeesSeed : IItemDropRuleCondition, IProvideItemConditionDescription
	{
		public bool CanDrop(DropAttemptInfo info) => Main.notTheBeesWorld;
		public bool CanShowItemDropInUI() => true;
		public string GetConditionDescription() => null;
	}
	public class NoTrapsSeed : IItemDropRuleCondition, IProvideItemConditionDescription
	{
		public bool CanDrop(DropAttemptInfo info) => Main.noTrapsWorld;
		public bool CanShowItemDropInUI() => true;
		public string GetConditionDescription() => null;
	}
	public class ZenithSeedIsUp : IItemDropRuleCondition, IProvideItemConditionDescription
	{
		public bool CanDrop(DropAttemptInfo info) => Main.zenithWorld;
		public bool CanShowItemDropInUI() => true;
		public string GetConditionDescription() => null;
	}
	public class DrunkWorldIsNotUp : IItemDropRuleCondition, IProvideItemConditionDescription
	{
		public bool CanDrop(DropAttemptInfo info) => !Main.drunkWorld;
		public bool CanShowItemDropInUI() => true;
		public string GetConditionDescription() => null;
	}
	public class ForTheWorthyIsNotUp : IItemDropRuleCondition, IProvideItemConditionDescription
	{
		public bool CanDrop(DropAttemptInfo info) => !Main.getGoodWorld;
		public bool CanShowItemDropInUI() => true;
		public string GetConditionDescription() => null;
	}
	public class NotBeesSeed : IItemDropRuleCondition, IProvideItemConditionDescription
	{
		public bool CanDrop(DropAttemptInfo info) => !Main.notTheBeesWorld;
		public bool CanShowItemDropInUI() => true;
		public string GetConditionDescription() => null;
	}
	public class NotNoTrapsSeed : IItemDropRuleCondition, IProvideItemConditionDescription
	{
		public bool CanDrop(DropAttemptInfo info) => !Main.noTrapsWorld;
		public bool CanShowItemDropInUI() => true;
		public string GetConditionDescription() => null;
	}
	public class ZenithSeedIsNotUp : IItemDropRuleCondition, IProvideItemConditionDescription
	{
		public bool CanDrop(DropAttemptInfo info) => !Main.zenithWorld;
		public bool CanShowItemDropInUI() => true;
		public string GetConditionDescription() => null;
	}
	public class SurfaceChest : IItemDropRuleCondition, IProvideItemConditionDescription
	{
		public bool CanDrop(DropAttemptInfo info)
		{
			if (info.chest is null)
				return false;
			return IsSurfaceOrHigher(info.chest.y + 2);
		}
		internal static bool IsSurfaceOrHigher(int floorY)
		{
			if (WorldGen.remixWorldGen) {
				return floorY >= (Main.rockLayer + ((Main.maxTilesY - 350) * 2)) / 3.0;
			}
			return floorY < Main.worldSurface + 25.0;
		}
		public bool CanShowItemDropInUI() => true;
		public string GetConditionDescription() => null;
	}
	public class UndergroundChest : IItemDropRuleCondition, IProvideItemConditionDescription
	{
		public bool CanDrop(DropAttemptInfo info)
		{
			if (info.chest is null)
				return false;
			return !SurfaceChest.IsSurfaceOrHigher(info.chest.y + 2) && IsUndergroundOrHigher(info.chest.x, info.chest.y);
		}
		internal static bool IsUndergroundOrHigher(int chestX, int chestY)
		{
			if (WorldGen.remixWorldGen) {
				int style = TileObjectData.GetTileStyle(Main.tile[chestX, chestY]);
				return style == 7 || style == 14;
			}
			return chestY + 2 < Main.rockLayer;
		}
		public bool CanShowItemDropInUI() => true;
		public string GetConditionDescription() => null;
	}
}
