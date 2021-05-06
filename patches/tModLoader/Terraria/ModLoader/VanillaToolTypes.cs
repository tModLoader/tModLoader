using Terraria.Localization;

namespace Terraria.ModLoader
{
	[Autoload(false)]
	public abstract class VanillaToolType : ToolType
	{
		internal protected override string DisplayNameInternal => Language.GetTextValue(LangKey);

		protected abstract string LangKey { get; }

		public override ToolPriority Priority => ToolPriority.MineBlock;

		public override bool AffectsBlocks => true;
	}

	public class ShovelToolType : VanillaToolType
	{
		protected override string LangKey => "LegacyTooltip.26";

		public override bool ShowsOnTooltip => false;

		public override ToolPriority Priority => ToolPriority.PreMineBlock;
	}

	public class PickaxeToolType : VanillaToolType
	{
		protected override string LangKey => "LegacyTooltip.26";

		public override string TooltipName => "PickPower";
	}

	public class AxeToolType : VanillaToolType
	{
		protected override string LangKey => "LegacyTooltip.27";

		public override string TooltipName => "AxePower";

		public override float TooltipToolPowerMultiplier => 5f;
	}

	public class HammerBlockToolType : VanillaToolType
	{
		protected override string LangKey => "LegacyTooltip.28";

		public override string TooltipName => "HammerPower";
	}

	public class HammerSlopeToolType : VanillaToolType
	{
		protected override string LangKey => "LegacyTooltip.28";

		public override ToolPriority Priority => ToolPriority.SlopeBlock;

		public override bool ShowsOnTooltip => false;

		public override ToolType? PowerProvider => ToolType.HammerBlock;
	}

	public class HammerWallToolType : VanillaToolType
	{
		protected override string LangKey => "LegacyTooltip.28";

		public override ToolPriority Priority => ToolPriority.HitWall;

		public override bool ShowsOnTooltip => false;

		public override bool AffectsBlocks => false;

		public override bool AffectsWalls => true;

		public override ToolType? PowerProvider => ToolType.HammerBlock;
	}
}
