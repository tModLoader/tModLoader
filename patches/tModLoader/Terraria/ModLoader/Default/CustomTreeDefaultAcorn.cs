using Terraria.Localization;

namespace Terraria.ModLoader.Default;

public class CustomTreeDefaultAcorn : CustomTreeAcorn
{
	public override string Name => Tree.Name + "Acorn";

	protected override bool CloneNewInstances => true;
	private LocalizedText displayName;
	private LocalizedText tooltip;

	public override LocalizedText DisplayName => displayName ?? DefaultDisplayNameLocalization;
	public override LocalizedText Tooltip => tooltip ?? DefaultTooltipLocalization;

	public CustomTreeDefaultAcorn(LocalizedText displayName, LocalizedText tooltip = null)
	{
		this.displayName = displayName;
		this.tooltip = tooltip;
	}
	public CustomTreeDefaultAcorn()
	{
	}
}
