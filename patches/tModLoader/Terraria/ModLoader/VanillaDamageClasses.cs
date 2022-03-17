using Terraria.ID;
using Terraria.Localization;

namespace Terraria.ModLoader
{
	[Autoload(false)]
	public abstract class VanillaDamageClass : DamageClass
	{
		internal protected override string DisplayNameInternal => Language.GetTextValue(LangKey).Substring(1);

		protected abstract string LangKey { get; }

		public override bool CheckClassEffectInheritance(DamageClass damageClass) => false;
	}

	public class DefaultDamageClass : VanillaDamageClass
	{
		protected override string LangKey => "LegacyTooltip.55";

		public override StatInheritanceData CheckBaseClassStatInheritance(DamageClass damageClass) => new StatInheritanceData(0f);
	}

	public class GenericDamageClass : VanillaDamageClass
	{
		protected override string LangKey => "LegacyTooltip.55";

		public override StatInheritanceData CheckBaseClassStatInheritance(DamageClass damageClass) => new StatInheritanceData(0f);

		public override void SetDefaultStats(Player player) {
			player.GetCritChance(this) = 4;
		}
	}

	public class MeleeDamageClass : VanillaDamageClass
	{
		protected override string LangKey => "LegacyTooltip.2";

		public override StatInheritanceData CheckBaseClassStatInheritance(DamageClass damageClass) => damageClass == Generic ? new StatInheritanceData() : new StatInheritanceData(0f);
	}

	public class RangedDamageClass : VanillaDamageClass
	{
		protected override string LangKey => "LegacyTooltip.3";

		public override StatInheritanceData CheckBaseClassStatInheritance(DamageClass damageClass) => damageClass == Generic ? new StatInheritanceData() : new StatInheritanceData(0f);
	}

	public class MagicDamageClass : VanillaDamageClass
	{
		protected override string LangKey => "LegacyTooltip.4";

		public override StatInheritanceData CheckBaseClassStatInheritance(DamageClass damageClass) => damageClass == Generic ? new StatInheritanceData() : new StatInheritanceData(0f);
	}

	public class SummonDamageClass : VanillaDamageClass
	{
		protected override string LangKey => "LegacyTooltip.53";

		public override StatInheritanceData CheckBaseClassStatInheritance(DamageClass damageClass) => damageClass == Generic ? new StatInheritanceData() : new StatInheritanceData(0f);

		public override StatInheritanceData? CheckDynamicClassStatInheritance(DamageClass damageClass, Player player, Item item) {
			if (ItemID.Sets.SummonerWeaponThatScalesWithAttackSpeed[item.type] && damageClass == Melee) {
				return new StatInheritanceData(0f, 0f, player.whipUseTimeMultiplier, player.whipUseTimeMultiplier, 0f, 0f, 0f);
			}
			return null;
		}

		public override bool AllowStandardCrits => false;

		public override bool ShowStatTooltipLine(Player player, string lineName) => lineName != "CritChance" && lineName != "Speed";
	}

	public class ThrowingDamageClass : VanillaDamageClass
	{
		protected override string LangKey => "LegacyTooltip.58";

		public override StatInheritanceData CheckBaseClassStatInheritance(DamageClass damageClass) => damageClass == Generic ? new StatInheritanceData() : new StatInheritanceData(0f);
	}
}