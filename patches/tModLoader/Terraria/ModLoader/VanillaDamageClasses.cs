using Terraria.Localization;

namespace Terraria.ModLoader
{
	public class Melee : DamageClass
	{
		public override void SetupContent() => ClassName = new ModTranslation("LegacyTooltip.2");
	}

	public class Ranged : DamageClass
	{
		public override void SetupContent() => ClassName = new ModTranslation("LegacyTooltip.3");
	}

	public class Magic : DamageClass
	{
		public override void SetupContent() => ClassName = new ModTranslation("LegacyTooltip.4");
	}

	public class Summon : DamageClass
	{
		public override void SetupContent() => ClassName = new ModTranslation("LegacyTooltip.53");
	}
}
