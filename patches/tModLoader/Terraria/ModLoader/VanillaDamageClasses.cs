using System.Collections.Generic;
using Terraria.Localization;

namespace Terraria.ModLoader
{
	public class Generic : DamageClass
	{
		internal protected override string DisplayNameInternal => Language.GetTextValue("LegacyTooltip.55").Substring(1);

		public override float BenefitsFrom(DamageClass damageClass) {
			return 0;
		}

		public override bool CountsAs(DamageClass damageClass) {
			return false;
		}
	}

	public class NoScaling : DamageClass
	{
		internal protected override string DisplayNameInternal => Language.GetTextValue("LegacyTooltip.55").Substring(1);

		public override float BenefitsFrom(DamageClass damageClass) {
			return 0;
		}

		public override bool CountsAs(DamageClass damageClass) {
			return false;
		}
	}

	public class Melee : DamageClass
	{
		internal protected override string DisplayNameInternal => Language.GetTextValue("LegacyTooltip.2").Substring(1);

		public override float BenefitsFrom(DamageClass damageClass) {
			return 0;
		}

		public override bool CountsAs(DamageClass damageClass) {
			return false;
		}
	}

	public class Ranged : DamageClass
	{
		internal protected override string DisplayNameInternal => Language.GetTextValue("LegacyTooltip.3").Substring(1);

		public override float BenefitsFrom(DamageClass damageClass) {
			return 0;
		}

		public override bool CountsAs(DamageClass damageClass) {
			return false;
		}
	}

	public class Magic : DamageClass
	{
		internal protected override string DisplayNameInternal => Language.GetTextValue("LegacyTooltip.4").Substring(1);

		public override float BenefitsFrom(DamageClass damageClass) {
			return 0;
		}

		public override bool CountsAs(DamageClass damageClass) {
			return false;
		}
	}

	public class Summon : DamageClass
	{
		internal protected override string DisplayNameInternal => Language.GetTextValue("LegacyTooltip.53").Substring(1);

		public override float BenefitsFrom(DamageClass damageClass) {
			return 0;
		}

		public override bool CountsAs(DamageClass damageClass) {
			return false;
		}
	}

	public class Throwing : DamageClass
	{
		internal protected override string DisplayNameInternal => Language.GetTextValue("LegacyTooltip.58").Substring(1);

		public override float BenefitsFrom(DamageClass damageClass) {
			return 0;
		}

		public override bool CountsAs(DamageClass damageClass) {
			return false;
		}
	}
}
