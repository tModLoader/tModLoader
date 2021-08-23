using Terraria.Localization;

namespace Terraria.Achievements
{
	partial class Achievement
	{
		public const string VanillaTextureIndicator = "vanilla";

		// Override this so we can set it manually.
		public override string Name { get; }

		// Always globally saved.
		public override AchievementType SaveType => AchievementType.Global;

		// Override these to replace them with the expected vanilla translations.
		public override string FriendlyName => Language.GetTextValue("Achievements." + Name + "_Name");

		public override string Description => Language.GetTextValue("Achievements." + Name + "_Description");

		public override string Texture => VanillaTextureIndicator;
	}
}