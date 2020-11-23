using System.Collections.Generic;
using Terraria.Localization;

namespace Terraria.ModLoader
{
	public class Generic : DamageClass
	{
		internal protected override string DisplayNameInternal => Language.GetTextValue("LegacyTooltip.55").Substring(1);

		public override Dictionary<DamageClass, float>? BenefitsFrom() {
			return null;
		}

		public override List<DamageClass>? CountsAs() {
			return null;
		}
	}

	public class NoScaling : DamageClass
	{
		internal protected override string DisplayNameInternal => Language.GetTextValue("LegacyTooltip.55").Substring(1);

		public override Dictionary<DamageClass, float>? BenefitsFrom() {
			return null;
		}

		public override List<DamageClass>? CountsAs() {
			return null;
		}
	}

	public class Melee : DamageClass
	{
		internal protected override string DisplayNameInternal => Language.GetTextValue("LegacyTooltip.2").Substring(1);

		public override Dictionary<DamageClass, float>? BenefitsFrom() {
			return null;
		}

		public override List<DamageClass>? CountsAs() {
			return null;
		}
	}

	public class Ranged : DamageClass
	{
		internal protected override string DisplayNameInternal => Language.GetTextValue("LegacyTooltip.3").Substring(1);

		public override Dictionary<DamageClass, float>? BenefitsFrom() {
			return null;
		}

		public override List<DamageClass>? CountsAs() {
			return null;
		}
	}

	public class Magic : DamageClass
	{
		internal protected override string DisplayNameInternal => Language.GetTextValue("LegacyTooltip.4").Substring(1);

		public override Dictionary<DamageClass, float>? BenefitsFrom() {
			return null;
		}

		public override List<DamageClass>? CountsAs() {
			return null;
		}
	}

	public class Summon : DamageClass
	{
		internal protected override string DisplayNameInternal => Language.GetTextValue("LegacyTooltip.53").Substring(1);

		public override Dictionary<DamageClass, float>? BenefitsFrom() {
			return null;
		}

		public override List<DamageClass>? CountsAs() {
			return null;
		}
	}

	public class Throwing : DamageClass
	{
		internal protected override string DisplayNameInternal => Language.GetTextValue("LegacyTooltip.58").Substring(1);

		public override Dictionary<DamageClass, float>? BenefitsFrom() {
			return null;
		}

		public override List<DamageClass>? CountsAs() {
			return null;
		}
	}
}
