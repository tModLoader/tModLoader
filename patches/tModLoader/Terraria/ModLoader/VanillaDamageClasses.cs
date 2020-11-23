using Terraria.Localization;

namespace Terraria.ModLoader
{
	public class Melee : DamageClass
	{
		internal protected override string DisplayNameInternal => Language.GetTextValue("LegacyTooltip.2").Substring(1);
	}

	public class Ranged : DamageClass
	{
		internal protected override string DisplayNameInternal => Language.GetTextValue("LegacyTooltip.3").Substring(1);
	}

	public class Magic : DamageClass
	{
		internal protected override string DisplayNameInternal => Language.GetTextValue("LegacyTooltip.4").Substring(1);
	}

	public class Summon : DamageClass
	{
		internal protected override string DisplayNameInternal => Language.GetTextValue("LegacyTooltip.53").Substring(1);
	}

	public class Throwing : DamageClass
	{
		internal protected override string DisplayNameInternal => Language.GetTextValue("LegacyTooltip.58").Substring(1);
	}
}
