﻿using Terraria.Localization;

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

		protected override float CheckClassStatInheritance(DamageClass damageClass) => 0;
	}

	public class GenericDamageClass : VanillaDamageClass
	{
		protected override string LangKey => "LegacyTooltip.55";

		protected override float CheckClassStatInheritance(DamageClass damageClass) => 0;

		public override void SetDefaultStats(Player player)
		{
			player.GetCritChance(this) = 4;
		}
	}

	public class MeleeDamageClass : VanillaDamageClass
	{
		protected override string LangKey => "LegacyTooltip.2";

		protected override float CheckClassStatInheritance(DamageClass damageClass) => damageClass == Generic ? 1f : 0f;
	}

	public class RangedDamageClass : VanillaDamageClass
	{
		protected override string LangKey => "LegacyTooltip.3";

		protected override float CheckClassStatInheritance(DamageClass damageClass) => damageClass == Generic ? 1f : 0f;
	}

	public class MagicDamageClass : VanillaDamageClass
	{
		protected override string LangKey => "LegacyTooltip.4";

		protected override float CheckClassStatInheritance(DamageClass damageClass) => damageClass == Generic ? 1f : 0f;
	}

	public class SummonDamageClass : VanillaDamageClass
	{
		protected override string LangKey => "LegacyTooltip.53";

		protected override float CheckClassStatInheritance(DamageClass damageClass) => damageClass == Generic ? 1f : 0f;

		public override bool AllowStandardCrits => false;

		public override bool ShowStatTooltipLine(Player player, string lineName) => lineName != "CritChance" && lineName != "Speed";
	}

	public class ThrowingDamageClass : VanillaDamageClass
	{
		protected override string LangKey => "LegacyTooltip.58";

		protected override float CheckClassStatInheritance(DamageClass damageClass) => damageClass == Generic ? 1f : 0f;
	}
}