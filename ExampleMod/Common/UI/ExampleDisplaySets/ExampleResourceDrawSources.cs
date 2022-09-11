using Terraria;
using Terraria.GameContent.UI.ResourceSets;
using Terraria.ModLoader;

namespace ExampleMod.Common.UI.ExampleDisplaySets
{
	internal class ExampleReversedPanelRight_Life : ResourceDrawSource_Life
	{
		public override IPlayerResourcesDisplaySet DisplaySet => Main.ResourceSetsManager.GetDisplaySet<ExampleReversedBarsDisplay>();

		public ExampleReversedPanelRight_Life(string context = null) : base(context) { }
	}

	internal class ExampleReversedPanelRight_Mana : ResourceDrawSource_Mana
	{
		public override IPlayerResourcesDisplaySet DisplaySet => Main.ResourceSetsManager.GetDisplaySet<ExampleReversedBarsDisplay>();

		public ExampleReversedPanelRight_Mana(string context = null) : base(context) { }
	}
}
