using System.Collections.Generic;
using Terraria.ID;

namespace Terraria.ModLoader
{
	public static class AchievementLoader
	{
		public static int AchievementCount => AchievementID.Count + achievements.Count;
		internal static readonly List<ModAchievement> achievements = new List<ModAchievement>();
	}
}