using System.Collections.Generic;

namespace Terraria.ModLoader;

public static class AchievementLoader
{
	private static readonly IList<ModAchievement> achievements = new List<ModAchievement>();
	public static int achievementCount;

	public static void Register(ModAchievement achievement)
	{
		achievement.Achievement.Type = achievementCount++;
		achievements.Add(achievement);
	}

	public static void Unregister(ModAchievement achievement)
	{
		if(achievements.Contains(achievement))
			achievements.Remove(achievement);
	}

	public static ModAchievement GetAchievement(int type)
	{
		return type >= 0 && type < achievementCount ? achievements[type] : null;
	}
}