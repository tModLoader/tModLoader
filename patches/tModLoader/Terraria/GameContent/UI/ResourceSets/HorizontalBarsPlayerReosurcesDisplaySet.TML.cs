using Terraria.Localization;

namespace Terraria.GameContent.UI.ResourceSets
{
	partial class HorizontalBarsPlayerReosurcesDisplaySet
	{
		public static readonly ResourceSetSlotId<HorizontalBarsPlayerReosurcesDisplaySet> LifePanels = 0;
		public static readonly ResourceSetSlotId<HorizontalBarsPlayerReosurcesDisplaySet> LifeBars = 1;
		public static readonly ResourceSetSlotId<HorizontalBarsPlayerReosurcesDisplaySet> ManaPanels = 2;
		public static readonly ResourceSetSlotId<HorizontalBarsPlayerReosurcesDisplaySet> ManaBars = 3;

		public string DisplayedName => Language.GetTextValue("UI.HealthManaStyle_" + NameKey);
	}
}
