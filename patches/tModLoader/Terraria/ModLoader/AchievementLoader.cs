using System.Collections.Generic;
using Terraria.Achievements;
using Terraria.Initializers;
using Terraria.UI;

namespace Terraria.ModLoader
{
	public class AchievementLoader : ILoader
	{
		private static readonly List<ModAchievement> _achievementsToLoad = new();

		internal static void AddAchievement(ModAchievement achievement) {
			_achievementsToLoad.Add(achievement); // Main.Achievements.Register(achievement);
			ModTypeLookup<ModAchievement>.Register(achievement);
		}

		void ILoader.ResizeArrays() {
			Main.instance._achievements = new AchievementManager();
			Main.instance._achievementAdvisor = new AchievementAdvisor();
			AchievementInitializer.Load();

			foreach (ModAchievement achievement in _achievementsToLoad)
				Main.Achievements.Register(achievement);
		}

		void ILoader.Unload() {
			Main.Achievements.Save();
			_achievementsToLoad.Clear();
		}
	}
}