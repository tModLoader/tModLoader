using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using Terraria.Achievements;
using Terraria.Localization;

namespace Terraria.ModLoader
{
	public abstract class ModAchievement : ModType, IAchievement
	{
		/// <summary>
		///		Achievement saving method. Saved either globally or per-player.
		/// </summary>
		public abstract AchievementType SaveType { get; }

		public virtual LocalizedText FriendlyName => Language.GetText($"Mods.{Mod.Name}.AchievementName.{Name}");

		public virtual LocalizedText Description => Language.GetText($"Mods.{Mod.Name}.AchievementDescription.{Name}");

		protected ModTranslation DisplayName { get; private set; }

		protected ModTranslation DisplayDescription { get; private set; }

		public virtual AchievementCategory Category { get; set; } = AchievementCategory.None;

		public virtual IAchievementTracker Tracker { get; set; }

		public virtual bool IsCompleted { get; set; }

		protected sealed override void Register() {
			DisplayName = LocalizationLoader.GetOrCreateTranslation(Mod, $"AchievementName.{Name}");
			DisplayDescription = LocalizationLoader.GetOrCreateTranslation(Mod, $"AchievementDescription.{Name}");
		}

		public override void SetupContent() {
			SetStaticDefaults();
		}

		public virtual void ClearProgress() {
		}

		public virtual void Load(Dictionary<string, JObject> conditions) {
		}

		public virtual void AddCondition(AchievementCondition condition) {
		}

		public virtual AchievementCondition GetCondition(string conditionName) => null;
	}
}