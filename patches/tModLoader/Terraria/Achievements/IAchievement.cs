using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using Terraria.ModLoader;

namespace Terraria.Achievements
{
	public interface IAchievement : IModType
	{
		string Texture { get; }

		AchievementType SaveType { get; }

		string FriendlyName { get; }

		string Description { get; }

		AchievementCategory Category { get; set; }

		IAchievementTracker Tracker { get; }

		bool IsCompleted { get; }

		Dictionary<string, AchievementCondition> Conditions { get; set; }

		Achievement.AchievementCompleted OnCompleted { get; set; }

		void ClearProgress();

		void Load(Dictionary<string, JObject> conditions);

		void AddCondition(AchievementCondition condition);

		AchievementCondition GetCondition(string conditionName);
	}
}