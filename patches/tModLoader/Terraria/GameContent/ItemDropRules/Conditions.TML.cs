namespace Terraria.GameContent.ItemDropRules
{
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
		public class NotTheBeesIsUp : IItemDropRuleCondition, IProvideItemConditionDescription
		{
			public bool CanDrop(DropAttemptInfo info) => Main.notTheBeesWorld;
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
		public class NotTheBeesIsNotUp : IItemDropRuleCondition, IProvideItemConditionDescription
		{
			public bool CanDrop(DropAttemptInfo info) => !Main.notTheBeesWorld;
			public bool CanShowItemDropInUI() => true;
			public string GetConditionDescription() => null;
		}
	}
}
