using System;
using Terraria.ObjectData;
using Terraria.UI;
using Terraria.WorldBuilding;

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
			return IsUndergroundOrHigher(info.chest.y + 2);
		}
		internal static bool IsUndergroundOrHigher(int floorY)
		{
			if (WorldGen.remixWorldGen) {
				return floorY > Main.rockLayer && floorY < Main.maxTilesY - 250;
			}
			return floorY < Main.rockLayer;
		}
		public bool CanShowItemDropInUI() => true;
		public string GetConditionDescription() => null;
	}
	public class CavernsChest : IItemDropRuleCondition, IProvideItemConditionDescription
	{
		public bool CanDrop(DropAttemptInfo info)
		{
			if (info.chest is null)
				return false;
			if (UndergroundChest.IsUndergroundOrHigher(info.chest.y + 2))
				return false;
			return IsCavernsOrHigher(info.chest.x, info.chest.y);
		}
		internal static bool IsCavernsOrHigher(int chestX, int chestY)
		{
			if (WorldGen.remixWorldGen) {
				int style = TileObjectData.GetTileStyle(Main.tile[chestX, chestY]);
				return style == 7 || style == 14;
			}
			return chestY + 2 < Main.maxTilesY - 250;
		}
		public bool CanShowItemDropInUI() => true;
		public string GetConditionDescription() => null;
	}
	public class LavaLayerChest : IItemDropRuleCondition, IProvideItemConditionDescription
	{
		public bool CanDrop(DropAttemptInfo info)
		{
			if (info.chest is null)
				return false;
			int floorY = info.chest.y + 2;
			if (WorldGen.remixWorldGen) {
				return floorY > Main.worldSurface && floorY < Main.rockLayer;
			}
			return floorY > GenVars.lavaLine;
		}
		public bool CanShowItemDropInUI() => true;
		public string GetConditionDescription() => null;
	}
	public class IsChestType(int type, int style) : IItemDropRuleCondition, IProvideItemConditionDescription
	{
		public bool CanDrop(DropAttemptInfo info)
		{
			if (info.chest is null)
				return false;
			Tile tile = Main.tile[info.chest.x, info.chest.y];
			if (tile.type != type)
				return false;
			return TileObjectData.GetTileStyle(tile) == style;
		}
		public bool CanShowItemDropInUI() => true;
		public string GetConditionDescription() => null;
	}
	public class IsNotChestType(int type, int style) : IItemDropRuleCondition, IProvideItemConditionDescription
	{
		public bool CanDrop(DropAttemptInfo info)
		{
			if (info.chest is null)
				return true;
			Tile tile = Main.tile[info.chest.x, info.chest.y];
			if (tile.type != type)
				return true;
			return TileObjectData.GetTileStyle(tile) != style;
		}
		public bool CanShowItemDropInUI() => true;
		public string GetConditionDescription() => null;
	}
	public abstract class SavedOreTier(int value) : IItemDropRuleCondition, IProvideItemConditionDescription
	{
		public int Value => value;
		public abstract int Tier { get; }
		public bool CanDrop(DropAttemptInfo info) => Tier == Value;
		public bool CanShowItemDropInUI() => Tier == Value;
		public string GetConditionDescription() => null;
		public override int GetHashCode() => HashCode.Combine(GetType(), Value);
		public override bool Equals(object obj) => obj.GetType() == GetType() && ((SavedOreTier)obj).Value == value;
	}
	public class SavedOreTierCopper(int type) : SavedOreTier(type)
	{
		public override int Tier => WorldGen.SavedOreTiers.Copper;
	}
	public class SavedOreTierIron(int type) : SavedOreTier(type)
	{
		public override int Tier => WorldGen.SavedOreTiers.Iron;
	}
	public class SavedOreTierSilver(int type) : SavedOreTier(type)
	{
		public override int Tier => WorldGen.SavedOreTiers.Silver;
	}
	public class SavedOreTierGold(int type) : SavedOreTier(type)
	{
		public override int Tier => WorldGen.SavedOreTiers.Gold;
	}
}
