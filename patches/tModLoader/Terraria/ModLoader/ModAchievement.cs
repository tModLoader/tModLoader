using Terraria.Achievements;

namespace Terraria.ModLoader
{
	public abstract class ModAchievement : ModTexturedType
	{
		public int Type { get; internal set; }

		public Achievement Achievement { get; internal set; }

		protected ModAchievement() {
			Achievement = new Achievement(Name) { ModAchievement = this };
		}

		protected sealed override void Register() {
			ModTypeLookup<ModAchievement>.Register(this);
			AchievementLoader.achievements.Add(this);
		}

		public sealed override void SetupContent() => SetStaticDefaults();
	}
}
