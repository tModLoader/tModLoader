using System;
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
			Type = LoaderManager.Get<AchievementLoader>().Register(this);

			if (Type != Achievement.Id) {
				throw new Exception($"ModAchievement \"{Name}\" was registered with a type of \"{Type}\", but the associated Achievement had an ID of \"{Achievement.Id}\"!" +
				                    "\nAchievements should not be instantiated manually as it causes issues with IDs.");
			}
		}

		public sealed override void SetupContent() => SetStaticDefaults();
	}
}
