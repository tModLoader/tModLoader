using Terraria.Localization;

namespace Terraria.ModLoader;

[Autoload(false)]
public abstract class VanillaDamageClass : DamageClass
{
	public override LocalizedText DisplayName => Language.GetText(LangKey);

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

	public override void SetDefaultStats(Player player)
	{
		player.GetCritChance(this) = 4;
	}
}

public class MeleeDamageClass : VanillaDamageClass
{
	protected override string LangKey => "LegacyTooltip.2";
}

public class MeleeNoSpeedDamageClass : VanillaDamageClass
{
	protected override string LangKey => "LegacyTooltip.2";

	public override StatInheritanceData GetModifierInheritance(DamageClass damageClass)
	{
		if (damageClass == Generic || damageClass == Melee)
			return StatInheritanceData.Full with { attackSpeedInheritance = 0 };

		return StatInheritanceData.None;
	}

	public override bool GetEffectInheritance(DamageClass damageClass) => damageClass == Melee;
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

	public override bool UseStandardCritCalcs => false;

	public override bool ShowStatTooltipLine(Player player, string lineName) => lineName != "CritChance" && lineName != "Speed";
	public override bool GetPrefixInheritance(DamageClass damageClass) => damageClass == Magic;
}

public class SummonMeleeSpeedDamageClass : VanillaDamageClass
{
	protected override string LangKey => "LegacyTooltip.53";

	public override StatInheritanceData GetModifierInheritance(DamageClass damageClass)
	{
		if (damageClass == Melee)
			return new StatInheritanceData(attackSpeedInheritance: 1f);

		if (damageClass == Generic || damageClass == Summon)
			return StatInheritanceData.Full;

		return StatInheritanceData.None;
	}

	public override bool GetEffectInheritance(DamageClass damageClass) => damageClass == Summon;

	public override bool UseStandardCritCalcs => false;

	public override bool ShowStatTooltipLine(Player player, string lineName) => lineName != "CritChance";
	public override bool GetPrefixInheritance(DamageClass damageClass) => damageClass == Melee;
}

public class MagicSummonHybridDamageClass : VanillaDamageClass
{
	protected override string LangKey => "tModLoader.MagicSummonHybridDamageClass";

	public override StatInheritanceData GetModifierInheritance(DamageClass damageClass)
	{
		if (damageClass == Generic || damageClass == Magic || damageClass == Summon)
			return StatInheritanceData.Full;

		return StatInheritanceData.None;
	}

	public override bool GetEffectInheritance(DamageClass damageClass) => damageClass == Magic || damageClass == Summon;
}

public class ThrowingDamageClass : VanillaDamageClass
{
	protected override string LangKey => "LegacyTooltip.58";
	public override bool GetPrefixInheritance(DamageClass damageClass) => damageClass == Ranged;
}