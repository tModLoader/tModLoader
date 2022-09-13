using Terraria.Localization;

namespace Terraria.GameContent.UI.ResourceSets
{
	partial class ClassicPlayerResourcesDisplaySet
	{
		public static readonly ResourceSetSlotId<ClassicPlayerResourcesDisplaySet> Hearts = 0;
		public static readonly ResourceSetSlotId<ClassicPlayerResourcesDisplaySet> Stars = 1;

		public string DisplayedName => Language.GetTextValue("UI.HealthManaStyle_" + NameKey);
	}
}
