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
	}
}
