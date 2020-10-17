using Terraria.Localization;

namespace Terraria.ModLoader
{
	public class Melee : DamageClass
	{
		internal protected override string DisplayNameInternal => Language.GetTextValue("LegacyTooltip.2");
	}

	public class Ranged : DamageClass
	{
		internal protected override string DisplayNameInternal => Language.GetTextValue("LegacyTooltip.3");
	}

	public class Magic : DamageClass
	{
		internal protected override string DisplayNameInternal => Language.GetTextValue("LegacyTooltip.4");
	}

	public class Summon : DamageClass
	{
		internal protected override string DisplayNameInternal => Language.GetTextValue("LegacyTooltip.53");
	}
}
