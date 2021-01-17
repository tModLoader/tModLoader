using Terraria.Localization;

namespace Terraria.ModLoader
{
	public abstract class VanillaDamageClass : DamageClass
	{
		internal protected override string DisplayNameInternal => Language.GetTextValue(LangKey).Substring(1);

		protected abstract string LangKey { get; }

		protected override float GetBenefitsFrom(DamageClass damageClass) => 0;

		public override bool CountsAs(DamageClass damageClass) => false;
	}

	public class Generic : VanillaDamageClass
	{
		protected override string LangKey => "LegacyTooltip.55";
	}

	public class NoScaling : VanillaDamageClass
	{
		protected override string LangKey => "LegacyTooltip.55";
	}

	public class Melee : VanillaDamageClass
	{
		protected override string LangKey => "LegacyTooltip.2";
	}

	public class Ranged : VanillaDamageClass
	{
		protected override string LangKey => "LegacyTooltip.3";
	}

	public class Magic : VanillaDamageClass
	{
		protected override string LangKey => "LegacyTooltip.4";
	}

	public class Summon : VanillaDamageClass
	{
		protected override string LangKey => "LegacyTooltip.53";
	}

	public class Throwing : VanillaDamageClass
	{
		protected override string LangKey => "LegacyTooltip.58";
	}
}
