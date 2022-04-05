using System.Linq;
using Terraria.Achievements;
using Terraria.ID;

namespace Terraria.ModLoader
{
	public class AchievementLoader : Loader<Achievement>
	{
		public AchievementLoader() {
			Initialize(AchievementID.Count);
		}

		internal override void Unload() {
			base.Unload();

			// Clear modded achievements.
			var moddedKeys = Main.Achievements._achievements.Where(x =>!x.Value.IsModded()).Select(x => x.Key);

			foreach (string key in moddedKeys)
				Main.Achievements._achievements.Remove(key);

			// Reset the base achievement ID to the vanilla count.
			Achievement._totalAchievements = AchievementID.Count;
		}
	}
}