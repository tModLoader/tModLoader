using Terraria.Localization;
using Terraria.ModLoader;

namespace Terraria.Achievements
{
	public interface IAchievement
	{
		AchievementType SaveType { get; }

		LocalizedText FriendlyName { get; }

		LocalizedText Description { get; }

		AchievementCategory Category { get; }
	}
}