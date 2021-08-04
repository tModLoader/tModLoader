using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using Terraria.Localization;

namespace Terraria.Achievements
{
	public interface IAchievement
	{
		AchievementType SaveType { get; }

		LocalizedText FriendlyName { get; }

		LocalizedText Description { get; }

		AchievementCategory Category { get; }

		IAchievementTracker Tracker { get; }

		bool IsCompleted { get; }

		void ClearProgress();

		void Load(Dictionary<string, JObject> conditions);

		void AddCondition(AchievementCondition condition);

		AchievementCondition GetCondition(string conditionName);
	}
}