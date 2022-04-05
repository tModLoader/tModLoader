using System.Collections.Generic;
using Terraria.ID;

namespace Terraria.ModLoader
{
	public class AchievementLoader : Loader<ModAchievement>
	{
		public AchievementLoader() {
			Initialize(AchievementID.Count);
		}
	}
}