using Terraria.Localization;

namespace Terraria.ModLoader
{
	[Autoload(false)]
	public abstract class VanillaDamageClass : DamageClass
	{
		internal protected override string DisplayNameInternal => Language.GetTextValue(LangKey).Substring(1);

		protected abstract string LangKey { get; }
	}

	public class DefaultDamageClass : VanillaDamageClass
	{
		protected override string LangKey => "LegacyTooltip.55";

		public override StatInheritanceData GetModifierInheritance(DamageClass damageClass) => StatInheritanceData.None;
	}

	public class GenericDamageClass : VanillaDamageClass
	{
		protected override string LangKey => "LegacyTooltip.55";

		public override StatInheritanceData GetModifierInheritance(DamageClass damageClass) => StatInheritanceData.None;

		public override void SetDefaultStats(Player player) {
			player.GetCritChance(this) = 4;
		}
	}

	public class MeleeDamageClass : VanillaDamageClass
	{
		protected override string LangKey => "LegacyTooltip.2";
	}

	public class RangedDamageClass : VanillaDamageClass
	{
		protected override string LangKey => "LegacyTooltip.3";
	}

	public class MagicDamageClass : VanillaDamageClass
	{
		protected override string LangKey => "LegacyTooltip.4";
	}

	public class SummonDamageClass : VanillaDamageClass
	{
		protected override string LangKey => "LegacyTooltip.53";

		public override bool AllowStandardCrits => false;

		public override bool ShowStatTooltipLine(Player player, string lineName) => lineName != "CritChance" && lineName != "Speed";
	}

	public class SummonMeleeSpeedDamageClass : VanillaDamageClass
	{
		protected override string LangKey => "LegacyTooltip.53";

		public override StatInheritanceData GetModifierInheritance(DamageClass damageClass) {
			if (damageClass == Melee)
				return new StatInheritanceData(0f, 1f, 0f, 0f, 0f);

			if (damageClass == Generic || damageClass == Summon)
				return StatInheritanceData.Full;

			return StatInheritanceData.None;
		}

		public override bool AllowStandardCrits => false;

		public override bool ShowStatTooltipLine(Player player, string lineName) => lineName != "CritChance";
	}

	public class MagicSummonHybridDamageClass : VanillaDamageClass
	{
		protected override string LangKey => "magic or summon damage";

		public override StatInheritanceData GetModifierInheritance(DamageClass damageClass) {
			if (damageClass == Generic || damageClass == Magic || damageClass == Summon)
				return StatInheritanceData.Full;

			return StatInheritanceData.None;
		}
	}

	public class ThrowingDamageClass : VanillaDamageClass
	{
		protected override string LangKey => "LegacyTooltip.58";
	}
}