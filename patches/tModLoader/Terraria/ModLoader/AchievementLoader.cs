using System.Collections.Generic;
using Terraria.Achievements;
using Terraria.ID;

namespace Terraria.ModLoader
{
	public class AchievementLoader : Loader<Achievement>
	{
		public AchievementLoader() {
			Initialize(AchievementID.Count);
		}
	}
}