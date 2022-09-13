using Terraria.Localization;

namespace Terraria.GameContent.UI.ResourceSets
{
	partial class FancyClassicPlayerResourcesDisplaySet
	{
		public static readonly ResourceSetSlotId<FancyClassicPlayerResourcesDisplaySet> HeartPanelsFirstRow = 0;
		public static readonly ResourceSetSlotId<FancyClassicPlayerResourcesDisplaySet> HeartPanelsSecondRow = 1;
		public static readonly ResourceSetSlotId<FancyClassicPlayerResourcesDisplaySet> HeartsFirstRow = 2;
		public static readonly ResourceSetSlotId<FancyClassicPlayerResourcesDisplaySet> HeartsSecondRow = 3;
		public static readonly ResourceSetSlotId<FancyClassicPlayerResourcesDisplaySet> StarPanels = 4;
		public static readonly ResourceSetSlotId<FancyClassicPlayerResourcesDisplaySet> Stars = 5;

		private PlayerStatsSnapshot preparedSnapshot;

		public string DisplayedName => Language.GetTextValue("UI.HealthManaStyle_" + NameKey);
	}
}
